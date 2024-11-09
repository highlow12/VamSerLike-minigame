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

    protected WeaponStatProvider.WeaponStat weaponStat;
    protected WeaponType weaponType;
    protected WeaponAttackRange weaponAttackRange;
    protected WeaponAttackTarget weaponAttackTarget;
    public WeaponAttackDirectionType weaponAttackDirectionType;
    private WeaponRare _weaponRare;
    public WeaponRare weaponRare
    {
        get { return _weaponRare; }
        set
        {
            _weaponRare = value;
            InitStat(value);
        }
    }
    protected int attackDamage;
    protected float attackSpeed;
    protected float attackRange;
    protected int attackTarget;
    protected int projectileCount;
    protected float projectileSpeed;
    public bool isAttackCooldown;

    public virtual void InitStat(WeaponRare weaponRare)
    {
        weaponStat = WeaponStatProvider.Instance.weaponsStat[weaponType];
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
