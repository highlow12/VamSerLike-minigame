using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class Shotgun : Weapon
{
    void Start()
    {
        WeaponStatProvider.WeaponStat shotgunStat = WeaponStatProvider.Instance.weaponsStat[WeaponType.Shotgun];
        WeaponRare weaponRare = WeaponRare.Common;
        weaponStruct = new()
        {
            weaponType = WeaponType.Shotgun,
            weaponAttackRange = WeaponAttackRange.Medium,
            weaponAttackTarget = WeaponAttackTarget.Multiple,
            weaponRare = weaponRare,
            damage = shotgunStat.attackDamage[weaponRare],
            attackSpeed = shotgunStat.attackSpeed[weaponRare],
            attackRange = shotgunStat.attackRange[weaponRare],
            attackTarget = shotgunStat.attackTarget[weaponRare],
            projectileCount = shotgunStat.projectileCount[weaponRare],
            projectileSpeed = shotgunStat.projectileSpeed[weaponRare]
        };
    }

    protected override void Attack()
    {
        Debug.Log("Shotgun Attack");
    }
}
