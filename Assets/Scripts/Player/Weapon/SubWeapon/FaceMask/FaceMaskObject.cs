using UnityEngine;
using System.Collections;

public class FaceMaskObject : AttackObject
{

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks, int attackTarget)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);

        // FaceMask는 일반적인 공격이 아니므로, colliderObject가 필요 없음
        if (colliderObject != null)
        {
            colliderObject.SetActive(false);
        }
    }

    public override void SubWeaponInit()
    {
        base.SubWeaponInit();
    }

    public override void Attack()
    {
    }

}
