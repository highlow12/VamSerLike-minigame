using System.Collections.Generic;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float damage;
    [SerializeField] protected float defense;

    protected bool isDead;
    protected Transform playerTransform;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        playerTransform = GameManager.Instance.player.transform;
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
    protected IMovement movement/* = Enter Movement*/;
    
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
        Vector2 movementVector = movement.CalculateMovement(
            transform.position, 
            playerTransform.position
        );
        transform.position += (Vector3)movementVector * Time.deltaTime * moveSpeed;
    }
// Monster Attack
    protected virtual void CheckAttackRange()
    {
        throw new System.NotImplementedException();
    }

    protected override void Attack()
    {
        throw new System.NotImplementedException();
    }
}

// 보스 몬스터

public abstract class BossMonster : Monster
{
    [SerializeField] private float phaseChangeHealthThreshold;
    [SerializeField] private float berserkHealthThreshold;
    protected Dictionary<int, BaseState> states;
    private float stateTimer;
    protected MonsterFSM monsterFSM;


    protected override void Start()
    {
        base.Start();
        initState();
    }
    protected abstract void initState();
    protected abstract void checkeState();
    public void Update()
    {
        monsterFSM.UpdateState();
        checkeState();
    }
// Boss Take Damage
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
        throw new System.NotImplementedException();
    }
}