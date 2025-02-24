using UnityEngine;
using System.IO;
using LitJson;
using System;

public class LocalDataManager : Singleton<LocalDataManager>
{
    private string localDataPath => Application.streamingAssetsPath + "/LocalData/";
    private string chartDataPath => localDataPath + "Chart/";
    private string probabilityDataPath => localDataPath + "Probability/";
    private string userDataPath => localDataPath + "User/";

    public JsonData GetLocalChart(string chartName)
    {
        string filePath = Path.Combine(chartDataPath, $"{chartName}.json");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Local chart file not found: {filePath}");
            return null;
        }
        string json = File.ReadAllText(filePath);
        return JsonMapper.ToObject(json);
    }

    public JsonData GetLocalProbabilityData(string probabilityName)
    {
        string filePath = Path.Combine(probabilityDataPath, $"{probabilityName}.json");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Local probability file not found: {filePath}");
            return null;
        }
        string json = File.ReadAllText(filePath);
        return JsonMapper.ToObject(json);
    }

    public JsonData GetLocalUserMainWeaponData()
    {
        string filePath = Path.Combine(userDataPath, "MainWeaponData.json");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Local user main weapon data file not found: {filePath}");
            Debug.Log("Creating new user main weapon data file");
            if (!CreateLocalUserMainWeaponData())
            {
                Debug.LogError("Failed to create new user main weapon data file");
                return null;
            }
            else
            {
                return GetLocalUserMainWeaponData();
            }
        }
        string json = File.ReadAllText(filePath);
        JsonData data = JsonMapper.ToObject(json);
        // validate that has all of main weapons data
        foreach (int wt in Enum.GetValues(typeof(Weapon.MainWeapon.WeaponType)))
        {
            bool hasWeapon = false;
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i]["weaponType"].ToString() == wt.ToString())
                {
                    hasWeapon = true;
                    break;
                }
            }
            if (!hasWeapon)
            {
                Debug.LogError($"Local user main weapon data is missing weapon type: {wt}");
                Debug.Log($"Add main weapon: {Enum.GetName(typeof(Weapon.MainWeapon.WeaponType), wt)}");
                JsonData newWeaponData = new JsonData();
                newWeaponData.SetJsonType(JsonType.Object);
                newWeaponData["weaponType"] = wt;
                newWeaponData["weaponRare"] = (int)Weapon.WeaponRare.Common;
                data.Add(newWeaponData);
                if (!UpdateLocalUserMainWeaponData(data))
                {
                    Debug.LogError("Failed to create new user main weapon data file");
                    return null;
                }
                else
                {
                    return GetLocalUserMainWeaponData();
                }
            }
        }
        return data;
    }

    // Chart와 Probability 데이터는 게임 업데이트 배포 시에만 수정되어야 하므로, 클라이언트에서는 수정 함수가 제공되지 않습니다.
    public bool UpdateLocalUserMainWeaponData(JsonData updatedData)
    {
        string filePath = Path.Combine(userDataPath, "MainWeaponData.json");
        try
        {
            StringWriter stringWriter = new StringWriter();
            JsonWriter jsonWriter = new JsonWriter(stringWriter);
            jsonWriter.PrettyPrint = true;
            JsonMapper.ToJson(updatedData, jsonWriter);
            string json = stringWriter.ToString();

            File.WriteAllText(filePath, json);
            Debug.Log($"Local user main weapon data updated: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update local user main weapon data: {ex.Message}");
            return false;
        }
    }

    public bool CreateLocalUserMainWeaponData()
    {
        JsonData data = new JsonData();
        data.SetJsonType(JsonType.Array);

        // Weapon.MainWeapon.WeaponType의 각 값을 WeaponRare.Common과 매칭
        foreach (int wt in Enum.GetValues(typeof(Weapon.MainWeapon.WeaponType)))
        {
            JsonData weaponData = new JsonData();
            weaponData.SetJsonType(JsonType.Object);
            weaponData["weaponType"] = wt;
            weaponData["weaponRare"] = (int)Weapon.WeaponRare.Common;
            data.Add(weaponData);
        }

        string filePath = Path.Combine(userDataPath, "MainWeaponData.json");
        try
        {
            StringWriter stringWriter = new StringWriter();
            JsonWriter jsonWriter = new JsonWriter(stringWriter);
            jsonWriter.PrettyPrint = true;
            JsonMapper.ToJson(data, jsonWriter);
            string json = stringWriter.ToString();

            File.WriteAllText(filePath, json);
            Debug.Log($"Local user main weapon data created: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create local user main weapon data: {ex.Message}");
            return false;
        }
    }

    public JsonData GetLocalUserGameAssetData()
    {
        string filePath = Path.Combine(userDataPath, "GameAssetData.json");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Local user game asset data file not found: {filePath}");
            Debug.Log("Creating new user game asset data file");
            if (!CreateLocalUserGameAssetData())
            {
                Debug.LogError("Failed to create new user game asset data file");
                return null;
            }
            else
            {
                return GetLocalUserGameAssetData();
            }
        }
        string json = File.ReadAllText(filePath);
        return JsonMapper.ToObject(json);
    }

    public bool UpdateLocalUserGameAssetData(JsonData updatedData)
    {
        string filePath = Path.Combine(userDataPath, "GameAssetData.json");
        try
        {
            StringWriter stringWriter = new StringWriter();
            JsonWriter jsonWriter = new JsonWriter(stringWriter);
            jsonWriter.PrettyPrint = true;
            JsonMapper.ToJson(updatedData, jsonWriter);
            string json = stringWriter.ToString();

            File.WriteAllText(filePath, json);
            Debug.Log($"Local user game asset data updated: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update local user game asset data: {ex.Message}");
            return false;
        }
    }

    public bool CreateLocalUserGameAssetData()
    {
        JsonData data = new JsonData();
        data.SetJsonType(JsonType.Object);
        data["skins"] = new JsonData();
        data["skins"].SetJsonType(JsonType.Array);

        string filePath = Path.Combine(userDataPath, "GameAssetData.json");
        try
        {
            StringWriter stringWriter = new StringWriter();
            JsonWriter jsonWriter = new JsonWriter(stringWriter);
            jsonWriter.PrettyPrint = true;
            JsonMapper.ToJson(data, jsonWriter);
            string json = stringWriter.ToString();

            File.WriteAllText(filePath, json);
            Debug.Log($"Local user game asset data created: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create local user game asset data: {ex.Message}");
            return false;
        }
    }
}
