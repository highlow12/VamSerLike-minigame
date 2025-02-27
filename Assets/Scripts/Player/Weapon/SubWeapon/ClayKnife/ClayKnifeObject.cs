using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class ClayKnifeObject : AttackObject
{
    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks, int attackTarget)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
        colliderObject.transform.localScale = new Vector3(attackRange, attackRange, 1);
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public override void SubWeaponInit()
    {
        base.SubWeaponInit();
    }

    void OnEnable()
    {
        base.Start();
    }

    private void FixedUpdate()
    {
        Attack();
    }

    public override void Attack()
    {
        base.Attack();
        if (hitCount > 0)
        {
            Despawn();
        }
    }

}
