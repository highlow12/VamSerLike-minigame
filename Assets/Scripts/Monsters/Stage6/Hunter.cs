using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : BossMonster
{
    public Animator animator { get; private set; }
    public GameObject iceShardPrefab;
    public float moveSpeed = 2f;
    public float attackRange = 5f;
    public float minDistance = 2f;
    public float maxDistance = 7f;
    public float stateInterval = 0.5f;
    public float iceDamage = 1f;
    public float iceSpeed = 5f;
    public float iceLifetime = 3f;

    enum HunterAction { Move = 0, Attack = 1 }

    protected override void Start()
    {
        base.Start();
    }

    protected override void initState()
    {
        animator = GetComponent<Animator>();
        states = new Dictionary<int, BaseState>
        {
            { (int)HunterAction.Move, new HunterMove(this, moveSpeed, stateInterval, minDistance, maxDistance) },
            { (int)HunterAction.Attack, new HunterAttack(this, stateInterval) }
        };
        monsterFSM = new MonsterFSM(states[(int)HunterAction.Move]);
    }

    protected override void checkeState()
    {
        // 상태 전환은 각 상태에서 직접 처리
    }

    // 투사체 발사 함수
    public void FireIceShard()
    {
        if (iceShardPrefab == null) return;
        var go = Instantiate(iceShardPrefab, transform.position, Quaternion.identity);
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        var snowball = go.GetComponent<SnowballProjectile>();
        if (snowball != null)
        {
            snowball.direction = dir;
            snowball.damage = iceDamage;
            snowball.speed = iceSpeed;
            snowball.lifetime = iceLifetime;
        }
    }

    public void ChangeToMoveState()
    {
        monsterFSM.ChangeState(states[0]);
    }
    public void ChangeToAttackState()
    {
        monsterFSM.ChangeState(states[1]);
    }
}

// 이동 상태
class HunterMove : BaseState
{
    Hunter hunter;
    float moveSpeed;
    float stateInterval;
    float minDistance, maxDistance;
    float timer;
    Vector2 moveDir;

    public HunterMove(BossMonster m, float moveSpeed, float stateInterval, float minDistance, float maxDistance) : base(m)
    {
        hunter = (Hunter)m;
        this.moveSpeed = moveSpeed;
        this.stateInterval = stateInterval;
        this.minDistance = minDistance;
        this.maxDistance = maxDistance;
    }

    public override void OnStateEnter()
    {
        hunter.animator.SetTrigger("Move");
        timer = stateInterval;
        moveDir = GetMoveDirection();
    }

    public override void OnStateUpdate()
    {
        timer -= Time.deltaTime;
        hunter.transform.position += (Vector3)moveDir * moveSpeed * Time.deltaTime;
        if (timer <= 0f)
        {
            hunter.ChangeToAttackState(); // Attack 상태로
        }
    }

    public override void OnStateExit() { }

    Vector2 GetMoveDirection()
    {
        var player = GameManager.Instance.player.transform.position;
        float dist = Vector2.Distance(hunter.transform.position, player);
        Vector2 toPlayer = (player - hunter.transform.position).normalized;
        Vector2 random = Random.insideUnitCircle.normalized;
        if (dist > maxDistance)
        {
            return (toPlayer + random * 0.7f).normalized;
        }
        else if (dist < minDistance)
        {
            return ((-toPlayer) + random * 0.7f).normalized;
        }
        else
        {
            return random;
        }
    }
}

// 공격 상태
class HunterAttack : BaseState
{
    Hunter hunter;
    float stateInterval;
    float timer;

    public HunterAttack(BossMonster m, float stateInterval) : base(m)
    {
        hunter = (Hunter)m;
        this.stateInterval = stateInterval;
    }

    public override void OnStateEnter()
    {
        hunter.animator.SetTrigger("Attack");
        timer = stateInterval;
        hunter.FireIceShard();
    }

    public override void OnStateUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            hunter.ChangeToMoveState(); // Move 상태로
        }
    }

    public override void OnStateExit() { }
}
