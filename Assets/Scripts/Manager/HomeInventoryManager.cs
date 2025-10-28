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

    private int curr_inventorySize = 0;//획득 순서 추적용으로 개발하였으나 실사용은 안함.

    private List<InventoryItem> orderedInventoryItems = new List<InventoryItem>();



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


        LoadInventory();
        PrintInventory();

    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Adding test items to inventory");
            AddItem("망치", ItemRarity.C, 1, 1);
            AddItem("도끼", ItemRarity.D, 2, 1);
            AddItem("십자가", ItemRarity.B, 4, 1);
        }
#endif
    }

    void Start()
    {
    }

    // 저장
    public void SaveInventory()
    {
        //var list = new List<InventoryItem>(inventoryDict.Values);
        var list = orderedInventoryItems;
        string json = JsonUtility.ToJson(new InventoryListWrapper { items = list });
        PlayerPrefs.SetString(INVENTORY_KEY, json);
        PlayerPrefs.Save();
    }

    // 불러오기
    public void LoadInventory()
    {
        curr_inventorySize = 0;
        orderedInventoryItems.Clear();
        inventoryDict.Clear();
        if (PlayerPrefs.HasKey(INVENTORY_KEY))
        {
            string json = PlayerPrefs.GetString(INVENTORY_KEY);
            var list = JsonUtility.FromJson<InventoryListWrapper>(json).items;
            foreach (var item in list)
            {
                orderedInventoryItems.Add(item);
                //inventoryDict은 참조만 할 뿐 데이터는 orderedInventoryItems가 실제로 보유
                inventoryDict[GetItemCodeByInventoryItem(item)] = orderedInventoryItems[orderedInventoryItems.Count - 1];
                curr_inventorySize += item.amount;
            }
        }
    }

    // 로그 출력
    public void PrintInventory()
    {
        Debug.Log("=====Current Inventory=====");
        foreach (var kvp in inventoryDict)
        {
            Debug.Log($"ItemKey: {kvp.Key}, ItemName: {kvp.Value.itemName}, Rarity: {kvp.Value.rarity}, Enhancement: {kvp.Value.enhancementValue}, Count: {kvp.Value.amount}");
        }
        Debug.Log("==========================");
    }

    // 아이템 추가 (중복 처리)
    public void AddItem(string itemName, ItemRarity rarity, int enhancementValue, int amount = 1)
    {
        //itemName이 정확한 이름인지 확인 필요. 일단 임시로 정확하다고 가정
        curr_inventorySize += amount;
        InventoryItem item = new InventoryItem
        {
            itemName = itemName,
            rarity = rarity,
            enhancementValue = enhancementValue,
            amount = amount
        };
        string itemKey = GetItemCodeByInventoryItem(item);

        if (inventoryDict.TryGetValue(itemKey, out var existingItem))
        {
            existingItem.amount += amount;
        }
        else
        {

            inventoryDict[itemKey]
             = new InventoryItem(item);
        }
        for(int i=0; i<amount; i++)
        {
            orderedInventoryItems.Add(new InventoryItem(item));
        }
        SaveInventory();
    }

    // 아이템 제거 (중복 처리)
    public bool RemoveItem(string itemName, ItemRarity rarity, int enhancementValue, int amount = 1)
    {
        if (inventoryDict.TryGetValue(GetItemCode(new InventoryItem(itemName, rarity, enhancementValue)), out var item) && item.amount >= amount)
        {
            item.amount -= amount;
            if (item.amount <= 0)
                inventoryDict.Remove(GetItemCode(item));
            curr_inventorySize -= amount;
            SaveInventory();
            return true;
        }
        return false;
    }

    public List<EquipmentItem> GetOrderedItems()//생각보다 자주 호출하는중.. 이 함수를 호출하는 코드를 변경하자. 캐시 개념 활용
    {
        List<EquipmentItem> itemList = new List<EquipmentItem>();
        foreach (var invItem in orderedInventoryItems)
        {
            var equipmentItem = CreateEquipmentItem(invItem);
            if (equipmentItem != null)
            {
                itemList.Add(equipmentItem);
            }
        }
        Debug.Log($"GetOrderedItems: {itemList.Count} items found in ordered list.");
        return itemList;
    }


    //아이템 전체 목록 반환
    public List<EquipmentItem> GetAllItems()
    {
        List<EquipmentItem> inventoryItemList = new List<EquipmentItem>();
        Debug.Log("GetAllItems: " + inventoryDict.Count + " items types in inventoryDict.");
        foreach (var kvp in inventoryDict)
        {
            //Debug.Log("GetAllItem: test");
            Debug.Log("GetAllItems: weaponData found->" + allWeaponsDataList.FirstOrDefault(w =>
            (w.itemName == kvp.Value.itemName) && (w.rarity == kvp.Value.rarity)));

            var weapon = CreateEquipmentItem(kvp.Value);
            if (weapon != null)
            {
                Debug.Log($"GetAllItems: created weapon {weapon.itemName} with rarity {weapon.rarity} and enhancement {kvp.Value.enhancementValue}, count: {kvp.Value.amount}");
                weapon.enhancementValue = kvp.Value.enhancementValue;
                //enhancementValue 수치만큼 능력치 증가 적용 필요=> 모든 Get 함수에 반영 필요
                //weapon.SetValuesWithEnhancement();//이 함수를 사용하지 않고, enhancement를 ui상으로 표시 안하면 강화는 없는거나 마찬가지. 당장은 강화를 구현 안함.
                for (int i = 0; i < kvp.Value.amount; i++)//중복된 아이템 수 만큼 추가
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

    public void SaveItemCode(string key, string itemCode)
    {
        PlayerPrefs.SetString(key, itemCode);
        PlayerPrefs.Save();
    }




    public string GetItemCode(EquipmentItem item)//핵심 함수. 코드 = key. 같은 코드의 아이템은 모든 값이 동일함.(획득 순서 제외) 
    {
        if (item.category == EquipmentCategory.Weapon)//category 구분은 사실 필요 없는듯
        {
            return $"{item.itemName}{(int)item.rarity}{(int)item.enhancementValue}";
        }
        else if (item.category == EquipmentCategory.Cloak)
        {
            return $"{item.itemName}{(int)item.rarity}{(int)item.enhancementValue}";
        }
        else
        {
            return "";
        }
    }

    private string GetItemCode(InventoryItem item)//핵심 함수. 코드 = key. 같은 코드의 아이템은 모든 값이 동일함.(획득 순서 제외) 
    {
        return $"{item.itemName}{(int)item.rarity}{(int)item.enhancementValue}";
    }
    private EquipmentItem CreateEquipmentItem(InventoryItem item)
    {
        EquipmentItem weapon = allWeaponsDataList.FirstOrDefault(w =>
            (w.itemName == item.itemName) && (w.rarity == item.rarity)).Clone();
        if (weapon != null)
        {
            weapon.enhancementValue = item.enhancementValue;
            return weapon;
        }
        return null;
    }

    private string GetItemCodeByInventoryItem(InventoryItem item)
    {
        return $"{item.itemName}{(int)item.rarity}{(int)item.enhancementValue}";
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
        public int amount;
        //public int acquiredIndex;//획득 순서 추적용

        public InventoryItem()
        {
        }

        public InventoryItem(string itemName, ItemRarity rarity, int enhancementValue)
        {
            this.itemName = itemName;
            this.rarity = rarity;
            this.enhancementValue = enhancementValue;
            this.amount = 0;
            //this.acquiredIndex = -1;
        }

        public InventoryItem(InventoryItem item)
        {
            itemName = item.itemName;
            rarity = item.rarity;
            enhancementValue = item.enhancementValue;
            amount = item.amount;
            //acquiredIndex = item.acquiredIndex;
        }
    }
}
