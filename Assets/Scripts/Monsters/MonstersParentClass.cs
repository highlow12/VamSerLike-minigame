using System.Collections.Generic;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 1;
    [SerializeField] protected float currentHealth = 1;
    [SerializeField] protected float damage = 1;
    [SerializeField] protected float attackRange = 0.1f;

    protected bool isDead;
    protected Transform playerTransform;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        playerTransform = GameManager.Instance.player.transform;
    }

    public virtual void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(damage, 1);
        currentHealth -= actualDamage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
#if UNITY_EDITOR
        Debug.Log("Dead: " + gameObject.name);
#endif
        isDead = true;
        DropLoot();
        MonsterPoolManager.Instance.ReturnMonsterToPool(gameObject,GetComponent<MonsterIdentify>().monsterName);
        // 사망 처리 로직
        
    }

    protected virtual void DropLoot()
    {
        // 임시 로직
        DropItemManager.Instance.DropItem(transform.position);
    }

    //protected abstract void DropLoot();
    protected abstract void Attack();

    public float DemoAttack()
    {
        return damage;
    }

    /// <summary>
    /// Resets the monster's health to its maximum value.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    /// <summary>
    /// Resets the monster's position to a default or predefined value.
    /// </summary>
    public void ResetPosition()
    {
        transform.position = Vector3.zero; // 기본 위치로 초기화 (필요에 따라 수정 가능)
    }
}


// 일반 몬스터 클래스
public class NormalMonster : Monster
{
    [SerializeField] protected float moveSpeed = 1;
    protected IMovement movement/* = Enter Movement*/;

    protected override void Start()
    {
        base.Start();
        // 일반 몬스터는 한 가지 이동 패턴만 사용
        if (movement == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Enter Movement");
#endif
            Destroy(this);
        }
    }

    protected virtual void Update()
    {
        if (!isDead)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) < attackRange)
            {
                CheckAttackRange();
            }
            else
            {
                Move();
            }

            

        }
    }

    /// <summary>
    /// Moves the monster based on the movement behavior.
    /// </summary>
    private void Move()
    {
        // Calculate movement using the movement interface
        movement.CalculateMovement(transform);

        // Assuming movementVector is updated within CalculateMovement
        Vector2 movementVector = transform.position - (Vector3)playerTransform.position;
        int sign = Mathf.Sign(transform.localScale.x) == Mathf.Sign(movementVector.x) ? 1 : -1;
        // Update local scale based on movement direction
        transform.localScale = new Vector3(
            transform.localScale.x * sign,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    /// <summary>
    /// Checks if the player is within attack range.
    /// </summary>
    protected virtual void CheckAttackRange()
    {
#if UNITY_EDITOR
        ////throw new System.NotImplementedException();
#endif
    }

    /// <summary>
    /// Executes the monster's attack behavior.
    /// </summary>
    protected override void Attack()
    {
#if UNITY_EDITOR
        ////throw new System.NotImplementedException();
#endif
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
#if UNITY_EDITOR
        //throw new System.NotImplementedException();
#endif
    }
}