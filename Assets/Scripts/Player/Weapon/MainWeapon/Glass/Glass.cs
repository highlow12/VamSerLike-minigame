using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Glass : Weapon.MainWeapon
{
    protected override void Awake()
    {
        weaponType = WeaponType.Glass;
        weaponAttackDirectionType = Weapon.WeaponAttackDirectionType.Nearest;
        weaponPositionXOffset = 0f;
        base.Awake();
    }

    public override void InitStat()
    {
        base.InitStat();
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        weaponScript.Init(attackDamage, attackRange, 0, attackTarget);
        attackObjectAnimator.SetFloat("AttackSpeed", attackSpeed);
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
