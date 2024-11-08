using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public abstract class Weapon : MonoBehaviour
{
    public enum WeaponType
    {
        Hammer,
        Axe,
        Cross,
        CrossBow,
        Shotgun,
        HolyWater
    }

    public enum WeaponAttackRange
    {
        Close,
        Medium,
        Long
    }

    public enum WeaponAttackTarget
    {
        Single,
        Multiple
    }

    public enum WeaponAttackDirectionType
    {
        Nearest,
        Aim
    }

    public enum WeaponRare
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public WeaponStatProvider.WeaponStat weaponStat;
    public WeaponType weaponType;
    public WeaponAttackRange weaponAttackRange;
    public WeaponAttackTarget weaponAttackTarget;
    public WeaponAttackDirectionType weaponAttackDirectionType;
    public WeaponRare weaponRare;
    public int attackDamage;
    public float attackSpeed;
    public float attackRange;
    public int attackTarget;
    public int projectileCount;
    public float projectileSpeed;
    public bool isAttackCooldown;

    public virtual void InitStat(WeaponRare weaponRare)
    {
        weaponStat = WeaponStatProvider.Instance.weaponsStat[weaponType];
        this.weaponRare = weaponRare;
        attackDamage = weaponStat.attackDamage[weaponRare];
        attackSpeed = weaponStat.attackSpeed[weaponRare];
        attackRange = weaponStat.attackRange[weaponRare];
        if (weaponAttackRange == WeaponAttackRange.Close)
        {
            attackTarget = weaponStat.attackTarget[weaponRare];
        }
        if (weaponAttackRange >= WeaponAttackRange.Medium)
        {
            projectileCount = weaponStat.projectileCount[weaponRare];
            projectileSpeed = weaponStat.projectileSpeed[weaponRare];
        }

    }


    public virtual IEnumerator Attack(Vector2 attackDirection)
    {
        Debug.Log("Attack");
        yield return null;
    }

}
