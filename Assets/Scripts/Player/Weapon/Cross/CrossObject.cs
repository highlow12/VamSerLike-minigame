using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;


public class CrossObject : AttackObject
{
    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks = 0, int attackTarget = 0)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
    }

    protected override void Awake()
    {
        base.Awake();
        attackCollider = GetComponentInChildren<PolygonCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Attack()
    {
        Physics2D.OverlapCollider(attackCollider, hits);
        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                break;
            }
            Monster monster = hit.GetComponent<Monster>();
            if (monster == null)
            {
                continue;
            }
            Debug.Log($"[{GameManager.Instance.gameTimer}] CrossObject hit {monster.name}");
            monster.TakeDamage(attackDamage);
        }
    }
}
