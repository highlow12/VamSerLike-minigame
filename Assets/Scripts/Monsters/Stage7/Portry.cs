using UnityEngine;
using System.Collections.Generic;

// Portry의 상태 정의
public enum PortryAction
{
    SlowWalk,
    Dash
}

public class Portry : BossMonster
{
    [Header("Movement Settings")]
    [SerializeField] private float slowWalkSpeed = 1.5f; // 느리게 걷는 속도
    [SerializeField] private float dashSpeed = 8f;     // 빠르게 다가오는 속도
    [SerializeField] private float slowWalkDuration = 3f; // 느리게 걷는 시간
    [SerializeField] private float dashDuration = 1f;     // 빠르게 다가오는 시간

    [Header("Audio Settings")]
    [SerializeField] private AudioClip jointCrackSound; // 관절 꺾이는 소리
    private AudioSource audioSource;

    private float stateTimer; // 현재 상태 유지 시간 타이머
    private PortryAction currentAction; // 현재 상태

    // 애니메이터 컴포넌트
    public Animator animator { get; private set; }

    protected override void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        animator = GetComponent<Animator>();
         if (animator == null)
        {
            // 애니메이터가 없다면 추가하거나 오류 로그 출력
            Debug.LogError("Portry requires an Animator component.");
        }
    }

    // 상태 초기화
    protected override void initState()
    {
        states = new Dictionary<int, BaseState>
        {
            { (int)PortryAction.SlowWalk, new PortrySlowWalk(this, slowWalkSpeed) },
            { (int)PortryAction.Dash, new PortryDash(this, dashSpeed) }
        };
        // 초기 상태 설정
        currentAction = PortryAction.SlowWalk;
        monsterFSM = new MonsterFSM(states[(int)currentAction]);
        stateTimer = slowWalkDuration; // 첫 상태의 타이머 설정
    }

    // 상태 전환 검사
    protected override void checkeState()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            // 상태 전환
            if (currentAction == PortryAction.SlowWalk)
            {
                ChangeState(PortryAction.Dash);
                stateTimer = dashDuration;
            }
            else // currentAction == PortryAction.Dash
            {
                ChangeState(PortryAction.SlowWalk);
                stateTimer = slowWalkDuration;
            }
        }
    }

    // 상태 변경 함수
    private void ChangeState(PortryAction nextAction)
    {
        currentAction = nextAction;
        monsterFSM.ChangeState(states[(int)nextAction]);
    }

    // 플레이어와 충돌 시 데미지 처리 (공격 애니메이션 없음)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                //player.TakeDamage(damage); // BossMonster에서 상속받은 damage 사용
            }
        }
    }

     // 관절 소리 재생 (필요시 상태 클래스에서 호출)
    public void PlayJointCrackSound()
    {
        if (audioSource != null && jointCrackSound != null)
        {
            audioSource.PlayOneShot(jointCrackSound);
        }
    }

    // 공격 메서드 (BossMonster 추상 메서드 구현, 실제 사용 X)
    protected override void Attack()
    {
        // Portry는 접촉 시 데미지를 주므로 이 메서드는 비워둡니다.
    }
}

// 느리게 걷는 상태
public class PortrySlowWalk : BaseState
{
    private Portry portry;
    private float speed;

    public PortrySlowWalk(Portry bossMonster, float moveSpeed) : base(bossMonster)
    {
        portry = bossMonster;
        speed = moveSpeed;
    }

    public override void OnStateEnter()
    {
        // 느리게 걷는 애니메이션 시작 (애니메이터 파라미터 설정)
        portry.animator?.SetFloat("MoveSpeed", speed);

        // 관절 꺾이는 소리 재생 (주기적으로 또는 시작 시 한 번)
        portry.PlayJointCrackSound(); // 예시: 상태 진입 시 한 번 재생
        // TODO: 필요하다면 코루틴 등으로 주기적인 소리 재생 구현
    }

    public override void OnStateUpdate()
    {
        // 플레이어 방향으로 천천히 이동
        Vector2 direction = ((Vector2)GameManager.Instance.player.transform.position - (Vector2)portry.transform.position).normalized;
        portry.transform.position += (Vector3)direction * speed * Time.deltaTime;

        // 이동 방향에 따라 스프라이트 뒤집기
        if (direction.x != 0)
        {
             portry.transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(portry.transform.localScale.x), portry.transform.localScale.y, portry.transform.localScale.z);
        }
    }

    public override void OnStateExit()
    {
        // 상태 종료 시 정리 (예: 소리 중지)
    }
}

// 빠르게 다가오는 상태
public class PortryDash : BaseState
{
    private Portry portry;
    private float speed;

    public PortryDash(Portry bossMonster, float moveSpeed) : base(bossMonster)
    {
        portry = bossMonster;
        speed = moveSpeed;
    }

    public override void OnStateEnter()
    {
        // 빠르게 다가오는 애니메이션 시작 (애니메이터 파라미터 설정)
         portry.animator?.SetFloat("MoveSpeed", speed);
        // TODO: 돌진 효과음 등이 있다면 여기서 재생
    }

    public override void OnStateUpdate()
    {
        // 플레이어 방향으로 빠르게 이동
        Vector2 direction = ((Vector2)GameManager.Instance.player.transform.position - (Vector2)portry.transform.position).normalized;
        portry.transform.position += (Vector3)direction * speed * Time.deltaTime;

         // 이동 방향에 따라 스프라이트 뒤집기
        if (direction.x != 0)
        {
             portry.transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(portry.transform.localScale.x), portry.transform.localScale.y, portry.transform.localScale.z);
        }
    }

    public override void OnStateExit()
    {
        // 상태 종료 시 정리
    }
}
