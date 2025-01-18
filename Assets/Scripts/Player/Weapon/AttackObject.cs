using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public abstract class AttackObject : MonoBehaviour
{
    public float attackDamage;
    public float attackRange;
    public int attackIntervalInTicks;
    public GameObject colliderObject;
    public SubWeaponSO subWeaponSO;
    protected int attackTarget;
    protected List<Weapon.MainWeapon.MonsterHit> monstersHit;
    protected List<Collider2D> hits;
    protected Collider2D attackCollider;
    protected LayerMask monsterLayer;
    protected ContactFilter2D contactFilter;
    // Sub weapon properties
    protected new Animation animation;
    protected SpriteRenderer spriteRenderer;

    public virtual void Setup(float baseAngle, float spreadDegree, int projectileCount, float projectileSpeed, float attackSpeed)
    {

    }

    public virtual void Init(float attackDamage, float attackRange, int attackIntervalInTicks, int attackTarget)
    {
        this.attackDamage = attackDamage;
        this.attackIntervalInTicks = attackIntervalInTicks;
        this.attackTarget = attackTarget;
        this.attackRange = attackRange;
    }

    public virtual void SubWeaponInit()
    {
        animation = GetComponent<Animation>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = subWeaponSO.weaponSprite;
        if (subWeaponSO.attackAnimation != null)
        {
            animation.AddClip(subWeaponSO.attackAnimation, "Attack");
        }
        if (subWeaponSO.hitAnimation != null)
        {
            animation.AddClip(subWeaponSO.hitAnimation, "Hit");
        }
    }

    protected virtual void Awake()
    {
        monstersHit = new List<Weapon.MainWeapon.MonsterHit>();
        hits = new List<Collider2D>();
        monsterLayer = 1 << LayerMask.NameToLayer("Monster");
        contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = true;
        contactFilter.SetLayerMask(monsterLayer);
        attackCollider = colliderObject.GetComponent<Collider2D>() ?? new Collider2D();

    }

    protected virtual void Start()
    {
        monstersHit.Clear();
        hits.Clear();
    }

    public virtual void Attack()
    {
        Start();
        Physics2D.OverlapCollider(attackCollider, contactFilter, hits);
        hits = hits.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToList();
        int count = 0;
        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                break;
            }
            if (count >= attackTarget)
            {
                break;
            }
            Monster monster = hit.GetComponent<Monster>();
            if (monster == null)
            {
                continue;
            }
            Debug.Log($"[{GameManager.Instance.gameTimer}] hit {monster.name}");
            monster.TakeDamage(attackDamage);
            count++;
        }
    }

}
