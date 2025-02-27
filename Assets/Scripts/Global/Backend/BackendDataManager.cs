using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class BackendDataManager : Singleton<BackendDataManager>
{
    public JsonData GetChartData(string chartName)
    {
        Debug.Log($"[BackendDataManager] GetChartData: {chartName} using local storage");
        return LocalDataManager.Instance.GetLocalChart(chartName);
    }

    public JsonData GetProbabilityData(string probabilityName)
    {
        Debug.Log($"[BackendDataManager] GetProbabilityData: {probabilityName} using local storage");
        return LocalDataManager.Instance.GetLocalProbabilityData(probabilityName);
    }

    public JsonData GetUserMainWeaponData()
    {
        Debug.Log("[BackendDataManager] GetUserMainWeaponData using local storage");
        return LocalDataManager.Instance.GetLocalUserMainWeaponData();
    }

    public JsonData GetUserAssetData()
    {
        Debug.Log("[BackendDataManager] GetUserAssetData using local storage");
        return LocalDataManager.Instance.GetLocalUserGameAssetData();
    }
}
