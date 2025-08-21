using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EyeMonster : BossMonster
{
    public Animator animator { get; private set; }
    public new float attackRange = 3f; // 공격 사거리
    public float traceSpeed = 5f; // 추적 속도
    public float pathfindingUpdateInterval = 0.5f; // 경로 업데이트 주기
    
    // 공격 데미지에 접근하기 위한 프로퍼티
    public float AttackDamage => damage;
    
    [Header("Pathfinding Settings")]
    public LayerMask obstacleLayerMask = -1; // 장애물 레이어
    public float nodeSize = 0.5f; // 그리드 노드 크기
    public int maxPathLength = 50; // 최대 경로 길이 (성능 최적화)
    public float maxPathfindingDistance = 15f; // 최대 pathfinding 거리
    
    private Vector3 targetPosition;
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private float pathUpdateTimer = 0f;
    private EyeMonsterAction currentAction;
    private float stateTimer = 0f;
    
    // A* 알고리즘을 위한 노드 클래스
    private class PathNode
    {
        public Vector2 position;
        public float gCost; // 시작점으로부터의 거리
        public float hCost; // 목표점까지의 추정 거리
        public float fCost => gCost + hCost; // 총 비용
        public PathNode parent;
        public bool isWalkable;
        
        public PathNode(Vector2 pos, bool walkable = true)
        {
            position = pos;
            isWalkable = walkable;
        }
    }
    
    public enum EyeMonsterAction
    {
        Idle,
        Chase,
        Attack
    }
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void Start()
    {
        base.Start();
        targetPosition = GameManager.Instance.player.transform.position;
    }
    
    protected override void initState()
    {
        animator = GetComponent<Animator>();
        states = new Dictionary<int, BaseState>
        {
            { (int)EyeMonsterAction.Idle, new EyeMonsterIdle(this) },
            { (int)EyeMonsterAction.Chase, new EyeMonsterChase(this) },
            { (int)EyeMonsterAction.Attack, new EyeMonsterAttack(this) }
        };
        ChangeState(EyeMonsterAction.Chase);
    }
    
    protected override void checkeState()
    {
        // 플레이어와의 거리 체크
        float distanceToPlayer = Vector3.Distance(transform.position, GameManager.Instance.player.transform.position);
        
        if (currentAction == EyeMonsterAction.Chase && distanceToPlayer <= attackRange)
        {
            ChangeState(EyeMonsterAction.Attack);
        }
        else if (currentAction == EyeMonsterAction.Attack && distanceToPlayer > attackRange)
        {
            ChangeState(EyeMonsterAction.Chase);
        }
        
        // 경로 업데이트 타이머
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathfindingUpdateInterval)
        {
            pathUpdateTimer = 0f;
            UpdatePathToTarget();
        }
    }
    
    public void ChangeState(EyeMonsterAction nextAction)
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
    }
    
    // A* 경로 찾기 알고리즘
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        List<Vector3> path = new List<Vector3>();
        
        // 그리드 기반 A* 구현
        Vector2 startNode = new Vector2(
            Mathf.Round(startPos.x / nodeSize) * nodeSize,
            Mathf.Round(startPos.y / nodeSize) * nodeSize
        );
        
        Vector2 targetNode = new Vector2(
            Mathf.Round(targetPos.x / nodeSize) * nodeSize,
            Mathf.Round(targetPos.y / nodeSize) * nodeSize
        );
        
        List<PathNode> openSet = new List<PathNode>();
        HashSet<Vector2> closedSet = new HashSet<Vector2>();
        Dictionary<Vector2, PathNode> allNodes = new Dictionary<Vector2, PathNode>();
        
        PathNode startPathNode = new PathNode(startNode);
        startPathNode.gCost = 0;
        startPathNode.hCost = Vector2.Distance(startNode, targetNode);
        
        openSet.Add(startPathNode);
        allNodes[startNode] = startPathNode;
        
        Vector2[] directions = {
            Vector2.up * nodeSize, Vector2.down * nodeSize,
            Vector2.left * nodeSize, Vector2.right * nodeSize,
            new Vector2(nodeSize, nodeSize), new Vector2(-nodeSize, nodeSize),
            new Vector2(nodeSize, -nodeSize), new Vector2(-nodeSize, -nodeSize)
        };
        
        int iterations = 0;
        while (openSet.Count > 0 && iterations < maxPathLength)
        {
            iterations++;
            
            // 가장 낮은 fCost를 가진 노드 선택
            PathNode currentNode = openSet.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();
            openSet.Remove(currentNode);
            closedSet.Add(currentNode.position);
            
            // 목표에 도달했는지 확인
            if (Vector2.Distance(currentNode.position, targetNode) < nodeSize)
            {
                // 경로 재구성
                PathNode pathNode = currentNode;
                while (pathNode != null)
                {
                    path.Add(new Vector3(pathNode.position.x, pathNode.position.y, 0));
                    pathNode = pathNode.parent;
                }
                path.Reverse();
                break;
            }
            
            // 인접 노드들 검사
            foreach (Vector2 direction in directions)
            {
                Vector2 neighborPos = currentNode.position + direction;
                
                if (closedSet.Contains(neighborPos))
                    continue;
                
                // 장애물 체크
                if (IsPositionBlocked(neighborPos))
                    continue;
                
                PathNode neighbor;
                if (!allNodes.TryGetValue(neighborPos, out neighbor))
                {
                    neighbor = new PathNode(neighborPos);
                    allNodes[neighborPos] = neighbor;
                }
                
                float tentativeGCost = currentNode.gCost + Vector2.Distance(currentNode.position, neighborPos);
                
                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGCost >= neighbor.gCost)
                {
                    continue;
                }
                
                neighbor.parent = currentNode;
                neighbor.gCost = tentativeGCost;
                neighbor.hCost = Vector2.Distance(neighborPos, targetNode);
            }
        }
        
        return path;
    }
    
    // 위치에 장애물이 있는지 확인
    private bool IsPositionBlocked(Vector2 position)
    {
        // 더 큰 반지름으로 체크하여 안전한 거리 확보
        Collider2D hit = Physics2D.OverlapCircle(position, nodeSize * 0.6f, obstacleLayerMask);
        
        // Block 태그를 가진 객체나 그 부모가 Block 태그를 가진 경우 차단된 것으로 판단
        if (hit != null && hit.gameObject != gameObject && hit.gameObject != GameManager.Instance.player)
        {
            // 직접 Block 태그를 가지고 있거나
            if (hit.CompareTag("Block"))
                return true;
                
            // 부모가 Block 태그를 가지고 있는 경우
            if (hit.transform.parent != null && hit.transform.parent.CompareTag("Block"))
                return true;
        }
        
        return false;
    }
    
    // 목표로의 경로 업데이트
    public void UpdatePathToTarget()
    {
        if (GameManager.Instance.player == null) return;
        
        targetPosition = GameManager.Instance.player.transform.position;
        
        // 거리가 너무 멀면 직선 이동 사용
        float distanceToPlayer = Vector3.Distance(transform.position, targetPosition);
        if (distanceToPlayer > maxPathfindingDistance)
        {
            currentPath.Clear();
            return;
        }
        
        currentPath = FindPath(transform.position, targetPosition);
        currentPathIndex = 0;
    }
    
    // 경로를 따라 이동
    public void MoveAlongPath()
    {
        if (currentPath.Count == 0) 
        {
            // 경로가 없으면 직선으로 플레이어에게 이동 (fallback)
            MoveDirectlyTowardsPlayer();
            return;
        }
        
        // 현재 목표 지점에 도달했는지 확인
        if (currentPathIndex < currentPath.Count)
        {
            Vector3 targetWaypoint = currentPath[currentPathIndex];
            float distance = Vector3.Distance(transform.position, targetWaypoint);
            
            if (distance < nodeSize * 0.5f)
            {
                currentPathIndex++;
            }
            else
            {
                // 목표 지점으로 이동
                Vector3 direction = (targetWaypoint - transform.position).normalized;
                transform.position += direction * traceSpeed * Time.deltaTime;
                
                // 스프라이트 방향 조정
                UpdateSpriteDirection(direction);
            }
        }
        else
        {
            // 모든 경로를 완주했으면 직선으로 목표에 접근
            MoveDirectlyTowardsPlayer();
        }
    }
    
    // 직선으로 플레이어에게 이동 (pathfinding 실패 시 fallback)
    private void MoveDirectlyTowardsPlayer()
    {
        if (GameManager.Instance.player == null) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * traceSpeed * Time.deltaTime;
        UpdateSpriteDirection(direction);
    }
    
    // 스프라이트 방향 업데이트
    private void UpdateSpriteDirection(Vector3 direction)
    {
        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
    
    // 디버그용 경로 그리기
    private void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
        
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentPath[currentPathIndex], 0.2f);
        }
        
        // 공격 범위 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 목표 위치 표시
        if (GameManager.Instance?.player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, 0.3f);
        }
    }
}

// 기본 상태 - 대기
public class EyeMonsterIdle : BaseState
{
    private EyeMonster eyeMonster;
    
    public EyeMonsterIdle(EyeMonster monster) : base(monster)
    {
        eyeMonster = monster;
    }
    
    public override void OnStateEnter()
    {
        if (eyeMonster.animator != null)
        {
            eyeMonster.animator.SetTrigger("Idle");
        }
    }
    
    public override void OnStateUpdate()
    {
        // 대기 상태에서는 특별한 동작 없음
    }
    
    public override void OnStateExit()
    {
    }
}

// 추적 상태
public class EyeMonsterChase : BaseState
{
    private EyeMonster eyeMonster;
    
    public EyeMonsterChase(EyeMonster monster) : base(monster)
    {
        eyeMonster = monster;
    }
    
    public override void OnStateEnter()
    {
        if (eyeMonster.animator != null)
        {
            eyeMonster.animator.SetTrigger("Chase");
        }
        eyeMonster.UpdatePathToTarget();
    }
    
    public override void OnStateUpdate()
    {
        // 경로를 따라 이동
        eyeMonster.MoveAlongPath();
    }
    
    public override void OnStateExit()
    {
    }
}

// 공격 상태 (라인 383-410에 해당)
public class EyeMonsterAttack : BaseState
{
    private EyeMonster eyeMonster;
    private float attackCooldown = 2f;
    private float attackTimer = 0f;
    private bool hasAttacked = false;
    
    public EyeMonsterAttack(EyeMonster monster) : base(monster)
    {
        eyeMonster = monster;
    }
    
    public override void OnStateEnter()
    {
        // 공격 애니메이션 재생
        if (eyeMonster.animator != null)
        {
            eyeMonster.animator.SetTrigger("Attack");
        }
        
        attackTimer = 0f;
        hasAttacked = false;
        
        // 공격 중에도 경로 찾기로 플레이어에게 접근
        eyeMonster.UpdatePathToTarget();
    }
    
    public override void OnStateUpdate()
    {
        attackTimer += Time.deltaTime;
        
        // 공격 쿨다운 중에도 경로를 따라 이동 (traceSpeed 유지)
        if (!hasAttacked && attackTimer < attackCooldown * 0.5f)
        {
            eyeMonster.MoveAlongPath();
        }
        
        // 공격 실행
        if (!hasAttacked && attackTimer >= attackCooldown * 0.5f)
        {
            ExecuteAttack();
            hasAttacked = true;
        }
        
        // 공격 완료 후 다시 추적 상태로 전환
        if (hasAttacked && attackTimer >= attackCooldown)
        {
            eyeMonster.ChangeState(EyeMonster.EyeMonsterAction.Chase);
        }
    }
    
    public override void OnStateExit()
    {
    }
    
    // 라인 412-452에 해당하는 공격 실행 로직
    private void ExecuteAttack()
    {
        // 플레이어와의 거리 확인
        float distanceToPlayer = Vector3.Distance(eyeMonster.transform.position, GameManager.Instance.player.transform.position);
        
        if (distanceToPlayer <= eyeMonster.attackRange)
        {
            // 공격 사운드 재생
            eyeMonster.PlayAttackSound();
            
            // 플레이어에게 데미지 적용
            Player player = GameManager.Instance.player.GetComponent<Player>();
            if (player != null)
            {
                // 기본 공격력으로 데미지 적용
                player.TakeDamage(eyeMonster.AttackDamage);
            }
            
            // 공격 이펙트 생성 (선택사항)
            CreateAttackEffect();
        }
        else
        {
            // 공격 범위 밖이면 경로 찾기로 접근
            eyeMonster.UpdatePathToTarget();
            eyeMonster.MoveAlongPath();
        }
    }
    
    private void CreateAttackEffect()
    {
        // 공격 이펙트 생성 로직 (필요시 구현)
        Debug.Log("Eye Monster Attack Effect!");
    }
}