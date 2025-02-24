using UnityEngine;
using System.Collections.Generic;

public class WeaponStatProvider : Singleton<WeaponStatProvider>
{
    public struct WeaponStat
    {
        public string displayName;
        public Weapon.MainWeapon.WeaponType weaponType;
        public string displayWeaponRare;
        public Weapon.WeaponRare weaponRare;
        public float attackDamage;
        public float attackSpeed;
        public string displayWeaponAttackRange;
        public Weapon.WeaponAttackRange weaponAttackRange;
        public float attackRange;
        public string displayWeaponAttackTarget;
        public Weapon.WeaponAttackTarget weaponAttackTarget;
        public int attackTarget;
        public int projectileCount;
        public float projectileSpeed;
    }
    public struct SubWeaponStat
    {
        public string displayName;
        public Weapon.SubWeapon.WeaponType weaponType;
        public int weaponGrade;
        public int maxWeaponGrade;
        public string displayWeaponAttackDirectionType;
        public Weapon.WeaponAttackDirectionType weaponAttackDirectionType;
        public float attackDamage;
        public float attackRange;
        public float attackSpeed;
        public float attackIntervalInSeconds;
        public int attackTarget;
        public int projectileCount;
        public float projectileSpeed;
    }
    public List<WeaponStat> weaponStats = new();
    public List<SubWeaponStat> subWeaponStats = new();

    // Get main weapon stat chart
    public void GetCurrentWeaponStatChart()
    {
        LitJson.JsonData chartData = BackendDataManager.Instance.GetChartData("WeaponStats");
        weaponStats.Clear();

        // Set weapon stats
        for (int i = 0; i < chartData.Count; i++)
        {
            WeaponStat weaponStat = new()
            {
                displayName = chartData[i]["displayName"].ToString(),
                weaponType = (Weapon.MainWeapon.WeaponType)int.Parse(chartData[i]["weaponType"].ToString()),
                displayWeaponRare = chartData[i]["displayWeaponRare"].ToString(),
                weaponRare = (Weapon.WeaponRare)int.Parse(chartData[i]["weaponRare"].ToString()),
                attackDamage = float.Parse(chartData[i]["attackDamage"].ToString()),
                attackSpeed = float.Parse(chartData[i]["attackSpeed"].ToString()),
                displayWeaponAttackRange = chartData[i]["displayWeaponAttackRange"].ToString(),
                weaponAttackRange = (Weapon.WeaponAttackRange)int.Parse(chartData[i]["weaponAttackRange"].ToString()),
                attackRange = float.Parse(chartData[i]["attackRange"].ToString()),
                displayWeaponAttackTarget = chartData[i]["displayWeaponAttackTarget"].ToString(),
                weaponAttackTarget = (Weapon.WeaponAttackTarget)int.Parse(chartData[i]["weaponAttackTarget"].ToString()),
                attackTarget = int.Parse(chartData[i]["attackTarget"].ToString()),
                projectileCount = int.Parse(chartData[i]["projectileCount"].ToString()),
                projectileSpeed = float.Parse(chartData[i]["projectileSpeed"].ToString())
            };
            weaponStats.Add(weaponStat);
        }
    }

    // Get sub weapon stat chart
    public void GetCurrentSubWeaponStatChart()
    {
        LitJson.JsonData chartData = BackendDataManager.Instance.GetChartData("SubWeaponStats");
        subWeaponStats.Clear();

        // Set sub weapon stats
        for (int i = 0; i < chartData.Count; i++)
        {
            SubWeaponStat subWeaponStat = new()
            {
                displayName = chartData[i]["displayName"].ToString(),
                weaponType = (Weapon.SubWeapon.WeaponType)int.Parse(chartData[i]["weaponType"].ToString()),
                weaponGrade = int.Parse(chartData[i]["weaponGrade"].ToString()),
                maxWeaponGrade = int.Parse(chartData[i]["maxWeaponGrade"].ToString()),
                displayWeaponAttackDirectionType = chartData[i]["displayWeaponAttackDirectionType"].ToString(),
                weaponAttackDirectionType = (Weapon.WeaponAttackDirectionType)int.Parse(chartData[i]["weaponAttackDirectionType"].ToString()),
                attackDamage = float.Parse(chartData[i]["attackDamage"].ToString()),
                attackRange = float.Parse(chartData[i]["attackRange"].ToString()),
                attackSpeed = float.Parse(chartData[i]["attackSpeed"].ToString()),
                attackIntervalInSeconds = float.Parse(chartData[i]["attackIntervalInSeconds"].ToString()),
                attackTarget = int.Parse(chartData[i]["attackTarget"].ToString()),
                projectileCount = int.Parse(chartData[i]["projectileCount"].ToString()),
                projectileSpeed = float.Parse(chartData[i]["projectileSpeed"].ToString())
            };
            subWeaponStats.Add(subWeaponStat);
        }
    }

    // Set weapon stats
    public override void Awake()
    {
        GetCurrentWeaponStatChart();
        GetCurrentSubWeaponStatChart();
        base.Awake();
    }


    void Update()
    {

    }
}
