using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public abstract class AttackObject : MonoBehaviour
{
    public float attackDamage;
    public int attackIntervalInTicks;
    protected List<Weapon.MonsterHit> monstersHit;
    protected List<Collider2D> hits;
    protected PolygonCollider2D polygonCollider;

    public virtual void Init(float attackDamage, float attackRange, int attackIntervalInTicks)
    {
        this.attackDamage = attackDamage;
        this.attackIntervalInTicks = attackIntervalInTicks;
        transform.localScale = new Vector3(attackRange, attackRange, 1);
    }

    protected virtual void Awake()
    {
        monstersHit = new List<Weapon.MonsterHit>();
        hits = new List<Collider2D>();
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
