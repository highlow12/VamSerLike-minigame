using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PaintMonsterAction
{
    Move,
    Attack
}

public class PaintMonster : BossMonster
{
    [Header("이동 관련 설정")]
    [SerializeField] private float moveSpeed = 3f; // 이동 속도
    [SerializeField] private float floatHeight = 2f; // 공중에 떠 있는 높이
    
    [Header("공격 관련 설정")]
    [SerializeField] private float attackCooldown = 3f; // 공격 쿨다운
    [SerializeField] private float attackDuration = 1.2f; // 공격 지속 시간
    
    private Animator animator;
    private float initialY; // 초기 Y 위치
    private float currentAttackCooldown; // 현재 공격 쿨다운 타이머

    protected override void Start()
    {
        base.Start();
        initialY = transform.position.y; // 초기 Y 위치 저장
        currentAttackCooldown = attackCooldown; // 쿨다운 초기화
    }

    // 초기 상태 설정
    protected override void initState()
    {
        animator = GetComponent<Animator>();
        
        // 상태 딕셔너리 초기화
        states = new Dictionary<int, BaseState>
        {
            { (int)PaintMonsterAction.Move, new PaintMonsterMove(this) },
            { (int)PaintMonsterAction.Attack, new PaintMonsterAttack(this) }
        };
        
        // 초기 상태는 이동으로 설정
        monsterFSM = new MonsterFSM(states[(int)PaintMonsterAction.Move]);
    }

    // 상태 체크 및 변경
    protected override void checkeState()
    {
        // 현재 상태에 따른 처리
        BaseState currentState = monsterFSM.currentState;
        
        // 공격 쿨다운 감소
        if (currentAttackCooldown > 0)
            currentAttackCooldown -= Time.deltaTime;
        
        // 플레이어와의 거리 계산
        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // 현재 상태가 이동 상태이고, 플레이어가 공격 범위 내에 있고, 공격 쿨다운이 끝났다면 공격 상태로 전환
        if (currentState is PaintMonsterMove && distToPlayer < attackRange && currentAttackCooldown <= 0)
        {
            monsterFSM.ChangeState(states[(int)PaintMonsterAction.Attack]);
        }
        // 공격 상태에서 이동 상태로의 전환은 애니메이션 이벤트에서 OnAttackComplete 함수를 호출하여 처리
    }

    // 공격 실행 메서드 (Attack 상태에서 호출)
    public void ExecuteAttack()
    {
        // 공격 애니메이션 시작 (애니메이션에서 히트박스 처리)
        if (animator != null)
            animator.SetTrigger("Attack");
        
        // 애니메이션 이벤트에서 OnAttackComplete 호출하여 상태 전환
    }
    
    // 애니메이션 이벤트에서 호출할 공격 완료 함수
    public void OnAttackComplete()
    {
        // 공격 쿨다운 설정
        currentAttackCooldown = attackCooldown;
        
        // 공격 후 이동 상태로 전환
        monsterFSM.ChangeState(states[(int)PaintMonsterAction.Move]);
    }
    
    // 플레이어 방향으로 이동 (Move/Attack 상태에서 호출)
    public void MoveTowardsPlayer(float speedMultiplier = 1.0f)
    {
        if (playerTransform == null)
            return;
            
        // 플레이어 방향 계산
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        
        // X축으로 이동
        Vector3 movement = new Vector3(direction.x, direction.y, 0) * moveSpeed * speedMultiplier * Time.deltaTime;
        
        // 최종 위치 계산 (X축 이동 + Y축 고정 높이)
        Vector3 newPosition = transform.position + movement;
        //newPosition.y = initialY + floatHeight; // 공중에 떠 있는 효과 (고정 높이)
        
        // 위치 업데이트
        transform.position = newPosition;
        
        // 이동 방향에 따라 스프라이트 뒤집기
        if (direction.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            
        // 애니메이션 설정 (있다면)
        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(direction.x) * moveSpeed);
    }
    
    // 공격 메서드 구현
    protected override void Attack()
    {
        // 이 메서드는 직접 호출되지 않고, FSM과 상태 클래스를 통해 관리됨
        // ExecuteAttack 메서드가 실제 공격 로직 처리
    }
    
    // 데미지를 입었을 때의 처리
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        
        // 데미지를 입었을 때 애니메이션 처리
        if (animator != null)
            animator.SetTrigger("Hit");
    }
    
    // 플레이어와 충돌 시 호출되는 메서드
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 오브젝트가 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어 컴포넌트 가져오기
            Player player = other.GetComponent<Player>();
            
            // 플레이어가 존재하면 데미지 적용
            if (player != null)
            {
                // 데미지 적용
                player.TakeDamage(damage);
                
            }
        }
    }
}

// 이동 상태
public class PaintMonsterMove : BaseState
{
    private PaintMonster monster;
    
    public PaintMonsterMove(PaintMonster m) : base(m)
    {
        monster = m;
    }
    
    public override void OnStateEnter()
    {
        // 상태 진입 시 필요한 처리
    }
    
    public override void OnStateUpdate()
    {
        // 플레이어 방향으로 이동
        monster.MoveTowardsPlayer();
        
        // 상태 전환은 PaintMonster의 checkeState()에서 처리
    }
    
    public override void OnStateExit()
    {
        // 상태 종료 시 필요한 처리
    }
}

// 공격 상태
public class PaintMonsterAttack : BaseState
{
    private PaintMonster monster;
    private bool hasAttacked;
    
    public PaintMonsterAttack(PaintMonster m) : base(m)
    {
        monster = m;
    }
    
    public override void OnStateEnter()
    {
        hasAttacked = false;
    }
    
    public override void OnStateUpdate()
    {
        // 공격 중에도 위치 유지 (이동은 하지 않고 Y축 높이만 유지)
        monster.MoveTowardsPlayer(0.0f);
        
        // 공격 실행 (한 번만)
        if (!hasAttacked)
        {
            monster.ExecuteAttack();
            hasAttacked = true;
        }
        
        // 공격 후 상태 전환은 애니메이션 이벤트가 OnAttackComplete()를 호출하여 처리
    }
    
    public override void OnStateExit()
    {
        // 상태 종료 시 필요한 처리
    }
}
