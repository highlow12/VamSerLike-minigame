using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Machete : Weapon.MainWeapon
{
    protected override void Awake()
    {
        weaponType = WeaponType.Machete;
        weaponAttackDirectionType = Weapon.WeaponAttackDirectionType.Nearest;
        weaponPositionXOffset = 0.3f;
        base.Awake();
    }

    public override void InitStat()
    {
        base.InitStat();
        weaponScript.Init(
            attackDamage,
            attackRange,
            0,
            attackTarget
        );

    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        attackObjectAnimator.SetFloat("AttackSpeed", attackSpeed);
        isAttackCooldown = true;
        Vector2 pos = (Vector2)transform.position + attackDirection.normalized * attackForwardDistance;
        float degree = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        weaponScript.colliderObject.transform.position = pos;
        weaponScript.colliderObject.transform.rotation = Quaternion.Euler(0, 0, degree);
        Flip(degree);
        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
        yield return null;
    }

    protected override void Flip(float degree)
    {
        base.Flip(degree);
    }

}
