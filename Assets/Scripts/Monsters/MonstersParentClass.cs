using System.Collections.Generic;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float damage;
    [SerializeField] protected float defense;
    [SerializeField] protected float detectionRange;
    protected bool isDead;
    protected Transform playerTransform;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public virtual void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(damage - defense, 1);
        currentHealth -= actualDamage;

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        // 사망 처리 로직
        //DropLoot();
    }

    //protected abstract void DropLoot();
    protected abstract void Attack();
}

// 일반 몬스터 클래스
public class NormalMonster : Monster
{
    [SerializeField] private float moveSpeed;
    public IMovement movement/* = Enter Movement*/;
    
    protected override void Start()
    {
        base.Start();
        // 일반 몬스터는 한 가지 이동 패턴만 사용
        if(movement == null){
            Debug.LogError("Enter Movement");
            Destroy(this);
        }
    }

    private void Update()
    {
        if (!isDead)
        {
            Move();
            CheckAttackRange();
        }
    }

    private void Move()
    {
        if (Vector2.Distance(transform.position, playerTransform.position) <= detectionRange)
        {
            Vector2 movementVector = movement.CalculateMovement(
                transform.position, 
                playerTransform.position
            );
            transform.position += (Vector3)movementVector * Time.deltaTime;
        }
    }

    protected virtual void CheckAttackRange()
    {
        // 공격 범위 체크 및 공격 로직
    }

    protected override void Attack()
    {
        throw new System.NotImplementedException();
    }
}
public enum BossState
{
    Idle,
    Chase,
    Attack1,
    Attack2,
    Berserk,
    StunState
}
public class BossMonster : Monster
{
    [SerializeField] private float phaseChangeHealthThreshold;
    [SerializeField] private float berserkHealthThreshold;
    private Dictionary<BossState, IMovement> stateMovements;
    private BossState currentState;
    private float stateTimer;

    // 각 상태별 지속 시간
    private Dictionary<BossState, float> stateDurations;

    protected override void Start()
    {
        base.Start();
        InitializeStates();
        SetState(BossState.Idle);
    }

    //상태 초기화
    protected virtual void InitializeStates()
    {
        /*
        // 상태별 이동 패턴 초기화
        stateMovements = new Dictionary<BossState, IMovement>
        {
            { BossState.Idle, null },
            { BossState.Chase, new StraightMovement(4f) },
            { BossState.Attack1, new ZigzagMovement(3f, 2f, 2f) },
            { BossState.Attack2, new StraightMovement(6f) },
            { BossState.Berserk, new ZigzagMovement(5f, 3f, 3f) }
        };

        // 상태별 지속 시간 설정
        stateDurations = new Dictionary<BossState, float>
        {
            { BossState.Idle, 2f },
            { BossState.Chase, 5f },
            { BossState.Attack1, 3f },
            { BossState.Attack2, 4f },
            { BossState.Berserk, 10f },
            { BossState.StunState, 3f }
        };
        */
    }

    private void Update()
    {
        if (!isDead)
        {
            UpdateState();
            PerformStateAction();
        }
    }

    private void UpdateState()
    {
        stateTimer -= Time.deltaTime;
        
        // 상태 전환 조건 체크
        if (stateTimer <= 0)
        {
            ChooseNextState();
        }

        // 체력에 따른 특수 상태 전환
        if (currentHealth <= berserkHealthThreshold && currentState != BossState.Berserk)
        {
            SetState(BossState.Berserk);
        }
    }

    private void ChooseNextState()
    {
        // 현재 체력과 상황에 따라 다음 상태 결정
        BossState nextState = currentHealth <= phaseChangeHealthThreshold 
            ? GetPhase2State() 
            : GetPhase1State();
            
        SetState(nextState);
    }

    private BossState GetPhase1State()
    {
        // Phase 1의 상태 선택 로직
        float random = Random.value;
        if (random < 0.4f) return BossState.Chase;
        if (random < 0.7f) return BossState.Attack1;
        return BossState.Attack2;
    }

    private BossState GetPhase2State()
    {
        // Phase 2의 상태 선택 로직
        float random = Random.value;
        if (random < 0.3f) return BossState.Chase;
        if (random < 0.6f) return BossState.Attack1;
        if (random < 0.9f) return BossState.Attack2;
        return BossState.Berserk;
    }

    private void SetState(BossState newState)
    {
        currentState = newState;
        stateTimer = stateDurations[newState];
        // 상태 진입 시 추가 로직
    }

    private void PerformStateAction()
    {
        // 현재 상태에 따른 행동 수행
        if (stateMovements.TryGetValue(currentState, out IMovement movement) && movement != null)
        {
            Vector2 movementVector = movement.CalculateMovement(
                transform.position, 
                playerTransform.position
            );
            transform.position += (Vector3)movementVector * Time.deltaTime;
        }

        switch (currentState)
        {
            case BossState.Attack1:
                PerformAttack1();
                break;
            case BossState.Attack2:
                PerformAttack2();
                break;
            case BossState.Berserk:
                PerformBerserkAttack();
                break;
        }
    }

    private void PerformAttack1()
    {
        // 공격 패턴 1 구현
    }

    private void PerformAttack2()
    {
        // 공격 패턴 2 구현
    }

    private void PerformBerserkAttack()
    {
        // 광폭화 상태 공격 구현
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        
        // 보스 특수 피해 처리
        if (currentHealth <= phaseChangeHealthThreshold)
        {
            // 페이즈 변경 로직
        }
    }
    protected override void Attack()
    {
        // 기본 공격 구현
    }
}