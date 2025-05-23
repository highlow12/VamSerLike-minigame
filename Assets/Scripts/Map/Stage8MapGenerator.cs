using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

public class Stage8MapGenera : MonoBehaviour
{
    // 블록 프리팹 리스트 (Inspector에서 할당, 롱블럭/정사각형 구분 없이 하나의 리스트)
    public List<GameObject> blockPrefabs; // 다양한 형태의 블럭 프리팹을 모두 포함

    // 이미 배치된 위치 관리 (0.5 단위)
    private HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();

    // 플레이어 Transform (Inspector에서 할당)
    public Transform player;

    // 셀(미로 구역) 배치 여부 관리 (셀의 중심 좌표)
    private HashSet<Vector2Int> spawnedCells = new HashSet<Vector2Int>();

    // 셀 크기(유니티 유닛, 프리팹 크기에 따라 동적으로 결정)
    [Tooltip("셀(블럭 그룹) 간 최소 거리 (유니티 유닛)")]
    public float cellSpacing = 2.0f; // 유니티 유닛 기준
    [Tooltip("플레이어와 이 거리(유니티 유닛) 이상 떨어진 셀만 생성")]
    public float spawnDistance = 8.0f; // 유니티 유닛 기준
    [Tooltip("셀 생성 시 오프셋 (유니티 유닛, 셀 간 거리 보정)")]
    public float cellOffset = 2.0f; // 유니티 유닛 기준
    [Tooltip("플레이어 주변 셀 검사 범위 (예: 4면 9x9)")]
    public int cellCheckRange = 4;
    [Tooltip("격자 셀 간격 (유니티 유닛, n*n마다 생성)")]
    public float cellGridStep = 1.0f;
    [Tooltip("카메라(플레이어)로부터 이 거리 이상 멀어지면 오브젝트 풀로 반환 (유니티 유닛)")]
    public float cullingDistance = 20f;

    // 셀 생성/컬링 주기 (초)
    [Tooltip("셀 생성/컬링 연산 주기 (초, 0이면 매 프레임)")]
    public float updateInterval = 0.1f;
    private float updateTimer = 0f;

    // 오브젝트 풀: 프리팹별로 관리
    private List<ObjectPool<GameObject>> blockPools = new List<ObjectPool<GameObject>>();
    // 셀 인덱스별 활성화된 블록 오브젝트 관리
    private Dictionary<Vector2Int, GameObject> activeBlocks = new Dictionary<Vector2Int, GameObject>();

    // 컬링용 임시 리스트를 멤버로 두고 재사용하여 GC 최소화
    private List<Vector2Int> toRelease = new List<Vector2Int>();

    // 프리팹별 사이즈 캐싱 (Awake에서 계산)
    private List<Vector2> cachedBlockSizes = new List<Vector2>();

    private Vector2 GetCellSize()
    {
        if (cachedBlockSizes == null || cachedBlockSizes.Count == 0) return new Vector2(3f, 3f); // 기본값
        float maxX = 0f, maxY = 0f;
        foreach (var size in cachedBlockSizes)
        {
            if (size.x > maxX) maxX = size.x;
            if (size.y > maxY) maxY = size.y;
        }
        // 셀 크기는 가장 큰 프리팹의 크기 + cellSpacing (모두 유니티 유닛 기준)
        return new Vector2(maxX + cellSpacing, maxY + cellSpacing);
    }

    // 셀 인덱스 계산 유틸리티 (Floor 방식으로 변경)
    private Vector2Int WorldToCell(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellGridStep),
            Mathf.FloorToInt(worldPos.y / cellGridStep)
        );
    }
    private Vector2 CellToWorld(Vector2Int cellIdx)
    {
        return new Vector2(cellIdx.x * cellGridStep, cellIdx.y * cellGridStep);
    }

    private void Awake()
    {
        // 프리팹별 오브젝트 풀 생성
        blockPools.Clear();
        cachedBlockSizes.Clear();
        if (blockPrefabs != null)
        {
            foreach (var prefab in blockPrefabs)
            {
                var pool = new ObjectPool<GameObject>(
                    () => Instantiate(prefab, transform),
                    obj => obj.SetActive(true),
                    obj => obj.SetActive(false),
                    obj => Destroy(obj),
                    false, 10, 100
                );
                blockPools.Add(pool);

                // 사이즈 캐싱
                Vector2 size = new Vector2(3f, 3f); // 기본값
                var renderer = prefab.GetComponent<Renderer>();
                if (renderer != null)
                {
                    size = new Vector2(renderer.bounds.size.x, renderer.bounds.size.y);
                }
                else
                {
                    var collider = prefab.GetComponent<Collider2D>();
                    if (collider != null)
                        size = new Vector2(collider.bounds.size.x, collider.bounds.size.y);
                }
                cachedBlockSizes.Add(size);
            }
        }
    }

    private void Start() { /* 초기 맵 생성 없음 */ }

    private void Update()
    {
        if (updateInterval > 0f)
        {
            updateTimer += Time.deltaTime;
            if (updateTimer < updateInterval) return;
            updateTimer = 0f;
        }
        if (player == null) return;
        Vector2 cellSize = GetCellSize();
        Vector2 playerPos = new Vector2(player.position.x, player.position.y);
        Vector2Int snappedPlayerCell = WorldToCell(playerPos);
        // 셀 생성 및 활성화
        for (int dx = -cellCheckRange; dx <= cellCheckRange; dx++)
        {
            for (int dy = -cellCheckRange; dy <= cellCheckRange; dy++)
            {
                Vector2Int cellIdx = new Vector2Int(snappedPlayerCell.x + dx, snappedPlayerCell.y + dy);
                if (cellIdx.x == 0 && cellIdx.y == 0) continue;
                float dist = Vector2.Distance(CellToWorld(cellIdx), playerPos);
                if (dist > cullingDistance) continue; // 컬링 거리 밖은 생성하지 않음
                if (!activeBlocks.ContainsKey(cellIdx))
                {
                    Vector2 cellCenter = CellToWorld(cellIdx);
                    int rot = Random.Range(0, 4) * 90;
                    PlaceClosedCellWithPooling(cellIdx, cellCenter, rot);
                }
            }
        }
        // 컬링: 컬링 거리 밖의 블록은 풀로 반환
        toRelease.Clear();
        foreach (var kv in activeBlocks)
        {
            Vector2 cellWorld = CellToWorld(kv.Key);
            float dist = Vector2.Distance(cellWorld, playerPos);
            if (dist > cullingDistance)
            {
                ReleaseBlockToPool(kv.Key);
                toRelease.Add(kv.Key);
            }
        }
        foreach (var idx in toRelease)
            activeBlocks.Remove(idx);
    }

    // 오브젝트 풀에서 꺼내어 배치
    private void PlaceClosedCellWithPooling(Vector2Int cellIdx, Vector2 cellCenter, int rotationDegree)
    {
        if (blockPrefabs == null || blockPrefabs.Count == 0 || blockPools.Count != blockPrefabs.Count) return;
        int prefabIdx = Random.Range(0, blockPrefabs.Count);
        var pool = blockPools[prefabIdx];
        GameObject block = pool.Get();
        block.transform.position = new Vector3(cellCenter.x, cellCenter.y, 0);
        block.transform.rotation = Quaternion.Euler(0, 0, rotationDegree);
        block.transform.SetParent(transform);
        activeBlocks[cellIdx] = block;
    }

    // 오브젝트를 풀로 반환
    private void ReleaseBlockToPool(Vector2Int cellIdx)
    {
        if (!activeBlocks.ContainsKey(cellIdx)) return;
        GameObject block = activeBlocks[cellIdx];
        // 어떤 풀에 속하는지 찾기 (프리팹별로)
        for (int i = 0; i < blockPrefabs.Count; i++)
        {
            if (block.name.StartsWith(blockPrefabs[i].name))
            {
                blockPools[i].Release(block);
                return;
            }
        }
        // 못 찾으면 그냥 비활성화
        block.SetActive(false);
    }
}
