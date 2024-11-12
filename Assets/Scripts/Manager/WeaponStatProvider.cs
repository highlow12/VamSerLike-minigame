using UnityEngine;
using System.Collections.Generic;
using BackEnd;

[DefaultExecutionOrder(-100)]
public class WeaponStatProvider : Singleton<WeaponStatProvider>
{
    public struct WeaponStat
    {
        public string displayName;
        public Weapon.WeaponType weaponType;
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
    public List<WeaponStat> weaponStats = new();


    private void GetCurrentWeaponStatChart()
    {
        // Get latest weapon stat chart file id

        Where where = new();
        where.Equal("columnType", "currentWeaponStatChartFileId");
        var chartFileIdBro = Backend.GameData.Get("Global", where);
        if (!chartFileIdBro.IsSuccess())
        {
            Debug.LogError("Get currentWeaponStatChartFileId failed");
            return;
        }
        if (chartFileIdBro.GetReturnValuetoJSON()["rows"].Count <= 0)
        {
            Debug.LogError("No currentWeaponStatChartFileId data");
            return;
        }
        string currentWeaponStatChartFileId = chartFileIdBro.Rows()[0]["data"]["N"].ToString();

        // Get weapon stat chart

        var chartBro = Backend.Chart.GetChartContents(currentWeaponStatChartFileId);
        if (!chartBro.IsSuccess())
        {
            Debug.LogError("Get weapon stat chart failed");
            Debug.LogError(chartBro);
            return;
        }
        Debug.Log("Get weapon stat chart success");
        LitJson.JsonData chartData = chartBro.GetReturnValuetoJSON()["rows"];
        weaponStats.Clear();

        // Set weapon stats

        for (int i = 0; i < chartData.Count; i++)
        {
            WeaponStat weaponStat = new()
            {
                displayName = chartData[i]["displayName"]["S"].ToString(),
                weaponType = (Weapon.WeaponType)int.Parse(chartData[i]["weaponType"]["S"].ToString()),
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

    // Set weapon stats
    private new void Awake()
    {
        GetCurrentWeaponStatChart();
    }


    void Update()
    {

    }
}
