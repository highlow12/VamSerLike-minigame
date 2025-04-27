using UnityEngine;
using System.Collections.Generic;

public class KingMovement : BossMonster
{
    public Animator animator { get; private set; }

    [SerializeField] public float moveSpeed = 1;
    enum KingAction
    {
        Move = 0,
        Attack = 1
    }
    protected override void checkeState()
    {
        // 계속 플레이어를 따라다니다 공격 범위안에 들어오면 공격
        if (Vector3.Distance(playerTransform.position, transform.position) < attackRange)
        {
            monsterFSM.ChangeState(states[(int)KingAction.Attack]);
        }
        else
        {
            monsterFSM.ChangeState(states[(int)KingAction.Move]);
        }
    }

    protected override void initState()
    {
        animator = GetComponent<Animator>();

        states = new Dictionary<int, BaseState>
        {
            { (int)KingAction.Move, new KingMove(this, moveSpeed) },
            { (int)KingAction.Attack, new KingAttack(this) }
        };
        monsterFSM = new MonsterFSM(states[(int)KingAction.Move]);
    }

    //트리거로 플레이어 공격
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //Attack
        }
    }
}

public class KingMove : BaseState
{
    KingMovement _monster;
    float moveSpeed = 1.0f;
    public KingMove(BossMonster bossMonster, float moveSpeed) : base(bossMonster)
    {
        _monster = (KingMovement)bossMonster;
        this.moveSpeed = moveSpeed;
    }

    public override void OnStateEnter()
    {
        _monster.animator.SetTrigger("Move");
        // 이동 사운드 재생
        _monster.PlayMoveSound();
    }

    public override void OnStateExit()
    {
        // 이동 상태 종료시 사운드 중지
        _monster.StopMoveSound();
    }

    public override void OnStateUpdate()
    {
        var dir = GameManager.Instance.player.transform.position - _monster.transform.position;
        dir = dir.normalized;

        _monster.transform.position += dir * moveSpeed * Time.deltaTime;
    }
}

public class KingAttack : BaseState
{
    KingMovement _monster;
    private bool hasPlayedAttackSound = false;
    
    public KingAttack(BossMonster bossMonster) : base(bossMonster)
    {
        _monster = (KingMovement)bossMonster;
    }
    
    public override void OnStateEnter()
    {
        // VFXManager.Instance.AnimateShake();
        _monster.animator.SetTrigger("Attack");
        hasPlayedAttackSound = false;
    }

    public override void OnStateExit()
    {
        // 상태 전환 시 변수 초기화
        hasPlayedAttackSound = false;
    }

    public override void OnStateUpdate()
    {
        // 애니메이션 시작 부분에 한 번만 사운드 재생
        if (!hasPlayedAttackSound)
        {
            _monster.PlayAttackSound();
            hasPlayedAttackSound = true;
        }
        
        Debug.Log("Attack");
    }
}
