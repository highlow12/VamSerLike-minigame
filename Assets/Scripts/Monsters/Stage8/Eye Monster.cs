using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class EyeMonster : BossMonster
{

    [Header("State Time Settings")]
    [SerializeField] private float traceTime = 2f; // 추적을 위한 시간
    [SerializeField] private float irregularMoveTime = 3f; // 변칙적인 움직임을 위한 시간
    [SerializeField] private float attackTime = 4f; // 공격을 위한 시간
    [SerializeField] private float waitTime = 1f; // 다시 움직이기 위한 대기시간
    [SerializeField] private float razorDelay = 1f; // 레이저 발사 지연 시간
    [SerializeField] private float razorDuration = 2f; // 레이저 발사 시간 (지연과 발사시간의 총합이 공격 시간과 같아야함)

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite attackSprite;
    [Header("Others")]
    [SerializeField] private float traceSpeed = 1f;
    [SerializeField] private float irregularMoveSpeed = 4f;
    [SerializeField] private float pupilRange = 1f; // 동공의 이동 범위
    [SerializeField] private float maxDistanceToPlayer = 7f; // 플레이어와의 최대 거리
    [SerializeField] private float fixedOffset = 1f; // 플레이어와의 최대 거리
    [SerializeField] private GameObject RazorPrefab; // Razor 프리팹

    [Header("Pathfinding Settings")]
    [SerializeField] private LayerMask obstacleLayerMask; // 장애물 레이어
    [SerializeField] private float nodeSize = 0.5f; // 그리드 노드 크기
    [SerializeField] private int maxPathLength = 50; // 최대 경로 길이
    [SerializeField] private float pathfindingUpdateInterval = 0.5f; // 경로 업데이트 주기



    public bool isWatching { get; set; } //state함수들이 접근해서 변경함

    //public Animator animator { get; private set; }
    //private SpriteRenderer spriteRenderer; //부모에 이미 들어있음?
    private EyeMonsterAction currentAction;
    private int currentRotation_z;
    private Transform pupil;
    private Vector3 prevBlockPosition;

    // A* 경로 찾기를 위한 변수들
    public List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private float pathUpdateTimer = 0f;

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

    protected override void Awake()
    {
        base.Awake();//anim 변수로 animator 컴포넌트 불러옴?
        //spriteRenderer = GetComponent<SpriteRenderer>();
        pupil = transform.Find("Pupil");

        // 장애물 레이어 마스크 초기화 (기본값이 설정되지 않은 경우)
        if (obstacleLayerMask == 0)
        {
            obstacleLayerMask = LayerMask.GetMask("MiddleBlock");
        }
    }

    public enum EyeMonsterAction
    {
        Trace = 0,
        IrregularMove = 1,
        Attack,
        Wait
    }

    protected override void Start()
    {
        base.Start();
        // SearchForAppearancePoints(); // 이제 나타날 때마다 호출하므로 시작 시에는 필요 없습니다.
    }

    protected override void initState()//start에서 호출함
    {
        //animator = GetComponent<Animator>();
        //부모인 BossMonster 클래스의 states 변수에다가 만들어 놓은 상태들을 저장해둠
        stateTimer = 0f;
        states = new Dictionary<int, BaseState>
        {
            { (int)EyeMonsterAction.Trace, new EyeMonsterTrace(this, traceSpeed) },
            { (int)EyeMonsterAction.IrregularMove, new EyeMonsterIrregularMove(this, irregularMoveSpeed) },
            { (int)EyeMonsterAction.Attack, new EyeMonsterAttack(this) },
            { (int)EyeMonsterAction.Wait, new EyeMonsterWait(this) }
        };
        ChangeState(EyeMonsterAction.Trace);
    }

    protected override void checkeState()
    {//Update 메서드에서 계속 호출되는 함수
        stateTimer += Time.deltaTime;
        if (currentAction == EyeMonsterAction.Trace)
        {
            if (stateTimer >= traceTime)
            {
                ChangeState(EyeMonsterAction.IrregularMove);
            }
        }
        else if (currentAction == EyeMonsterAction.IrregularMove)
        {
            if (stateTimer >= irregularMoveTime)
            {
                ChangeState(EyeMonsterAction.Attack);
            }
        }
        else if (currentAction == EyeMonsterAction.Attack)
        {
            if (stateTimer >= attackTime)
            {
                ChangeState(EyeMonsterAction.Wait);
            }
        }
        else if (currentAction == EyeMonsterAction.Wait)
        {
            if (stateTimer >= waitTime)
            {
                ChangeState(EyeMonsterAction.Trace);
                Debug.Log("EyeMonster 패턴 종료 및 Trace 재시작");
            }
        }
    }

    public void ChangeState(EyeMonsterAction nextAction)
    {
        stateTimer = 0f;//다음 state의 경과시간 계산 위해 0으로 초기화
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

    public void RazorAttack(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        StartCoroutine(RazorAttackCoroutine(direction));

    }
    IEnumerator RazorAttackCoroutine(Vector3 direction)
    {
        //Debug.Log("EyeMonster: RazorAttackCoroutine 및 진동 시작");
        VFXManager.Instance.AnimateShakeBack(0f, 0.5f, razorDelay + 1);
        pupil.GetComponent<SpriteRenderer>().color = Color.red; // 동공 색상 변경 (공격 준비)
        yield return new WaitForSeconds(razorDelay);

        RazorPrefab.SetActive(true);
        RazorPrefab.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
        yield return new WaitForSeconds(razorDuration);

        pupil.GetComponent<SpriteRenderer>().color = Color.white; // 동공 색상 변경 (공격 종료)
        RazorPrefab.SetActive(false);
    }

    public void Watch()
    {
        if (isWatching)
        {
            Vector3 direction = GameManager.Instance.player.transform.position - transform.position;
            direction.Normalize();
            pupil.position = transform.position + direction * pupilRange;
        }
    }

    public void ChangeToAttackSprite()
    {
        sr.sprite = attackSprite;
        Debug.Log("EyeMonster: ChangeToAttackSprite 호출");
    }
    public void ChangeToNormalSprite()
    {
        sr.sprite = normalSprite;
        Debug.Log("EyeMonster: ChangeToNormalSprite 호출");
    }

    void TurnToPlayer(bool isHorizontal, Vector3 blockPosition)
    {
        if (isHorizontal)
        {
            Debug.Log("EyeMonster: 가로형태의 블록 발견 - ");
            //플레이어가 위에 있는지 아래에 있는지 판단
            if (GameManager.Instance.player.transform.position.y > blockPosition.y)
            {
                transform.rotation = Quaternion.Euler(0, 0, -90);
                currentRotation_z = -90;
                Debug.Log("EyeMonster: 플레이어가 블록 위에 있음");
            }
            else
            {
                Debug.Log("EyeMonster: 플레이어가 블록 아래에 있음");
                transform.rotation = Quaternion.Euler(0, 0, 90);
                currentRotation_z = 90;
            }
        }
        else
        {
            Debug.Log("EyeMonster: 세로형태의 블록 발견 - ");
            //플레이어가 왼쪽에 있는지 오른쪽에 있는지 판단
            if (GameManager.Instance.player.transform.position.x > blockPosition.x)
            {
                Debug.Log("EyeMonster: 플레이어가 블록 오른쪽에 있음");
                transform.rotation = Quaternion.Euler(0, 0, 180);
                currentRotation_z = 180;
            }
            else
            {
                Debug.Log("EyeMonster: 플레이어가 블록 왼쪽에 있음");
                transform.rotation = Quaternion.Euler(0, 0, 0);
                currentRotation_z = 0;
            }
        }
    }


    Vector3 GetPositionWithRotation(Vector3 blockPosition, float z_value)
    {
        Vector3 pos;
        switch (z_value)
        {
            case -90://수평 위쪽
                pos = new Vector3(blockPosition.x, blockPosition.y + fixedOffset, transform.position.z);
                break;
            case 90://수평 아래쪽
                pos = new Vector3(blockPosition.x, blockPosition.y - fixedOffset, transform.position.z);
                break;
            case 0://수직 왼쪽
                pos = new Vector3(blockPosition.x - fixedOffset, blockPosition.y, transform.position.z);
                break;
            case 180://수직 오른쪽
                pos = new Vector3(blockPosition.x + fixedOffset, blockPosition.y, transform.position.z);
                break;
            default:
                Debug.LogError("EyeMonster: 알 수 없는 회전 값: " + z_value);
                pos = blockPosition; // 기본 위치로 설정
                break;
        }
        return pos;
    }

    public Vector3 FindRandomBlock()
    {
        // Block 레이어만 검사 (LayerMask 사용)
        LayerMask blockLayer = LayerMask.GetMask("MiddleBlock");

        for (int i = 0; i < 20; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            RaycastHit2D hit = Physics2D.Raycast(GameManager.Instance.player.transform.position, direction, 20f, blockLayer);

            if (hit.collider != null)
            {
                GameObject block = hit.collider.gameObject;
                Vector3 blockPosition = block.transform.position;
                float distanceToPlayer = Vector2.Distance(blockPosition, GameManager.Instance.player.transform.position);

                if (distanceToPlayer < maxDistanceToPlayer && block.activeInHierarchy && !CheckIsNearBlock(blockPosition, prevBlockPosition))
                {
                    Debug.Log("EyeMonster: 찾은 블록 - " + block.name + " (거리: " + distanceToPlayer + ")");
                    prevBlockPosition = blockPosition;
                    //블럭을 찾았으면 그 블럭에 적합한 각도로 변경해줌
                    TurnToPlayer(CheckIsHorizontalRotation(block), blockPosition);
                    return GetPositionWithRotation(blockPosition, currentRotation_z);
                }
            }
        }
        //적합한 블록을 찾지 못한 경우, 이전 블럭 위치를 반환
        return prevBlockPosition;
    }

    bool CheckIsHorizontalRotation(GameObject cur_block)
    {
        float block_z = cur_block.transform.rotation.eulerAngles.z;//eulerAngles의 값은 다 양수 -90 == 270
        block_z = SnapToNearestAngle(block_z); // 0, 90, 180, 270 중 가장 가까운 값으로 스냅
        switch( block_z)
        {
            case 0:
            case 180:
                Debug.Log("EyeMonster: 블록이 수평 상태");
                return true; // 수평 상태
            case 90:
            case 270:
                Debug.Log("EyeMonster: 블록이 수직 상태");
                return false; // 수직 상태
        }

        /*float block_parent_z = cur_block.transform.parent.transform.rotation.eulerAngles.z;
        block_parent_z = SnapToNearestAngle(block_parent_z); // 0, 90, 180, 270 중 가장 가까운 값으로 스냅
        Debug.Log("블록 부모 로테이션" + block_parent_z);
        Debug.Log("블록 로테이션" + block_z);

        if (block_parent_z == 0)
        {
            if (block_z == 0)
            {
                Debug.Log("EyeMonster: 블록이 수평 상태");
                return true;
            }
            else
            {
                Debug.Log("EyeMonster: 블록이 수직 상태");
                return false;
            }
        }
        else if (block_parent_z == 270)
        {
            if (block_z == 270)
            {
                Debug.Log("EyeMonster: 블록이 수직 상태");
                return false;
            }
            else
            {
                Debug.Log("EyeMonster: 블록이 수평 상태");
                return true;
            }
        }
        else if (block_parent_z == 90)
        {
            if (block_z == 90)
            {
                Debug.Log("EyeMonster: 블록이 수직 상태");
                return false;
            }
            else
            {
                Debug.Log("EyeMonster: 블록이 수평 상태");
                return true;
            }
        }
        else if (block_parent_z == 180)
        {
            if (block_z == 180)
            {
                Debug.Log("EyeMonster: 블록이 수평 상태");
                return true;
            }
            else
            {
                Debug.Log("EyeMonster: 블록이 수직 상태");
                return false;
            }
        }*/

        Debug.LogError("EyeMonster: 알 수 없는 블록 회전 상태");
        return false;
    }
    bool CalculateChildBlockRotation(GameObject cur_block, bool isNormal)
    {
        float rot_z = cur_block.transform.rotation.eulerAngles.z;
        Debug.Log("EyeMonster: 현재 블록 회전 각도: " + rot_z);

        float absRot = System.Math.Abs(rot_z);
        float snappedAngle = SnapToNearestAngle(absRot);

        Debug.Log($"EyeMonster: 원본 각도: {rot_z}, 절댓값: {absRot}, 스냅된 각도: {snappedAngle}, 이름 {cur_block.name}");

        if (isNormal) // 정상 부모 회전 상태
        {
            return snappedAngle == 0 || snappedAngle == 180;
        }
        else
        {
            return snappedAngle == 90;
        }
        // 참이면 가로, 거짓이면 세로
    }

    private float SnapToNearestAngle(float angle)
    {
        float[] targetAngles = { 0, 90, 180, 270 };
        float nearestAngle = targetAngles[0];
        float minDistance = System.Math.Abs(angle - targetAngles[0]);

        foreach (float target in targetAngles)
        {
            float distance = System.Math.Abs(angle - target);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestAngle = target;
            }
        }

        return nearestAngle;
    }


    private bool CheckIsNearBlock(Vector3 position1, Vector3 position2)
    {
        float maxDistance = 5f;
        float distance = Vector3.Distance(position1, position2);
        return distance < maxDistance; // maxDistance 이내에 있으면 근접한 것으로 간주
    }

    // 플레이어와 충돌 시 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("EyeMonster 충돌: 플레이어와 접촉");
            //Attack
        }
    }

    public void SetIsMoving(bool isMoving)
    {
        //anim.SetBool("isMoving", isMoving);
        if (isMoving)
        {
            anim.SetTrigger("Move");
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

    // 해당 위치에 장애물이 있는지 확인
    private bool IsPositionBlocked(Vector2 position)
    {
        Collider2D collider = Physics2D.OverlapCircle(position, nodeSize * 0.4f, obstacleLayerMask);
        return collider != null;
    }

    // 경로를 따라 이동하는 메서드
    public bool MoveAlongPath(float speed)
    {
        if (currentPath == null || currentPath.Count == 0)
            return false;

        if (currentPathIndex >= currentPath.Count)
            return true; // 경로 완료

        Vector3 targetPos = currentPath[currentPathIndex];
        Vector3 direction = (targetPos - transform.position).normalized;

        float distanceToTarget = Vector3.Distance(transform.position, targetPos);

        if (distanceToTarget < 0.1f)
        {
            currentPathIndex++;
            return currentPathIndex >= currentPath.Count;
        }

        Vector3 newPosition = transform.position;
        newPosition.x += direction.x * speed * Time.deltaTime;
        newPosition.y += direction.y * speed * Time.deltaTime;
        transform.position = newPosition;

        return false;
    }

    // 목표 위치로의 경로 업데이트
    public void UpdatePathToTarget(Vector3 targetPosition)
    {
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer >= pathfindingUpdateInterval)
        {
            pathUpdateTimer = 0f;
            currentPath = FindPath(transform.position, targetPosition);
            currentPathIndex = 0;
        }
    }
}

//Monster 부모 클래스에 monstrFSM이라는 변수가 있어서, 여기에 상태를 계속 업데이트 해나가며 EyeMonster를 대신 변경해줌.
public class EyeMonsterTrace : BaseState
{
    EyeMonster boss;
    private float traceSpeed = 3.0f; // 이동 속도

    public EyeMonsterTrace(EyeMonster boss, float traceSpeed) : base(boss)
    {
        this.boss = boss;
        this.traceSpeed = traceSpeed;
    }

    public override void OnStateEnter()
    {
        boss.ChangeToNormalSprite(); // 일반 스프라이트로 변경
        boss.isWatching = true;
        boss.SetIsMoving(true); // 걷는 애니메이션 시작

        // 초기 경로 계산
        boss.UpdatePathToTarget(GameManager.Instance.player.transform.position);
    }
    public override void OnStateUpdate()
    {
        // A* 경로 업데이트 및 이동
        boss.UpdatePathToTarget(GameManager.Instance.player.transform.position);
        
        // A* 경로를 따라 이동, 경로가 없으면 직접 이동
        bool pathCompleted = boss.MoveAlongPath(traceSpeed);
        
        if (pathCompleted || boss.currentPath.Count == 0)
        {
            // 경로가 없거나 완료된 경우 직접 이동 (fallback)
            var dir = (GameManager.Instance.player.transform.position - monsterTransform.position).normalized;
            Vector3 newPosition = monsterTransform.position;
            newPosition.x += dir.x * traceSpeed * Time.deltaTime;
            newPosition.y += dir.y * traceSpeed * Time.deltaTime;
            monsterTransform.position = newPosition;
        }

        boss.Watch();
    }
    public override void OnStateExit(){ }
}

public class EyeMonsterIrregularMove : BaseState
{
    EyeMonster boss;
    Vector3 targetBlock;
    float irregularMoveSpeed = 4.0f;
    bool isEnd = false;

    public EyeMonsterIrregularMove(EyeMonster boss, float irregularMoveSpeed) : base(boss)
    {
        this.boss = boss;
        this.irregularMoveSpeed = irregularMoveSpeed;
    }

    public override void OnStateEnter()
    {
        boss.isWatching = true;
        isEnd = false;
        targetBlock = boss.FindRandomBlock();
        boss.SetIsMoving(true); // 걷는 애니메이션 시작

        // 목표 블록으로의 초기 경로 계산
        boss.UpdatePathToTarget(targetBlock);
    }
    public override void OnStateUpdate()
    {
        if (isEnd)
        {
            boss.Watch();
            return;
        }
        Vector3 dir = (targetBlock - monsterTransform.position);
        if (dir.magnitude < 0.5f)// 벡터가 0에 가까운 경우 (즉, 목표 위치에 도착)
        {
            //밑에 코드들은 딱 한번만 실행됨
            //Vector3 endPosition = new Vector3(targetBlock.x, targetBlock.y, monsterTransform.position.z);//통일해야함
            //monsterTransform.position = endPosition;
            boss.Watch();
            boss.ChangeToAttackSprite();
            monsterTransform.position = targetBlock;
            isEnd = true;
            //position 이동해서 벽에 붙어있는 상태로 반환
            return;
        }
        // A* 경로 업데이트 및 이동
        boss.UpdatePathToTarget(targetBlock);

        // A* 경로를 따라 이동, 경로가 없으면 직접 이동
        bool pathCompleted = boss.MoveAlongPath(irregularMoveSpeed);

        if (pathCompleted || boss.currentPath.Count == 0)
        {
            // 경로가 없거나 완료된 경우 직접 이동 (fallback)
            dir = dir.normalized;
            Vector3 newPosition = monsterTransform.position;
            newPosition.x += dir.x * irregularMoveSpeed * Time.deltaTime;
            newPosition.y += dir.y * irregularMoveSpeed * Time.deltaTime;
            monsterTransform.position = newPosition;
        }

        //Debug.Log("EyeMonster가 블록을 향해 이동 중: " + monsterTransform.position);
        boss.Watch();
    }
    public override void OnStateExit(){ }
}


public class EyeMonsterAttack : BaseState
{
    EyeMonster boss;

    public EyeMonsterAttack(EyeMonster boss) : base(boss)
    {
        this.boss = boss;
    }
    public override void OnStateEnter()
    {
        //아마도 고정된 상태에서 공격을 진행함
        //벽에 붙어있는 상태의 스프라이트로 설정
        //광선 발사 애니메이션 시작
        boss.Watch();//공격전 마지막 플레이어 위치를 계속 바라봄
        boss.isWatching = false; // 공격 시에는 시선을 고정
        boss.SetIsMoving(false); // 걷는 애니메이션 중지
        boss.RazorAttack(GameManager.Instance.player.transform.position); // 플레이어 위치로 레이저 공격
    }
    public override void OnStateUpdate()
    {

    }
    public override void OnStateExit(){ }
}

public class EyeMonsterWait : BaseState
{
    EyeMonster boss;

    public EyeMonsterWait(EyeMonster boss) : base(boss)
    {
        this.boss = boss;
    }
    public override void OnStateEnter()
    {
        boss.isWatching = true;
        boss.SetIsMoving(false); // 걷는 애니메이션 중지
    }
    public override void OnStateUpdate()
    {
        boss.Watch();
    }
    public override void OnStateExit(){ }
}
