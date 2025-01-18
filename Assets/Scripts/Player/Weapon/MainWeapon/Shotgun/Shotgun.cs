using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Shotgun : Weapon.MainWeapon
{
    [SerializeField] private float spreadDegree = 45f;

    protected override void Awake()
    {
        weaponType = WeaponType.Shotgun;
        weaponAttackDirectionType = Weapon.WeaponAttackDirectionType.Aim;
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
        isAttackCooldown = true;
        float baseAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        float spreadDegree = this.spreadDegree / projectileCount; // 퍼짐 정도를 조절
        Flip(baseAngle);
        weaponScript.Setup(baseAngle, spreadDegree, projectileCount, projectileSpeed, attackSpeed);
        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
        yield return null;

    }
    protected override void Flip(float degree)
    {
        base.Flip(degree);
    }
}
