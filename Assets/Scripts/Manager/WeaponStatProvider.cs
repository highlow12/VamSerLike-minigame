using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class WeaponStatProvider : Singleton<WeaponStatProvider>
{
    public struct WeaponStat
    {
        public Dictionary<Weapon.WeaponRare, int> attackDamage;
        public Dictionary<Weapon.WeaponRare, float> attackSpeed;
        public Dictionary<Weapon.WeaponRare, float> attackRange;
        public Dictionary<Weapon.WeaponRare, int> attackTarget;
        public Dictionary<Weapon.WeaponRare, int> projectileCount;
        public Dictionary<Weapon.WeaponRare, float> projectileSpeed;
    }
    public Dictionary<Weapon.WeaponType, WeaponStat> weaponsStat = new();


    // Set weapon stats
    private new void Awake()
    {
        WeaponStat shotgunStat = new()
        {
            attackDamage = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 10 },
                {Weapon.WeaponRare.Uncommon, 15 },
                {Weapon.WeaponRare.Rare, 20 },
                {Weapon.WeaponRare.Epic, 25 },
                {Weapon.WeaponRare.Legendary, 30 }
            },
            attackSpeed = new Dictionary<Weapon.WeaponRare, float>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 2 },
                {Weapon.WeaponRare.Rare, 3 },
                {Weapon.WeaponRare.Epic, 4 },
                {Weapon.WeaponRare.Legendary, 5 }
            },
            attackRange = new Dictionary<Weapon.WeaponRare, float>
            {
                {Weapon.WeaponRare.Common, 10 },
                {Weapon.WeaponRare.Uncommon, 12 },
                {Weapon.WeaponRare.Rare, 14 },
                {Weapon.WeaponRare.Epic, 16 },
                {Weapon.WeaponRare.Legendary, 18 }
            },
            projectileCount = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 7 },
                {Weapon.WeaponRare.Uncommon, 8 },
                {Weapon.WeaponRare.Rare, 9 },
                {Weapon.WeaponRare.Epic, 10 },
                {Weapon.WeaponRare.Legendary, 12 }
            },
            projectileSpeed = new Dictionary<Weapon.WeaponRare, float>
            {
                {Weapon.WeaponRare.Common, 3 },
                {Weapon.WeaponRare.Uncommon, 4 },
                {Weapon.WeaponRare.Rare, 5 },
                {Weapon.WeaponRare.Epic, 6 },
                {Weapon.WeaponRare.Legendary, 7 }
            }
        };
        WeaponStat axeStat = new()
        {
            attackDamage = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 100 },
                {Weapon.WeaponRare.Uncommon, 150 },
                {Weapon.WeaponRare.Rare, 200 },
                {Weapon.WeaponRare.Epic, 250 },
                {Weapon.WeaponRare.Legendary, 300 }
            },
            attackSpeed = new Dictionary<Weapon.WeaponRare, float>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 2 },
                {Weapon.WeaponRare.Rare, 3 },
                {Weapon.WeaponRare.Epic, 4 },
                {Weapon.WeaponRare.Legendary, 5 }
            },
            attackRange = new Dictionary<Weapon.WeaponRare, float>
            {
                {Weapon.WeaponRare.Common, 0.5f },
                {Weapon.WeaponRare.Uncommon, 1.0f },
                {Weapon.WeaponRare.Rare, 1.5f },
                {Weapon.WeaponRare.Epic, 2.0f },
                {Weapon.WeaponRare.Legendary, 2.5f }
            },
            attackTarget = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 1 },
                {Weapon.WeaponRare.Rare, 1 },
                {Weapon.WeaponRare.Epic, 1 },
                {Weapon.WeaponRare.Legendary, 1 }
            }
        };
        WeaponStat holyWaterStat = new()
        {
            attackDamage = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 10 },
                {Weapon.WeaponRare.Uncommon, 15 },
                {Weapon.WeaponRare.Rare, 20 },
                {Weapon.WeaponRare.Epic, 25 },
                {Weapon.WeaponRare.Legendary, 30 }
            },
            attackSpeed = new Dictionary<Weapon.WeaponRare, float>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 1.2f },
                {Weapon.WeaponRare.Rare, 1.5f },
                {Weapon.WeaponRare.Epic, 1.8f },
                {Weapon.WeaponRare.Legendary, 2.1f }
            },
            attackRange = new Dictionary<Weapon.WeaponRare, float>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 1.5f },
                {Weapon.WeaponRare.Rare, 2 },
                {Weapon.WeaponRare.Epic, 2.5f },
                {Weapon.WeaponRare.Legendary, 3f }
            },
            projectileCount = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 1 },
                {Weapon.WeaponRare.Rare, 1 },
                {Weapon.WeaponRare.Epic, 1 },
                {Weapon.WeaponRare.Legendary, 1 }
            },
            projectileSpeed = new Dictionary<Weapon.WeaponRare, float>
            {
                {Weapon.WeaponRare.Common, 5 },
                {Weapon.WeaponRare.Uncommon, 6 },
                {Weapon.WeaponRare.Rare, 7 },
                {Weapon.WeaponRare.Epic, 8 },
                {Weapon.WeaponRare.Legendary, 9 }
            }
        };
        weaponsStat.Add(Weapon.WeaponType.Shotgun, shotgunStat);
        weaponsStat.Add(Weapon.WeaponType.Axe, axeStat);
        weaponsStat.Add(Weapon.WeaponType.HolyWater, holyWaterStat);
    }


    void Update()
    {

    }
}
