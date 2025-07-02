using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MasterOfCloset : BossMonster
{
    public Animator animator { get; private set; }
    public new float attackRange = 2f; // 공격 사거리
    public float hideTime = 2f; // 숨어있는 시간
    public float appearTime = 3f; // 나타나는 데 걸리는 시간
    
    [Header("Appearance Settings")]
    public int numberOfRays = 36; // 레이캐스트 방향 수
    public float raycastDistance = 20f; // 레이캐스트 최대 거리

    private List<Transform> appearancePoints = new List<Transform>(); // 나타날 수 있는 위치 목록
    private HashSet<Transform> foundPoints = new HashSet<Transform>(); // 중복 방지를 위한 재사용 HashSet
    private RaycastHit2D[] raycastHits = new RaycastHit2D[20]; // RaycastNonAlloc을 위한 재사용 배열
    private float stateTimer = 0f;
    private MasterOfClosetAction currentAction;
    private Transform lastAppearancePoint;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public enum MasterOfClosetAction
    {
        Hide,
        Appear,
        Attack
    }

    protected override void Start()
    {
        base.Start();
        // SearchForAppearancePoints(); // 이제 나타날 때마다 호출하므로 시작 시에는 필요 없습니다.
    }

    protected override void initState()
    {
        animator = GetComponent<Animator>();
        states = new Dictionary<int, BaseState>
        {
            { (int)MasterOfClosetAction.Hide, new MasterOfClosetHide(this) },
            { (int)MasterOfClosetAction.Appear, new MasterOfClosetAppear(this) },
            { (int)MasterOfClosetAction.Attack, new MasterOfClosetAttack(this) }
        };
        ChangeState(MasterOfClosetAction.Hide);
    }

    protected override void checkeState()
    {
        // Hide 상태일 때만 타이머를 사용하여 Appear 상태로 전환합니다.
        // 다른 상태 전환은 애니메이션 이벤트로 처리됩니다.
        if (currentAction == MasterOfClosetAction.Hide)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
            {
                ChangeState(MasterOfClosetAction.Appear);
            }
        }
    }

    public void ChangeState(MasterOfClosetAction nextAction)
    {
        currentAction = nextAction;
        if (monsterFSM == null)
        {
            monsterFSM = new MonsterFSM(states[(int)nextAction]);
        }
        else
        {
            monsterFSM.ChangeState(states[(int)nextAction]);
        }
        
        switch (nextAction)
        {
            case MasterOfClosetAction.Hide:
                stateTimer = hideTime;
                break;
            case MasterOfClosetAction.Appear:
            case MasterOfClosetAction.Attack:
                // 이 상태들의 전환은 애니메이션 이벤트가 처리하므로 타이머를 설정하지 않습니다.
                break;
        }
    }

    public Transform GetRandomAppearancePoint()
    {
        if (appearancePoints.Count == 0) return null;

        if (appearancePoints.Count == 1)
        {
            lastAppearancePoint = appearancePoints[0];
            return appearancePoints[0];
        }

        // LINQ를 사용하여 마지막 위치를 제외한 목록을 만듭니다.
        var availablePoints = appearancePoints.Where(p => p != lastAppearancePoint).ToList();
        
        // 만약 필터링된 목록이 비어있다면 (모든 포인트가 마지막 위치와 동일한 경우 등)
        // 전체 목록에서 다시 선택하도록 합니다.
        if (availablePoints.Count == 0)
        {
            availablePoints = appearancePoints;
        }

        // 새 목록에서 무작위로 하나를 선택합니다.
        Transform nextPoint = availablePoints[Random.Range(0, availablePoints.Count)];

        lastAppearancePoint = nextPoint;
        return nextPoint;
    }
    
    // --- Animation Events Start ---

    /// <summary>
    /// 'Appear' 애니메이션의 끝에서 호출될 애니메이션 이벤트입니다.
    /// 나타난 후, 플레이어와의 거리에 따라 공격 또는 숨기 상태로 전환합니다.
    /// </summary>
    public void OnAppearAnimationEnd()
    {
        if (playerTransform == null) 
        {
            ChangeState(MasterOfClosetAction.Hide);
            return;
        }

        if (Vector2.Distance(transform.position, playerTransform.position) < attackRange)
        {
            ChangeState(MasterOfClosetAction.Attack);
        }
        else
        {
            ChangeState(MasterOfClosetAction.Hide);
        }
    }

    /// <summary>
    /// 'Attack' 애니메이션의 끝에서 호출될 애니메이션 이벤트입니다.
    /// 공격 후에는 항상 숨기 상태로 전환합니다.
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        ChangeState(MasterOfClosetAction.Hide);
    }

    // --- Animation Events End ---

    /// <summary>
    /// 주어진 블록의 가장자리 중 플레이어와 가장 가까운 지점에 보스를 위치시키고, 해당 면을 바라보도록 회전시킵니다.
    /// </summary>
    /// <param name="blockTransform">대상 블록의 Transform</param>
    public void SetPositionOnEdge(Transform blockTransform)
    {
        if (blockTransform == null) return;

        BoxCollider2D blockCollider = blockTransform.GetComponent<BoxCollider2D>();
        if (blockCollider == null)
        {
            transform.position = blockTransform.position;
            return;
        }

        Vector2 playerPos = GameManager.Instance.playerScript.transform.position;

        // Get collider vertices in world space
        Vector2 center = blockCollider.offset;
        Vector2 size = blockCollider.size;
        Vector2 halfSize = size / 2;

        Vector2[] localVertices = new Vector2[4];
        localVertices[0] = center + new Vector2(-halfSize.x, -halfSize.y);
        localVertices[1] = center + new Vector2(halfSize.x, -halfSize.y);
        localVertices[2] = center + new Vector2(halfSize.x, halfSize.y);
        localVertices[3] = center + new Vector2(-halfSize.x, halfSize.y);

        Vector2[] worldVertices = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            worldVertices[i] = blockTransform.TransformPoint(localVertices[i]);
        }

        Vector2 bestPosition = Vector2.zero;
        Vector2 bestNormal = Vector2.zero;
        float minDistanceSq = float.MaxValue;

        // Find the closest point on each edge to the player
        for (int i = 0; i < 4; i++)
        {
            Vector2 p1 = worldVertices[i];
            Vector2 p2 = worldVertices[(i + 1) % 4];

            Vector2 edge = p2 - p1;
            Vector2 playerToP1 = playerPos - p1;

            float t = Vector2.Dot(playerToP1, edge) / edge.sqrMagnitude;
            t = Mathf.Clamp01(t);

            Vector2 closestPointOnEdge = p1 + t * edge;
            float distSq = (playerPos - closestPointOnEdge).sqrMagnitude;

            if (distSq < minDistanceSq)
            {
                minDistanceSq = distSq;
                bestPosition = p1 + edge * 0.5f; // 가장 가까운 변의 중앙 지점을 저장
                
                // Calculate outward normal
                Vector2 edgeNormal = new Vector2(-edge.y, edge.x).normalized;
                Vector2 centerToEdge = (bestPosition - (Vector2)blockTransform.position).normalized;
                if (Vector2.Dot(edgeNormal, centerToEdge) < 0)
                {
                    edgeNormal = -edgeNormal;
                }
                bestNormal = edgeNormal;
            }
        }

        // Set boss position and rotation
        Vector3 newPosition = bestPosition;
        newPosition.z = blockTransform.position.z - 0.1f; // 블록보다 약간 앞에 위치하도록 Z값 조정
        transform.position = newPosition;

        // 플레이어를 바라보는 방향 계산
        Vector2 directionToPlayer = (Vector2)GameManager.Instance.playerScript.transform.position - (Vector2)transform.position;
        float angleToPlayer = Vector2.SignedAngle(Vector2.right, directionToPlayer.normalized);

        // 가장 가까운 90도 각도로 스냅
        float snappedAngle = Mathf.Round(angleToPlayer / 90f) * 90f;

        transform.rotation = Quaternion.Euler(0, 0, snappedAngle);
    }

    public void Show()
    {
        if(spriteRenderer) spriteRenderer.enabled = true;
        if(col) col.enabled = true;
    }

    public void Hide()
    {
        if(spriteRenderer) spriteRenderer.enabled = false;
        if(col) col.enabled = false;
    }

    public void SearchForAppearancePoints()
    {
        Debug.Log("MasterOfCloset: Searching for appearance points...");
        appearancePoints.Clear();
        foundPoints.Clear();

        if (GameManager.Instance == null || GameManager.Instance.playerScript == null)
        {
            Debug.LogWarning("MasterOfCloset: Player not found, cannot search for appearance points.");
            return;
        }

        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = i * (360f / numberOfRays);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            Vector3 playerPosition = GameManager.Instance.playerScript.transform.position;

            // 레이 시작 위치를 플레이어 위치에서 0.7f 떨어진 곳으로 조정
            Vector3 rayStartPosition = playerPosition + (Vector3)direction * 0.7f;

            RaycastHit2D[] hits = Physics2D.RaycastAll(rayStartPosition, direction, raycastDistance - 0.7f);

            // LINQ를 사용하여 가장 가까운 'Block' 태그를 가진 부모의 첫 번째 자식 transform을 찾습니다.
            var firstBlockHit = hits
                .OrderBy(h => h.distance)
                .FirstOrDefault(h => h.collider.transform.parent != null && h.collider.transform.parent.CompareTag("Block"));

            if (firstBlockHit.collider != null)
            {
                foundPoints.Add(firstBlockHit.collider.transform);
            }
        }
        
        // HashSet에서 List로 복사
        appearancePoints.AddRange(foundPoints);
    }

    private void OnDrawGizmos()
    {
        if (appearancePoints == null || appearancePoints.Count == 0)
            return;

        Gizmos.color = Color.green;
        foreach (var point in appearancePoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f); // 유효 지점에 원 그리기
            }
        }
    }
}

// 숨기 상태
public class MasterOfClosetHide : BaseState
{
    private MasterOfCloset boss;

    public MasterOfClosetHide(MasterOfCloset boss) : base(boss)
    {
        this.boss = boss;
    }

    public override void OnStateEnter()
    {
        // 보스 비활성화 또는 모습 감추기
        boss.Hide();
    }

    public override void OnStateExit()
    {
        // 여기서 Show()를 호출하지 않습니다.
    }

    public override void OnStateUpdate() { }
}

// 나타나기 상태
public class MasterOfClosetAppear : BaseState
{
    private MasterOfCloset boss;

    public MasterOfClosetAppear(MasterOfCloset boss) : base(boss)
    {
        this.boss = boss;
    }

    public override void OnStateEnter()
    {
        // 나타날 때마다 항상 접근 가능한 지점을 다시 계산합니다.
        boss.SearchForAppearancePoints();
        Transform appearPoint = boss.GetRandomAppearancePoint();

        if (appearPoint != null)
        {
            // 여기서 Show()를 호출하여 렌더러를 켭니다.
            boss.Show();
            // boss.transform.position = appearPoint.position; // 이전 위치 설정 코드
            boss.SetPositionOnEdge(appearPoint); // 새 위치 및 방향 설정 코드

            // 나타나는 애니메이션 재생
            if (boss.animator != null)
            {
                // 애니메이션을 강제로 처음부터 다시 재생하여 '나오다 마는' 현상을 방지합니다.
                boss.animator.Play("Appear", -1, 0f);
            }
        }
        else
        {
            // 출현 지점이 없으면 다시 숨기 상태로 전환
            Debug.LogWarning("MasterOfCloset: No appearance points found. Returning to Hide state.");
            boss.ChangeState(MasterOfCloset.MasterOfClosetAction.Hide);
        }
    }

    public override void OnStateExit() { }

    public override void OnStateUpdate() { }
}

// 공격 상태
public class MasterOfClosetAttack : BaseState
{
    private MasterOfCloset boss;

    public MasterOfClosetAttack(MasterOfCloset boss) : base(boss)
    {
        this.boss = boss;
    }

    public override void OnStateEnter()
    {
        // 공격 애니메이션 재생
        if (boss.animator != null)
        {
            boss.animator.SetTrigger("Attack");
        }
        // 실제 데미지는 애니메이션 이벤트에서 처리하는 것을 권장
    }

    public override void OnStateExit() { }

    public override void OnStateUpdate() { }
}
