using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

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

    protected enum WeaponAttackRange
    {
        Close,
        Medium,
        Long
    }

    protected enum WeaponAttackTarget
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


    protected struct WeaponStruct
    {
        public WeaponType weaponType;
        public WeaponAttackRange weaponAttackRange;
        public WeaponAttackTarget weaponAttackTarget;
        public WeaponRare weaponRare;
        public int damage;
        public int attackSpeed;
        public int attackRange;
        public int attackTarget;
        public int projectileCount;
        public int projectileSpeed;
    }




    protected WeaponStruct weaponStruct;


    protected virtual void Attack()
    {
        Debug.Log("Attack");
    }



}
