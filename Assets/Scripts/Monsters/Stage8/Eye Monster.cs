using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EyeMonster : BossMonster
{

    [Header("State Time Settings")]
    [SerializeField] private float traceTime = 2f; // 변칙적인 움직임을 위한 시간
    [SerializeField] private float irregularMoveTime = 3f; // 변칙적인 움직임을 위한 시간
    [SerializeField] private float attackTime = 4f; // 변칙적인 움직임을 위한 시간
    [SerializeField] private float waitTime = 1f; // 변칙적인 움직임을 위한 시간
    [Header("Others")]
    [SerializeField] private float traceSpeed = 1f; 
    [SerializeField] private float irregularMoveSpeed = 4f; 
    [SerializeField] private float pupilRange = 1f; // 동공의 이동 범위
    [SerializeField] private float maxDistanceToPlayer = 7f; // 플레이어와의 최대 거리



    public bool isWatching { get; set; } //state함수들이 접근해서 변경함

    //public Animator animator { get; private set; }
    //private SpriteRenderer spriteRenderer; //부모에 이미 들어있음?
    private Collider2D col;
    private EyeMonsterAction currentAction;
    private Transform pupil;
    private Vector3 prevBlockPosition;

    protected override void Awake()
    {
        base.Awake();//anim 변수로 animator 컴포넌트 불러옴?
        //spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        pupil = transform.Find("Pupil");
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

    public void RazorAttack(){ }

    public void Watch()
    {
        if (isWatching)
        {
            Vector3 direction = GameManager.Instance.player.transform.position - transform.position;
            direction.Normalize();
            pupil.position = transform.position + direction * pupilRange;
        }
    }

    public Vector3 FindRandomBlock()
    {
        // Block 레이어만 검사 (LayerMask 사용)
        LayerMask blockLayer = LayerMask.GetMask("Block");
        //블럭중에 눈알괴물이 박혀있을 수 있는 기다란 블럭만을 찾은 후에, 그 블럭의 중간 위치의 transform을 반환해야할듯

        for (int i = 0; i < 20; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 20f, blockLayer);

            if (hit.collider != null)
            {
                GameObject block = hit.collider.gameObject;
                Vector3 blockPosition = block.transform.position;
                float distanceToPlayer = Vector2.Distance(blockPosition, GameManager.Instance.player.transform.position);

                if (distanceToPlayer < maxDistanceToPlayer && block.activeInHierarchy && !CheckIsNearBlock(blockPosition, prevBlockPosition))
                {
                    Debug.Log("EyeMonster: 찾은 블록 - " + block.name + " (거리: " + distanceToPlayer + ")");
                    prevBlockPosition = blockPosition;
                    return blockPosition;
                }
            }
        }
        //적합한 블록을 찾지 못한 경우, 이전 블럭 위치를 반환
        return prevBlockPosition;
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
        boss.isWatching = true;
    }
    public override void OnStateUpdate()
    {//monsterTransform은 BaseState클래스로 상속받아 사용
        var dir = (GameManager.Instance.player.transform.position - monsterTransform.position).normalized;
        Vector3 newPosition = monsterTransform.position;
        newPosition.x += dir.x * traceSpeed * Time.deltaTime;
        newPosition.y += dir.y * traceSpeed * Time.deltaTime;
        monsterTransform.position = newPosition;
        boss.Watch();
    }
    public override void OnStateExit(){ }
}

public class EyeMonsterIrregularMove : BaseState
{
    EyeMonster boss;
    Vector3 targetBlock;
    float irregularMoveSpeed = 4.0f;

    public EyeMonsterIrregularMove(EyeMonster boss, float irregularMoveSpeed) : base(boss)
    {
        this.boss = boss;
        this.irregularMoveSpeed = irregularMoveSpeed;
    }

    public override void OnStateEnter()
    {
        boss.isWatching = true;
        targetBlock = boss.FindRandomBlock();
    }
    public override void OnStateUpdate()
    {
        var dir = (targetBlock - monsterTransform.position).normalized;
        Vector3 newPosition = monsterTransform.position;
        newPosition.x += dir.x * irregularMoveSpeed * Time.deltaTime;
        newPosition.y += dir.y * irregularMoveSpeed * Time.deltaTime;
        monsterTransform.position = newPosition;
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
    }
    public override void OnStateUpdate()
    {
        boss.Watch();
    }
    public override void OnStateExit(){ }
}
