using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureGhost : BossMonster
{
    public Animator animator { get; private set; }
    public GameObject iceShardPrefab;
    public GameObject glacierPrefab;
    public float skateSpeed = 6f;
    public float iceShardDamage = 1f;
    public float iceShardSpeed = 8f;
    public float iceShardLifetime = 3f;
    public float glacierInterval = 2f;
    public float skateDuration = 3f;
    public float attackDuration = 3f;
    public float glacierDuration = 4f;

    private bool isInvincible = false;
    private FigureGhostAction currentAction = FigureGhostAction.SkateMove;
    private float stateTimer = 0f;

    enum FigureGhostAction { SkateMove = 0, IceShardAttack = 1, GlacierAttack = 2 }

    protected override void initState()
    {
        animator = GetComponent<Animator>();
        states = new Dictionary<int, BaseState>
        {
            { (int)FigureGhostAction.SkateMove, new FigureGhostSkateMove(this, skateSpeed, skateDuration) },
            { (int)FigureGhostAction.IceShardAttack, new FigureGhostIceShardAttack(this, attackDuration) },
            { (int)FigureGhostAction.GlacierAttack, new FigureGhostGlacierAttack(this, glacierDuration, glacierInterval) }
        };
        monsterFSM = new MonsterFSM(states[(int)FigureGhostAction.SkateMove]);
    }

    protected override void checkeState()
    {
        // 상태별 전환 조건만 판단, 실제 동작은 상태 클래스에서 처리
        switch (currentAction)
        {
            case FigureGhostAction.SkateMove:
                if (states[(int)FigureGhostAction.SkateMove] is FigureGhostSkateMove skate && skate.IsEnd)
                    ChangeState(FigureGhostAction.IceShardAttack);
                break;
            case FigureGhostAction.IceShardAttack:
                if (states[(int)FigureGhostAction.IceShardAttack] is FigureGhostIceShardAttack atk && atk.IsEnd)
                    ChangeState(FigureGhostAction.GlacierAttack);
                break;
            case FigureGhostAction.GlacierAttack:
                if (states[(int)FigureGhostAction.GlacierAttack] is FigureGhostGlacierAttack gla && gla.IsEnd)
                    ChangeState(FigureGhostAction.SkateMove);
                break;
        }
    }

    private void ChangeState(FigureGhostAction nextAction)
    {
        currentAction = nextAction;
        monsterFSM.ChangeState(states[(int)nextAction]);
    }

    public override void TakeDamage(float damage)
    {
        if (isInvincible) return;
        base.TakeDamage(damage);
    }
    public void SetInvincible(bool value) => isInvincible = value;
    public void FireIceShard()
    {
        if (iceShardPrefab == null) return;
        var go = Instantiate(iceShardPrefab, transform.position, Quaternion.identity);
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        var proj = go.GetComponent<FigureGhostIceShardProjectile>();
        if (proj != null)
        {
            proj.direction = dir;
            proj.damage = iceShardDamage;
            proj.speed = iceShardSpeed;
            proj.lifetime = iceShardLifetime;
        }
    }
    public void SpawnGlacier()
    {
        if (glacierPrefab == null) return;
        Vector2 playerPos = playerTransform.position;
        Vector2 rand = Random.insideUnitCircle.normalized * Random.Range(2f, 5f);
        Vector2 spawnPos = playerPos + rand;
        Instantiate(glacierPrefab, spawnPos, Quaternion.identity);
    }
}

// 상태별 동작은 BaseState 파생 클래스에서 처리
class FigureGhostSkateMove : BaseState
{
    FigureGhost ghost;
    float speed, duration, timer;
    public bool IsEnd { get; private set; }
    public FigureGhostSkateMove(FigureGhost ghost, float speed, float duration) : base(ghost)
    {
        this.ghost = ghost; this.speed = speed; this.duration = duration;
    }
    public override void OnStateEnter()
    {
        timer = duration; IsEnd = false;
        ghost.SetInvincible(true);
        ghost.animator?.SetTrigger("Skate");
    }
    public override void OnStateUpdate()
    {
        timer -= Time.deltaTime;
        Vector2 dir = (GameManager.Instance.player.transform.position - ghost.transform.position).normalized;
        ghost.transform.position += (Vector3)dir * speed * Time.deltaTime;
        if (timer <= 0f) IsEnd = true;
    }
    public override void OnStateExit() { ghost.SetInvincible(false); }
}
class FigureGhostIceShardAttack : BaseState
{
    FigureGhost ghost;
    float duration, timer, fireTimer;
    public bool IsEnd { get; private set; }
    public FigureGhostIceShardAttack(FigureGhost ghost, float duration) : base(ghost)
    {
        this.ghost = ghost; this.duration = duration;
    }
    public override void OnStateEnter()
    {
        timer = duration; fireTimer = 0f; IsEnd = false;
        ghost.SetInvincible(false);
        ghost.animator?.SetTrigger("Attack");
    }
    public override void OnStateUpdate()
    {
        timer -= Time.deltaTime; fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            ghost.FireIceShard();
            fireTimer = 1f;
        }
        if (timer <= 0f) IsEnd = true;
    }
    public override void OnStateExit() { }
}
class FigureGhostGlacierAttack : BaseState
{
    FigureGhost ghost;
    float duration, timer, interval, glacierTimer;
    public bool IsEnd { get; private set; }
    public FigureGhostGlacierAttack(FigureGhost ghost, float duration, float interval) : base(ghost)
    {
        this.ghost = ghost; this.duration = duration; this.interval = interval;
    }
    public override void OnStateEnter()
    {
        timer = duration; glacierTimer = 0f; IsEnd = false;
        ghost.SetInvincible(false);
        ghost.animator?.SetTrigger("Glacier");
    }
    public override void OnStateUpdate()
    {
        timer -= Time.deltaTime; glacierTimer -= Time.deltaTime;
        if (glacierTimer <= 0f)
        {
            ghost.SpawnGlacier();
            glacierTimer = interval;
        }
        if (timer <= 0f) IsEnd = true;
    }
    public override void OnStateExit() { }
}

public class FigureGhostIceShardProjectile : MonoBehaviour
{
    public Vector2 direction;
    public float damage;
    public float speed;
    public float lifetime;
    float timer;
    void Start() { timer = lifetime; }
    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        timer -= Time.deltaTime;
        if (timer <= 0f) Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            var player = col.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                player.ApplySpecialEffect(Player.PlayerSpecialEffect.Stun, 2f);
            }
            Destroy(gameObject);
        }
    }
}
