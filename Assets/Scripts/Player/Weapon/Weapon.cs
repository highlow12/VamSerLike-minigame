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
        Chain,
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
    public WeaponRare weaponRare;
    public int attackDamage;
    public int attackSpeed;
    public int attackRange;
    public int attackTarget;
    public int projectileCount;
    public int projectileSpeed;
    public bool isAttackCooldown;

    public virtual void InitStat(WeaponRare weaponRare)
    {
        weaponStat = WeaponStatProvider.Instance.weaponsStat[weaponType];
        this.weaponRare = weaponRare;
        attackDamage = weaponStat.attackDamage[weaponRare];
        attackSpeed = weaponStat.attackSpeed[weaponRare];
        attackRange = weaponStat.attackRange[weaponRare];
        if (attackRange == (int)WeaponAttackRange.Close)
        {
            attackTarget = weaponStat.attackTarget[weaponRare];
        }
        if (attackRange >= (int)WeaponAttackRange.Medium)
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
