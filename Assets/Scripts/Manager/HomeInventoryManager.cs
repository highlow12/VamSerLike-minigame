using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class HomeInventoryManager : MonoBehaviour
{
    public static HomeInventoryManager Instance { get; private set; }

    public WeaponDataLoader weaponDataLoader;

    private Dictionary<string, InventoryItem> inventoryDict = new Dictionary<string, InventoryItem>();
    private List<EquipmentItem> allWeaponsDataList = new List<EquipmentItem>();//가능한 모든 무기들의 리스트. json에서 불러옴

    const string INVENTORY_KEY = "PlayerInventory";

    void Awake()
    {
        // DontDestroyOnLoad 없이 싱글톤 구현
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        allWeaponsDataList = weaponDataLoader.GetWeapons();
    }

    void Start()
    {
        LoadInventory();
        AddItem("망치", 1);
        AddItem("십자가", 1);
        PrintInventory();
    }

    // 저장
    public void SaveInventory()
    {
        var list = new List<InventoryItem>(inventoryDict.Values);
        string json = JsonUtility.ToJson(new InventoryListWrapper { items = list });
        PlayerPrefs.SetString(INVENTORY_KEY, json);
        PlayerPrefs.Save();
    }

    // 불러오기
    public void LoadInventory()
    {
        inventoryDict.Clear();
        if (PlayerPrefs.HasKey(INVENTORY_KEY))
        {
            string json = PlayerPrefs.GetString(INVENTORY_KEY);
            var list = JsonUtility.FromJson<InventoryListWrapper>(json).items;
            foreach (var item in list)
                inventoryDict[item.itemName] = item;
        }
    }

    // 로그 출력
    public void PrintInventory()
    {
        Debug.Log("Current Inventory:");
        foreach (var kvp in inventoryDict)
        {
            Debug.Log($"Item: {kvp.Key}, Count: {kvp.Value.count}");
        }
    }

    // 아이템 추가 (중복 처리)
    public void AddItem(string itemName, int amount = 1)
    {
        if (inventoryDict.TryGetValue(itemName, out var item))
        {
            item.count += amount;
        }
        else
        {
            inventoryDict[itemName] = new InventoryItem { itemName = itemName, count = amount };
        }
        SaveInventory();
    }

    // 아이템 제거 (중복 처리)
    public bool RemoveItem(string itemName, int amount = 1)
    {
        if (inventoryDict.TryGetValue(itemName, out var item) && item.count >= amount)
        {
            item.count -= amount;
            if (item.count <= 0)
                inventoryDict.Remove(itemName);
            SaveInventory();
            return true;
        }
        return false;
    }

    //아이템 전체 목록 반환
    public List<EquipmentItem> GetAllItems()
    {
        List<EquipmentItem> itemList = new List<EquipmentItem>();
        foreach (var kvp in inventoryDict)
        {
            var weapon = allWeaponsDataList.FirstOrDefault(w => w.itemName == kvp.Key);
            if (weapon != null)
            {   
                for (int i = 0; i < kvp.Value.count; i++)//중복된 아이템 수 만큼 추가
                {
                    itemList.Add(weapon);
                }
            }
        }
        Debug.Log($"GetAllItems: {itemList.Count} items found.");
        return itemList;
    }

    [System.Serializable]
    private class InventoryListWrapper
    {
        public List<InventoryItem> items = new List<InventoryItem>();
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public int count;
    }
}
