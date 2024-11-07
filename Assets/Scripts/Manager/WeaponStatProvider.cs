using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class WeaponStatProvider : Singleton<WeaponStatProvider>
{
    public struct WeaponStat
    {
        public Dictionary<Weapon.WeaponRare, int> attackDamage;
        public Dictionary<Weapon.WeaponRare, int> attackSpeed;
        public Dictionary<Weapon.WeaponRare, int> attackRange;
        public Dictionary<Weapon.WeaponRare, int> attackTarget;
        public Dictionary<Weapon.WeaponRare, int> projectileCount;
        public Dictionary<Weapon.WeaponRare, int> projectileSpeed;
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
            attackSpeed = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 2 },
                {Weapon.WeaponRare.Rare, 3 },
                {Weapon.WeaponRare.Epic, 4 },
                {Weapon.WeaponRare.Legendary, 5 }
            },
            attackRange = new Dictionary<Weapon.WeaponRare, int>
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
            projectileSpeed = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 3 },
                {Weapon.WeaponRare.Uncommon, 4 },
                {Weapon.WeaponRare.Rare, 5 },
                {Weapon.WeaponRare.Epic, 6 },
                {Weapon.WeaponRare.Legendary, 7 }
            }
        };
        weaponsStat.Add(Weapon.WeaponType.Shotgun, shotgunStat);
    }


    void Update()
    {

    }
}
