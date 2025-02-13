using UnityEngine;
using System.Collections.Generic;

// 쥐 보스 몬스터 클래스
public class Mouse : BossMonster
{
    public Animator animator { get; private set; }
    [SerializeField] public float moveSpeed = 3f; // 이동 속도

    // 쥐 몬스터의 행동 상태
    enum MouseAction
    {
        Move = 0,    // 이동 상태
        Attack = 1   // 공격 상태
    }

    // 현재 상태를 체크하고 상태 전환을 처리하는 메서드
    protected override void checkeState()
    {
        // 플레이어와의 거리를 체크하여 공격 범위 안에 있으면 공격 상태로 전환
        if (Vector3.Distance(playerTransform.position, transform.position) < attackRange)
        {
            monsterFSM.ChangeState(states[(int)MouseAction.Attack]);
        }
        else // 공격 범위 밖이면 이동 상태로 전환
        {
            monsterFSM.ChangeState(states[(int)MouseAction.Move]);
        }
    }

    // 초기 상태 설정 및 FSM 초기화
    protected override void initState()
    {
        animator = GetComponent<Animator>();

        states = new Dictionary<int, BaseState>
        {
            { (int)MouseAction.Move, new MouseMove(this, moveSpeed) },
            { (int)MouseAction.Attack, new MouseAttack(this) }
        };
        monsterFSM = new MonsterFSM(states[(int)MouseAction.Move]);
    }

    // 플레이어와 충돌 시 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //Attack
        }
    }
}

// 쥐 몬스터의 이동 상태를 처리하는 클래스
public class MouseMove : BaseState
{
    Mouse _monster;
    float moveSpeed = 3.0f;
    float prevXDir = 0f;  // 이전 프레임의 x축 방향
    
    public MouseMove(BossMonster bossMonster, float moveSpeed) : base(bossMonster)
    {
        _monster = (Mouse)bossMonster;
        this.moveSpeed = moveSpeed;
    }

    public override void OnStateEnter()
    {
        _monster.animator.SetTrigger("Move");
    }

    public override void OnStateExit() { }

    public override void OnStateUpdate()
    {
        // 플레이어 방향으로 이동
        var dir = GameManager.Instance.player.transform.position - _monster.transform.position;
        dir = dir.normalized;
        _monster.transform.position += moveSpeed * Time.deltaTime * dir;

        // x축 이동 방향이 바뀌었을 때 스프라이트 방향 전환
        if (prevXDir * dir.x < 0)  // 이전 방향과 현재 방향의 부호가 다르면
        {
            Vector3 scale = _monster.transform.localScale;
            scale.x *= -1;
            _monster.transform.localScale = scale;
        }
        prevXDir = dir.x;  // 현재 방향 저장
    }
}

// 쥐 몬스터의 공격 상태를 처리하는 클래스
public class MouseAttack : BaseState
{
    Mouse _monster;
    
    public MouseAttack(BossMonster bossMonster) : base(bossMonster)
    {
        _monster = (Mouse)bossMonster;
    }

    public override void OnStateEnter()
    {
        _monster.animator.SetTrigger("Attack");
    }

    public override void OnStateExit() { }

    public override void OnStateUpdate()
    {
        // 공격 로직 구현 필요
        Debug.Log("Mouse Attack");
    }
}
