using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : BossMonster
{
    public Animator animator { get; private set; }
    public GameObject iceShardPrefab; // 얼음 파편 프리팹
    [SerializeField] public float moveSpeed = 2f;
    [SerializeField] public new float attackRange = 5f;

    [Header("Ice Shard Settings")]
    public float iceDamage = 1f;
    public float iceSpeed = 5f;
    public float iceLifetime = 3f;

    // playerTransform을 안전하게 노출
    public Transform PlayerTransform => playerTransform;

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
            { (int)HunterAction.Move, new HunterMove(this, moveSpeed) },
            { (int)HunterAction.Attack, new HunterAttack(this) }
        };
        monsterFSM = new MonsterFSM(states[(int)HunterAction.Move]);
    }

    protected override void checkeState()
    {
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist < attackRange)
        {
            if (!(monsterFSM.currentState is HunterAttack))
                monsterFSM.ChangeState(states[(int)HunterAction.Attack]);
        }
        else
        {
            if (!(monsterFSM.currentState is HunterMove))
                monsterFSM.ChangeState(states[(int)HunterAction.Move]);
        }
    }
}

// 이동 상태: 팔로 거꾸로 걷기, 예측 불가 이동
class HunterMove : BaseState
{
    Hunter hunter;
    float moveSpeed;
    float directionTimer = 0f;
    Vector2 moveDir;
    public HunterMove(BossMonster m, float moveSpeed) : base(m)
    {
        hunter = (Hunter)m;
        this.moveSpeed = moveSpeed;
    }
    public override void OnStateEnter()
    {
        hunter.animator.SetTrigger("Move");
        directionTimer = 0f;
        moveDir = GetRandomDirection();
    }
    public override void OnStateExit() { }
    public override void OnStateUpdate()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            moveDir = GetRandomDirection();
            directionTimer = Random.Range(0.5f, 1.5f); // 예측 불가하게 방향 전환
        }
        // 거꾸로(팔로) 걷는 연출: y축을 뒤집거나, 애니메이션에서 처리
        hunter.transform.localScale = new Vector3(hunter.transform.localScale.x, -1, hunter.transform.localScale.z);
        hunter.transform.position += (Vector3)moveDir * moveSpeed * Time.deltaTime;
    }
    Vector2 GetRandomDirection()
    {
        // 플레이어 방향 + 랜덤성
        Vector2 toPlayer = (hunter.PlayerTransform.position - hunter.transform.position).normalized;
        Vector2 random = Random.insideUnitCircle.normalized;
        return (toPlayer + random * 0.7f).normalized;
    }
}

// 공격 상태: 0.5초마다 얼음 파편 발사
class HunterAttack : BaseState
{
    Hunter hunter;
    Coroutine attackRoutine;
    public HunterAttack(BossMonster m) : base(m)
    {
        hunter = (Hunter)m;
    }
    public override void OnStateEnter()
    {
        hunter.animator.SetTrigger("Attack");
        attackRoutine = hunter.StartCoroutine(AttackRoutine());
    }
    public override void OnStateExit()
    {
        if (attackRoutine != null)
            hunter.StopCoroutine(attackRoutine);
    }
    public override void OnStateUpdate()
    {
        // 공격 중에도 플레이어를 바라보게
        Vector2 dir = hunter.PlayerTransform.position - hunter.transform.position;
        if (dir.x != 0)
            hunter.transform.localScale = new Vector3(Mathf.Sign(dir.x), hunter.transform.localScale.y, hunter.transform.localScale.z);
    }
    IEnumerator AttackRoutine()
    {
        while (true)
        {
            FireIceShard();
            yield return new WaitForSeconds(0.5f);
        }
    }
    void FireIceShard()
    {
        if (hunter.iceShardPrefab == null) return;
        var go = GameObject.Instantiate(hunter.iceShardPrefab, hunter.transform.position, Quaternion.identity);
        Vector2 dir = (GameManager.Instance.player.transform.position - hunter.transform.position).normalized;
        var snowball = go.GetComponent<SnowballProjectile>();
        if (snowball != null)
        {
            snowball.direction = dir;
            snowball.damage = hunter.iceDamage;
            snowball.speed = hunter.iceSpeed;
            snowball.lifetime = hunter.iceLifetime;
        }
    }
}
