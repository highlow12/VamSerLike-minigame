using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class QueenMovement : BossMonster
{
    public Animator animator { get; private set; }
    float stateTimer;
    [SerializeField] public float moveSpeed = 1;
    [SerializeField] public float idleRange = 5;
    public bool canTakeDamage = false;
    public float[] chaseTimeRange = new float[2];
    public float[] idleTimerRange = new float[2];
    enum QueenAction
    {
        Idle = 0,
        Chase = 1
    }
    protected override void checkeState()
    {
        // stateTimer를 지난 프레임 이후 경과한 시간만큼 감소시킨다
        stateTimer -= Time.deltaTime;

        // if를 사용한 이유: switch의 case문은 컴파일 시에 값이 정해져 있어야 한다
        if (monsterFSM.currentState == states[(int)QueenAction.Idle])
        {
            // stateTimer가 0보다 크면 함수를 종료한다
            if (stateTimer > 0) return;

            // stateTimer를 chaseTimeRange 범위 내의 랜덤 값으로 설정한다
            stateTimer = Random.Range(chaseTimeRange.Min(), chaseTimeRange.Max());

            // 몬스터의 상태를 Chase로 변경한다
            monsterFSM.ChangeState(states[(int)QueenAction.Chase]);
        }
        else if (monsterFSM.currentState == states[(int)QueenAction.Chase])
        {
            // stateTimer가 0보다 크면 함수를 종료한다
            if (stateTimer > 0) return;

            // stateTimer를 idleTimerRange 범위 내의 랜덤 값으로 설정한다
            stateTimer = Random.Range(idleTimerRange.Min(), idleTimerRange.Max());

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
    }

    // 상태가 종료될 때 호출되는 메서드
    public override void OnStateExit()
    {
        // 현재는 아무 동작도 하지 않음
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
    public QueenChase(Monster _monster, float moveSpeed) : base(_monster)
    {
        this._monster = (QueenMovement)_monster;
        this.moveSpeed = moveSpeed;
    }

    public override void OnStateEnter()
    {
        _monster.canTakeDamage = false;
        _monster.animator.SetTrigger("Move");
    }

    public override void OnStateExit()
    {

    }

    public override void OnStateUpdate()
    {
        var dir = GameManager.Instance.player.transform.position - _monster.transform.position;
        dir = dir.normalized;

        _monster.transform.position += dir * moveSpeed * Time.deltaTime;

        Debug.Log("Chase");
    }
}
