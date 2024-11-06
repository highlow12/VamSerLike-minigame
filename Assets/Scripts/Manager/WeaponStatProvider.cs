using UnityEngine;
using System.Collections.Generic;

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
    private void Start()
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
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 1 },
                {Weapon.WeaponRare.Rare, 1 },
                {Weapon.WeaponRare.Epic, 1 },
                {Weapon.WeaponRare.Legendary, 1 }
            },
            attackTarget = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 2 },
                {Weapon.WeaponRare.Rare, 3 },
                {Weapon.WeaponRare.Epic, 4 },
                {Weapon.WeaponRare.Legendary, 5 }
            },
            projectileCount = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 2 },
                {Weapon.WeaponRare.Rare, 3 },
                {Weapon.WeaponRare.Epic, 4 },
                {Weapon.WeaponRare.Legendary, 5 }
            },
            projectileSpeed = new Dictionary<Weapon.WeaponRare, int>
            {
                {Weapon.WeaponRare.Common, 1 },
                {Weapon.WeaponRare.Uncommon, 2 },
                {Weapon.WeaponRare.Rare, 3 },
                {Weapon.WeaponRare.Epic, 4 },
                {Weapon.WeaponRare.Legendary, 5 }
            }
        };
        weaponsStat.Add(Weapon.WeaponType.Shotgun, shotgunStat);
    }


    void Update()
    {

    }
}
