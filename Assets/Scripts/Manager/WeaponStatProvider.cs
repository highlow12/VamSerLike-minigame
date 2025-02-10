using UnityEngine;
using System.Collections.Generic;
using BackEnd;

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
    private void GetCurrentWeaponStatChart()
    {
        LitJson.JsonData chartData = BackendDataManager.Instance.GetChartData("WeaponStats");
        weaponStats.Clear();

        // Set weapon stats

        for (int i = 0; i < chartData.Count; i++)
        {
            WeaponStat weaponStat = new()
            {
                displayName = chartData[i]["displayName"]["S"].ToString(),
                weaponType = (Weapon.MainWeapon.WeaponType)int.Parse(chartData[i]["weaponType"]["S"].ToString()),
                displayWeaponRare = chartData[i]["displayWeaponRare"]["S"].ToString(),
                weaponRare = (Weapon.WeaponRare)int.Parse(chartData[i]["weaponRare"]["S"].ToString()),
                attackDamage = float.Parse(chartData[i]["attackDamage"]["S"].ToString()),
                attackSpeed = float.Parse(chartData[i]["attackSpeed"]["S"].ToString()),
                displayWeaponAttackRange = chartData[i]["displayWeaponAttackRange"]["S"].ToString(),
                weaponAttackRange = (Weapon.WeaponAttackRange)int.Parse(chartData[i]["weaponAttackRange"]["S"].ToString()),
                attackRange = float.Parse(chartData[i]["attackRange"]["S"].ToString()),
                displayWeaponAttackTarget = chartData[i]["displayWeaponAttackTarget"]["S"].ToString(),
                weaponAttackTarget = (Weapon.WeaponAttackTarget)int.Parse(chartData[i]["weaponAttackTarget"]["S"].ToString()),
                attackTarget = int.Parse(chartData[i]["attackTarget"]["S"].ToString()),
                projectileCount = int.Parse(chartData[i]["projectileCount"]["S"].ToString()),
                projectileSpeed = float.Parse(chartData[i]["projectileSpeed"]["S"].ToString())
            };
            weaponStats.Add(weaponStat);
        }

    }

    // Get sub weapon stat chart
    private void GetCurrentSubWeaponStatChart()
    {
        LitJson.JsonData chartData = BackendDataManager.Instance.GetChartData("SubWeaponStats");
        subWeaponStats.Clear();

        // Set sub weapon stats

        for (int i = 0; i < chartData.Count; i++)
        {
            SubWeaponStat subWeaponStat = new()
            {
                displayName = chartData[i]["displayName"]["S"].ToString(),
                weaponType = (Weapon.SubWeapon.WeaponType)int.Parse(chartData[i]["weaponType"]["S"].ToString()),
                weaponGrade = int.Parse(chartData[i]["weaponGrade"]["S"].ToString()),
                maxWeaponGrade = int.Parse(chartData[i]["maxWeaponGrade"]["S"].ToString()),
                displayWeaponAttackDirectionType = chartData[i]["displayWeaponAttackDirectionType"]["S"].ToString(),
                weaponAttackDirectionType = (Weapon.WeaponAttackDirectionType)int.Parse(chartData[i]["weaponAttackDirectionType"]["S"].ToString()),
                attackDamage = float.Parse(chartData[i]["attackDamage"]["S"].ToString()),
                attackRange = float.Parse(chartData[i]["attackRange"]["S"].ToString()),
                attackSpeed = float.Parse(chartData[i]["attackSpeed"]["S"].ToString()),
                attackIntervalInSeconds = float.Parse(chartData[i]["attackIntervalInSeconds"]["S"].ToString()),
                attackTarget = int.Parse(chartData[i]["attackTarget"]["S"].ToString()),
                projectileCount = int.Parse(chartData[i]["projectileCount"]["S"].ToString()),
                projectileSpeed = float.Parse(chartData[i]["projectileSpeed"]["S"].ToString())
            };
            subWeaponStats.Add(subWeaponStat);
        }
    }

    // Set weapon stats
    public override void Awake()
    {
        if (Backend.IsInitialized)
        {
            GetCurrentWeaponStatChart();
            GetCurrentSubWeaponStatChart();
        }
        base.Awake();
    }


    void Update()
    {

    }
}
