using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;


public class AxeObject : AttackObject
{
    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks = 0, int attackTarget = 0)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
    }

    protected override void Awake()
    {
        base.Awake();
        attackCollider = GetComponent<BoxCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Attack()
    {
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
            Debug.Log($"[{GameManager.Instance.gameTimer}] AxeObject hit {monster.name}");
            monster.TakeDamage(attackDamage);
            count++;
        }
    }
}
