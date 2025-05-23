using UnityEngine;
using System.Collections.Generic;

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

    private Vector2 GetCellSize()
    {
        if (blockPrefabs == null || blockPrefabs.Count == 0) return new Vector2(3f, 3f); // 기본값
        float maxX = 0f, maxY = 0f;
        foreach (var prefab in blockPrefabs)
        {
            var renderer = prefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                var size = renderer.bounds.size;
                if (size.x > maxX) maxX = size.x;
                if (size.y > maxY) maxY = size.y;
            }
            else
            {
                var collider = prefab.GetComponent<Collider2D>();
                if (collider != null)
                {
                    var size = collider.bounds.size;
                    if (size.x > maxX) maxX = size.x;
                    if (size.y > maxY) maxY = size.y;
                }
            }
        }
        // 셀 크기는 가장 큰 프리팹의 크기 + cellSpacing (모두 유니티 유닛 기준)
        return new Vector2(maxX + cellSpacing, maxY + cellSpacing);
    }

    private void Start() { /* 초기 맵 생성 없음 */ }

    private void Update()
    {
        if (player == null) return;
        Vector2 cellSize = GetCellSize();
        Vector2 playerPos = new Vector2(player.position.x, player.position.y);
        Vector2Int snappedPlayerCell = new Vector2Int(
            Mathf.RoundToInt(playerPos.x / cellGridStep),
            Mathf.RoundToInt(playerPos.y / cellGridStep)
        );
        for (int dx = -cellCheckRange; dx <= cellCheckRange; dx++)
        {
            for (int dy = -cellCheckRange; dy <= cellCheckRange; dy++)
            {
                Vector2Int cellIdx = new Vector2Int(snappedPlayerCell.x + dx, snappedPlayerCell.y + dy);
                // (0,0) 위치는 생성하지 않음
                if (cellIdx.x == 0 && cellIdx.y == 0) continue;
                if (!spawnedCells.Contains(cellIdx))
                {
                    Vector2 cellCenter = new Vector2(cellIdx.x * cellGridStep, cellIdx.y * cellGridStep);
                    int rot = Random.Range(0, 4) * 90;
                    PlaceClosedCell(cellCenter, 6, 6, rot);
                    spawnedCells.Add(cellIdx);
                }
            }
        }
    }

    // 0.5 단위 위치로 스냅
    private Vector2 SnapToGrid(Vector2 pos)
    {
        float x = Mathf.Round(pos.x * 2f) / 2f;
        float y = Mathf.Round(pos.y * 2f) / 2f;
        return new Vector2(x, y);
    }

    // 블록 배치 함수 (겹침 방지, 회전 지원, 2D xy축 위치, 프리팹 랜덤 선택, 거리 조건 적용)
    public bool PlaceBlock(Vector2 pos, Vector2 size, Quaternion rotation)
    {
        Vector2 snappedPos = SnapToGrid(pos);
        // 블록이 차지할 모든 0.5 단위 위치 체크 (회전은 90도 단위만 지원)
        for (float dx = 0; dx < size.x; dx += 0.5f)
        {
            for (float dy = 0; dy < size.y; dy += 0.5f)
            {
                Vector2 offset2 = new Vector2(dx, dy);
                Vector2 rotatedOffset2 = Rotate(offset2, (int)rotation.eulerAngles.z);
                Vector2 checkPos = snappedPos + rotatedOffset2;
                // 거리 조건: 기존 블록과 2유닛 이상 떨어져야 함
                foreach (var occ in occupiedPositions)
                {
                    if (Vector2.Distance(occ, checkPos) < 2.0f)
                        return false;
                }
            }
        }
        // 프리팹 리스트에서 랜덤 선택
        if (blockPrefabs == null || blockPrefabs.Count == 0) return false;
        GameObject prefabToUse = blockPrefabs[Random.Range(0, blockPrefabs.Count)];
        // 배치 (z=0으로 고정)
        Instantiate(prefabToUse, new Vector3(snappedPos.x, snappedPos.y, 0), rotation, transform);
        // 위치 등록
        for (float dx = 0; dx < size.x; dx += 0.5f)
        {
            for (float dy = 0; dy < size.y; dy += 0.5f)
            {
                Vector2 offset2 = new Vector2(dx, dy);
                Vector2 rotatedOffset2 = Rotate(offset2, (int)rotation.eulerAngles.z);
                Vector2 regPos = snappedPos + rotatedOffset2;
                occupiedPositions.Add(regPos);
            }
        }
        return true;
    }

    private Vector2 Rotate(Vector2 point, int degree)
    {
        switch (degree % 360)
        {
            case 90:  return new Vector2(-point.y, point.x);
            case 180: return new Vector2(-point.x, -point.y);
            case 270: return new Vector2(point.y, -point.x);
            default:  return point;
        }
    }

    // 셀(미로 구역) 생성: 셀당 하나의 블럭만 생성
    public bool PlaceClosedCell(Vector2 origin, int width, int height, int rotationDegree = 0)
    {
        Quaternion rot = Quaternion.Euler(0, 0, rotationDegree); // z축 회전
        // 셀의 중심에 blockPrefabs에서 랜덤으로 하나 배치
        Vector2 cellCenter = origin;
        Vector2 blockSize = new Vector2(1f, 1f); // 기본값, 프리팹마다 다를 수 있음
        if (blockPrefabs != null && blockPrefabs.Count > 0)
        {
            // 프리팹의 실제 크기 사용 및 회전 랜덤 적용
            var prefab = blockPrefabs[Random.Range(0, blockPrefabs.Count)];
            var renderer = prefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                var size = renderer.bounds.size;
                blockSize = new Vector2(size.x, size.y);
            }
            else
            {
                var collider = prefab.GetComponent<Collider2D>();
                if (collider != null)
                {
                    var size = collider.bounds.size;
                    blockSize = new Vector2(size.x, size.y);
                }
            }
            // 프리팹마다 회전 랜덤 적용 (변수명 충돌 방지)
            int rotDeg = Random.Range(0, 4) * 90;
            Quaternion blockRot = Quaternion.Euler(0, 0, rotDeg);
            return PlaceBlock(cellCenter, blockSize, blockRot);
        }
        return PlaceBlock(cellCenter, blockSize, Quaternion.identity);
    }
}
