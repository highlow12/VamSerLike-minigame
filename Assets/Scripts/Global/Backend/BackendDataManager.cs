using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class BackendDataManager : Singleton<BackendDataManager>
{
    protected override bool useDontDestroyOnLoad { get { return true; } }

    public JsonData GetChartData(string chartName)
    {
        Debug.Log($"[BackendDataManager] GetChartData: {chartName} using local storage");
        return LocalDataManager.Instance.GetLocalChart(chartName);
    }

    public JsonData GetProbabilityData(string probabilityName)
    {
        Debug.Log($"[BackendDataManager] GetProbabilityData: {probabilityName} using local storage");
        JsonData data = LocalDataManager.Instance.GetLocalProbabilityData(probabilityName);
        if (data == null)
        {
            Debug.LogError($"[BackendDataManager] Failed to get probability data: {probabilityName}");
        }
        return data;
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
