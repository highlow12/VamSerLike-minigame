using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class QueenMovement : BossMonster
{
    public Animator animator { get; private set; }
    float StateTimer;
    [SerializeField] public float moveSpeed = 1;
    [SerializeField] public float idleRange = 5;
    // Removed duplicate attackRange field - we'll use the one inherited from the parent class
    public bool canTakeDamage = false;
    public float[] chaseTimeRange = new float[2];
    public float[] idleTimerRange = new float[2];
    enum QueenAction
    {
        Idle = 0,
        Chase = 1
    }
    
    // Initialize the StateTimer in the Start or Awake method
    protected override void Start()
    {
        base.Start();
        // Initialize with a reasonable default value
        StateTimer = 0;
    }
    
    protected override void checkeState()
    {
        // StateTimer를 지난 프레임 이후 경과한 시간만큼 감소시킨다
        StateTimer -= Time.deltaTime;

        // if를 사용한 이유: switch의 case문은 컴파일 시에 값이 정해져 있어야 한다
        if (monsterFSM.currentState == states[(int)QueenAction.Idle])
        {
            // StateTimer가 0보다 크면 함수를 종료한다
            if (StateTimer > 0) return;

            // StateTimer를 chaseTimeRange 범위 내의 랜덤 값으로 설정한다
            StateTimer = Random.Range(chaseTimeRange.Min(), chaseTimeRange.Max());

            // 몬스터의 상태를 Chase로 변경한다
            monsterFSM.ChangeState(states[(int)QueenAction.Chase]);
        }
        else if (monsterFSM.currentState == states[(int)QueenAction.Chase])
        {
            // StateTimer가 0보다 크면 함수를 종료한다
            if (StateTimer > 0) return;

            // StateTimer를 idleTimerRange 범위 내의 랜덤 값으로 설정한다
            StateTimer = Random.Range(idleTimerRange.Min(), idleTimerRange.Max());

            // 몬스터의 상태를 Idle로 변경한다
            monsterFSM.ChangeState(states[(int)QueenAction.Idle]);
        }

    }

    protected override void initState()
    {
        animator = GetComponent<Animator>();

        states = new Dictionary<int, BaseState>
        {
            { (int)QueenAction.Idle, new QueenIdle(this, idleRange, moveSpeed) },
            { (int)QueenAction.Chase, new QueenChase(this, moveSpeed) }
        };
        monsterFSM = new MonsterFSM(states[(int)QueenAction.Idle]);
    }

    public override void TakeDamage(float damage)
    {
        if (!canTakeDamage) return;
        base.TakeDamage(damage);
    }

    protected override void Die()
    {
        VFXManager.Instance.Normalize();
        base.Die();
    }
}

class QueenIdle : BaseState
{
    private QueenMovement _monster;
    private Vector2 targetPos;
    private float idleRange;
    private float moveSpeed;

    // QueenIdle 상태를 초기화하는 생성자
    public QueenIdle(Monster _monster, float idleRange, float moveSpeed) : base(_monster)
    {
        this._monster = (QueenMovement)_monster;
        this.idleRange = idleRange;
        this.moveSpeed = moveSpeed;
    }

    // 상태가 시작될 때 호출되는 메서드
    public override void OnStateEnter()
    {
        // 몬스터가 데미지를 받을 수 있도록 설정
        _monster.canTakeDamage = true;
        // 목표 위치를 idleRange 내의 랜덤 위치로 설정
        targetPos = Random.insideUnitCircle * idleRange + (Vector2)GameManager.Instance.player.transform.position;

        _monster.animator.SetTrigger("Dance");
        
        // 춤을 출 때도 부드럽게 이동하기 때문에 이동 사운드 재생
        _monster.PlayMoveSound();
    }

    // 상태가 종료될 때 호출되는 메서드
    public override void OnStateExit()
    {
        // 상태 전환 시 이동 사운드 중지
        _monster.StopMoveSound();
    }

    // 상태가 업데이트될 때 호출되는 메서드
    public override void OnStateUpdate()
    {
        // 몬스터가 목표 위치에 거의 도달했는지 확인
        if (Vector2.Distance(_monster.transform.position, targetPos) < 0.1f)
        {
            // 새로운 목표 위치를 idleRange 내의 랜덤 위치로 설정
            targetPos = Random.insideUnitCircle * idleRange + (Vector2)GameManager.Instance.player.transform.position;
        }

        // 몬스터를 목표 위치로 이동
        _monster.transform.position = Vector2.MoveTowards(_monster.transform.position, targetPos, moveSpeed * Time.deltaTime);
        Debug.Log("Idle");
    }
}
class QueenChase : BaseState
{
    QueenMovement _monster;
    private float moveSpeed;
    private bool hasPlayedAttackSound = false;
    
    public QueenChase(Monster _monster, float moveSpeed) : base(_monster)
    {
        this._monster = (QueenMovement)_monster;
        this.moveSpeed = moveSpeed;
    }

    public override void OnStateEnter()
    {
        VFXManager.Instance.AnimateNoise();
        _monster.canTakeDamage = false;
        _monster.animator.SetTrigger("Move");
        
        // 퀸 추격 모드는 공격적인 움직임이므로 공격 사운드도 재생
        _monster.PlayAttackSound();
        hasPlayedAttackSound = true;
        
        // 이동 사운드도 재생
        _monster.PlayMoveSound();
    }

    public override void OnStateExit()
    {
        VFXManager.Instance.Normalize();
        
        // 상태 전환 시 이동 사운드 중지
        _monster.StopMoveSound();
    }

    public override void OnStateUpdate()
    {
        var dir = GameManager.Instance.player.transform.position - _monster.transform.position;
        dir = dir.normalized;

        _monster.transform.position += dir * moveSpeed * Time.deltaTime;
        
        // 일정 시간마다 공격 사운드 재생 (약 3초 간격)
        if (hasPlayedAttackSound && Time.time % 3 < 0.1f)
        {
            _monster.PlayAttackSound();
        }

        Debug.Log("Chase");
    }
}
