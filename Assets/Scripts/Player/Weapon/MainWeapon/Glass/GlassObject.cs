using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;


public class GlassObject : AttackObject
{
    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks = 0, int attackTarget = 0)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Attack()
    {
        base.Attack();
    }
}
