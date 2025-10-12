using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class HomeInventoryManager : MonoBehaviour
{
    public static HomeInventoryManager Instance { get; private set; }

    public WeaponDataLoader weaponDataLoader;

    //key: itemName+rarityValue+enhancementValue, value: InventoryItem
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
        AddItem("망치", ItemRarity.C, 1, 1);
        AddItem("망치", ItemRarity.C, 2, 1);
        AddItem("십자가", ItemRarity.B, 4, 1);
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
                inventoryDict[item.itemName + item.rarity + item.enhancementValue] = item;//equipmentItem중 이름이 같더라도 다른 희귀도를 가진 아이템이 존재할 수 있음.=>이름 뒤에 희귀도 수치를 붙여서 구분하는 방법도 있음.
        }
    }

    // 로그 출력
    public void PrintInventory()
    {
        Debug.Log("Current Inventory:");
        foreach (var kvp in inventoryDict)
        {
            Debug.Log($"ItemKey: {kvp.Key}, ItemName: {kvp.Value.itemName}, Rarity: {kvp.Value.rarity}, Enhancement: {kvp.Value.enhancementValue}, Count: {kvp.Value.count}");
        }
    }

    // 아이템 추가 (중복 처리)
    public void AddItem(string itemName, ItemRarity rarity, int enhancementValue, int amount = 1)
    {
        if (inventoryDict.TryGetValue(itemName + rarity + enhancementValue, out var item))
        {
            item.count += amount;
        }
        else
        {
            inventoryDict[itemName + rarity + enhancementValue] = new InventoryItem { itemName = itemName, count = amount, enhancementValue = enhancementValue, rarity = rarity };
        }
        SaveInventory();
    }

    // 아이템 제거 (중복 처리)
    public bool RemoveItem(string itemName, ItemRarity rarity, int enhancementValue, int amount = 1)
    {
        if (inventoryDict.TryGetValue(itemName + rarity + enhancementValue, out var item) && item.count >= amount)
        {
            item.count -= amount;
            if (item.count <= 0)
                inventoryDict.Remove(itemName + rarity + enhancementValue);
            SaveInventory();
            return true;
        }
        return false;
    }

    //아이템 전체 목록 반환
    public List<EquipmentItem> GetAllItems()
    {
        List<EquipmentItem> inventoryItemList = new List<EquipmentItem>();
        Debug.Log("GetAllItems: " + inventoryDict.Count + " items in inventoryDict.");
        foreach (var kvp in inventoryDict)
        {
            Debug.Log("GetAllItem: test");
            Debug.Log("GetAllItems: weaponData found->" + allWeaponsDataList.FirstOrDefault(w =>
            (w.itemName == kvp.Value.itemName) && (w.rarity == kvp.Value.rarity)));

            var weapon = allWeaponsDataList.FirstOrDefault(w =>
            (w.itemName == kvp.Value.itemName) && (w.rarity == kvp.Value.rarity)).Clone();
            if (weapon != null)
            {
                Debug.Log($"GetAllItems: created weapon {weapon.itemName} with rarity {weapon.rarity} and enhancement {kvp.Value.enhancementValue}, count: {kvp.Value.count}");
                weapon.enhancementValue = kvp.Value.enhancementValue;
                //enhancementValue 수치만큼 능력치 증가 적용 필요=> 모든 Get 함수에 반영 필요
                weapon.SetValuesWithEnhancement();
                for (int i = 0; i < kvp.Value.count; i++)//중복된 아이템 수 만큼 추가
                {
                    inventoryItemList.Add(weapon.Clone());
                }
            }
        }
        Debug.Log($"GetAllItems: {inventoryItemList.Count} items found.");
        return inventoryItemList;
    }

    public List<EquipmentItem> GetAllWeaponsDataList()//test용 전체 무기 데이터 반환
    {
        return allWeaponsDataList;
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
        public ItemRarity rarity;
        public int enhancementValue;
        public int count;
    }
}
