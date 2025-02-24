using UnityEngine;
using System.IO;
using LitJson;
using System;
using CustomEncryption;
using System.Text;

public class LocalDataManager : Singleton<LocalDataManager>
{
    private const string m_key = "EnRBcwL791f3oEf/AH2D0D2EhbajQ0yBimSUbLHDTA8=";
    private string localDataPath => Application.streamingAssetsPath + "/LocalData/";
    private string chartDataPath => localDataPath + "Chart/";
    private string probabilityDataPath => localDataPath + "Probability/";
    private string userDataPath => localDataPath + "User/";

    private string _LoadData(string path, Func<bool> fallbackCreator = null)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"File not found: {path}");
            if (fallbackCreator != null)
            {
                Debug.Log("Creating new file using fallback creator");
                if (fallbackCreator())
                {
                    return _LoadData(path);
                }
            }
            return null;
        }
        byte[] encryptedBytes = File.ReadAllBytes(path);
        byte[] decryptedBytes = Rijndael.Decrypt(encryptedBytes, m_key);
        if (decryptedBytes == null)
        {
            Debug.LogError($"Failed to decrypt file: {path}");
            if (fallbackCreator != null)
            {
                Debug.Log("Creating new file using fallback creator");
                if (fallbackCreator())
                {
                    return _LoadData(path);
                }
            }
            return null;
        }
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    private void _SaveData(string path, string data)
    {
        byte[] bytes = Rijndael.EncryptString(data, m_key);
        File.WriteAllBytes(path, bytes);
    }

    public JsonData GetLocalChart(string chartName)
    {
        string filePath = Path.Combine(chartDataPath, $"{chartName}.json");
        string json = _LoadData(filePath);
        return JsonMapper.ToObject(json);
    }

    public JsonData GetLocalProbabilityData(string probabilityName)
    {
        string filePath = Path.Combine(probabilityDataPath, $"{probabilityName}.json");
        string json = _LoadData(filePath);
        return JsonMapper.ToObject(json);
    }

    public JsonData GetLocalUserMainWeaponData()
    {
        string filePath = Path.Combine(userDataPath, "MainWeaponData.json");
        string json = _LoadData(filePath, CreateLocalUserMainWeaponData);
        if (json == null) return null;

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
            _SaveData(filePath, json);
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
            _SaveData(filePath, json);
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
        string json = _LoadData(filePath, CreateLocalUserGameAssetData);
        return json != null ? JsonMapper.ToObject(json) : null;
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
            _SaveData(filePath, json);
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
            _SaveData(filePath, json);
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
