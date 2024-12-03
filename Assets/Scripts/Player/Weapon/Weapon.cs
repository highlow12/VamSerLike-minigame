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
        Machete,
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
    [Serializable]
    public struct MonsterHit
    {
        public Monster monster;
        public long hitTime;
    }

    public GameObject attackObject;
    public string displayName;
    public WeaponType weaponType;
    protected WeaponStatProvider.WeaponStat weaponStat;
    public string displayWeaponAttackRange;
    protected WeaponAttackRange weaponAttackRange;
    public string displayWeaponAttackTarget;
    protected WeaponAttackTarget weaponAttackTarget;
    public WeaponAttackDirectionType weaponAttackDirectionType;
    public string displayWeaponRare;
    private WeaponRare _weaponRare;
    public WeaponRare weaponRare
    {
        get
        {
            return _weaponRare;
        }
        set
        {
            _weaponRare = value;
            InitStat();
        }
    }
    protected Animator attackObjectAnimator;
    protected float attackDamage;
    protected float attackSpeed;
    protected float attackRange;
    protected float attackForwardDistance;
    protected int attackTarget;
    protected int projectileCount;
    protected float projectileSpeed;
    public bool isAttackCooldown;

    public virtual void InitStat()
    {
        weaponStat = WeaponStatProvider.Instance.weaponStats.Find(x => x.weaponType == weaponType && x.weaponRare == weaponRare);
        displayWeaponAttackRange = weaponStat.displayWeaponAttackRange;
        displayWeaponAttackTarget = weaponStat.displayWeaponAttackTarget;
        displayWeaponRare = weaponStat.displayWeaponRare;
        displayName = weaponStat.displayName;
        weaponAttackRange = weaponStat.weaponAttackRange;
        weaponAttackTarget = weaponStat.weaponAttackTarget;
        attackDamage = weaponStat.attackDamage;
        attackSpeed = weaponStat.attackSpeed;
        attackRange = weaponStat.attackRange;
        attackForwardDistance = Mathf.Sqrt(attackRange);
        attackTarget = weaponStat.attackTarget;
        projectileCount = weaponStat.projectileCount;
        projectileSpeed = weaponStat.projectileSpeed;
    }


    public virtual IEnumerator Attack(Vector2 attackDirection)
    {
        Debug.Log("Attack");
        yield return null;
    }

}
