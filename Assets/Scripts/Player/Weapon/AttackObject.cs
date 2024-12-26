using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public abstract class AttackObject : MonoBehaviour
{
    public float attackDamage;
    public int attackIntervalInTicks;
    public GameObject colliderObject;
    protected int attackTarget;
    protected List<Weapon.MonsterHit> monstersHit;
    protected List<Collider2D> hits;
    protected Collider2D attackCollider;
    protected LayerMask monsterLayer;
    protected ContactFilter2D contactFilter;


    public virtual void Init(float attackDamage, float attackRange, int attackIntervalInTicks, int attackTarget)
    {
        this.attackDamage = attackDamage;
        this.attackIntervalInTicks = attackIntervalInTicks;
        this.attackTarget = attackTarget;
        transform.localScale = new Vector3(0.5f, 0.5f, 1);
    }

    protected virtual void Awake()
    {
        monstersHit = new List<Weapon.MonsterHit>();
        hits = new List<Collider2D>();
        monsterLayer = 1 << LayerMask.NameToLayer("Monster");
        contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = true;
        contactFilter.SetLayerMask(monsterLayer);
    }

    protected virtual void Start()
    {
        monstersHit.Clear();
        hits.Clear();
    }

    public virtual void Attack()
    {

    }

}
