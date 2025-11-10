using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;



public class HomeInventoryManager : Singleton<HomeInventoryManager>
{

    /// <summary>
    /// 솔직히 말하자면, SaveManager 구현되어있는 줄 모르고 저장 시스템 따로 더 만들어서 이것만 사용중.. ㅈㅅ요;;
    /// docmost 서버 터져서 인벤토리 개발할 때 관련 코드가 있는지 확인을 못했던거라 양해 바람...
    /// 이 클래스는 플레이어의 인벤토리를 관리하며, 아이템 추가, 제거, 저장 및 로드를 담당합니다.
    /// </summary>

    public WeaponDataLoader weaponDataLoader;

    //key: itemName+rarityValue+enhancementValue, value: InventoryItem
    private Dictionary<string, InventoryItem> inventoryDict = new Dictionary<string, InventoryItem>();

    private List<EquipmentItem> allWeaponsDataList = new List<EquipmentItem>();//가능한 모든 무기들의 리스트. json에서 불러옴

    const string INVENTORY_KEY = "PlayerInventory";

    private int curr_inventorySize = 0;//획득 순서 추적용으로 개발하였으나 실사용은 안함.

    private List<InventoryItem> orderedInventoryItems = new List<InventoryItem>();


    const string EQUIPPED_WEAPON_KEY = "EquippedWeapon";
    const string EQUIPPED_CLOAK_KEY = "EquippedCloak";


    void Awake()
    {

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
            InventoryItem item1 = AddItem("망치", ItemRarity.C, 1, 1);
            AddItem("도끼", ItemRarity.D, 2, 1);
            AddItem("십자가", ItemRarity.B, 4, 1);
            AddItem("도끼", ItemRarity.S, 1, 1);
            RemoveItem(item1, 1);
        }
        else if(Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("Saving and loading inventory");
            SaveInventory();
            LoadInventory();
            PrintInventory();
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
                inventoryDict[GetItemCode(item)] = orderedInventoryItems[orderedInventoryItems.Count - 1];
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
            Debug.Log($"ItemKey: {kvp.Key}, ItemName: {kvp.Value.itemName}, Rarity: {kvp.Value.rarity}, RarityEnhance: {kvp.Value.enhanceActions.Count}, Count: {kvp.Value.amount}");
        }
        Debug.Log("==========================");
    }

    // 아이템 추가 (중복 처리)
    public InventoryItem AddItem(string itemName, ItemRarity rarity, int enhancementValue, int amount = 1)
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
        string itemKey = GetItemCode(item);

        if (inventoryDict.TryGetValue(itemKey, out var existingItem))
        {
            existingItem.amount += amount;
        }
        else
        {

            inventoryDict[itemKey]
             = new InventoryItem(item);
        }
        for (int i = 0; i < amount; i++)
        {
            orderedInventoryItems.Add(new InventoryItem(item));
        }
        //SaveInventory(); //저장 호출은 외부에서 필요할 때 호출하도록 변경
        return item;
    }

    // 아이템 제거 (중복 처리)
    public bool RemoveItem(InventoryItem item, int deleteAmount = 1)
    {
        Debug.Log($"RemoveItem: Trying to remove {deleteAmount} of {item.itemName} (Rarity: {item.rarity}, Enhancement: {item.enhancementValue})");
        if (inventoryDict.TryGetValue(GetItemCode(item), out var existingItem)
         && existingItem.amount >= deleteAmount)
        {
            existingItem.amount -= deleteAmount;
            if (existingItem.amount <= 0)
                inventoryDict.Remove(GetItemCode(item));
            Debug.Log($"RemoveItem: Removed {deleteAmount} of {item.itemName}. Remaining: {existingItem.amount}");
            curr_inventorySize -= deleteAmount;
            //SaveInventory();
            return true;
        }
        return false;
    }


    public List<EquipmentItem> GetOrderedItems()
    //생각보다 자주 호출하는중.. 이 함수를 호출하는 코드를 변경하자. 캐시 개념 활용
    //근데 최대 35번 반복이니 충분히 괜찮을지도?
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


    /*//아이템 전체 목록 반환
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
    }*/

    public List<EquipmentItem> GetAllWeaponsDataList()//test용 전체 무기 데이터 반환
    {
        return allWeaponsDataList;
    }

    public void SaveEquippedItem(EquipmentCategory equippedItem, string itemCode)
    {
        string key = equippedItem == EquipmentCategory.Weapon ? EQUIPPED_WEAPON_KEY : EQUIPPED_CLOAK_KEY;
        PlayerPrefs.SetString(key, itemCode);
        PlayerPrefs.Save();
    }

    public string LoadEquippedItemCode(EquipmentCategory equippedItem)
    {
        string key = equippedItem == EquipmentCategory.Weapon ? EQUIPPED_WEAPON_KEY : EQUIPPED_CLOAK_KEY;
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetString(key);
        }
        return "";
    }



    public string GetItemCode(EquipmentItem item)//핵심 함수. 코드 = key. 같은 코드의 아이템은 모든 값이 동일함.(획득 순서 제외) 
    {
        if (item.category == EquipmentCategory.Weapon)//category 구분은 현재는 필요 없음. 나중에 망토에 추가 기능 생기면 필요
        {
            return GetItemCode(new InventoryItem(item));
        }
        else if (item.category == EquipmentCategory.Cloak)
        {
            return GetItemCode(new InventoryItem(item));
        }
        else
        {
            return "";
        }
    }

    private string GetItemCode(InventoryItem item)//핵심 함수. 코드 = key. 같은 코드의 아이템은 모든 값이 동일함.(획득 순서 제외) 
    {
        return $"{item.itemName}{(int)item.rarity}{(int)item.enhancementValue}{item.enhanceActions}";
    }


    private EquipmentItem CreateEquipmentItem(InventoryItem item) //핵심 함수. 실제로 필요한 객체인 EquipmentItem 생성
    {
        EquipmentItem weapon = allWeaponsDataList.FirstOrDefault(w =>
            (w.itemName == item.itemName) && (w.rarity == item.rarity)).Clone();
        if (weapon != null)
        {
            weapon.enhancementValue = item.enhancementValue;
            if (item.enhanceActions == null)
            {
                item.enhanceActions = weapon.GetRandomEnhanceByRarity();
            }
            else
            {
                Debug.Log($"CreateEquipmentItem: Using existing enhanceActions for item {weapon.itemName} with rarity {weapon.rarity}");
            }
            
            Debug.Log($"CreateEquipmentItem: Creating item {weapon.itemName} with rarityEnhances {item.enhanceActions.Count}");
            weapon.AddStatByType(item.enhanceActions);

            //weapon.SetValuesWithEnhancement(); 임시로 주석 처리. 강화 기능 구현 전까지는 필요 없음
            return weapon;
        }
        return null;
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
        public List<AddedStatType> enhanceActions = null;//강화 수치에 따른 능력치 증가 액션 리스트
        //public int acquiredIndex;//획득 순서 추적용

        public InventoryItem()
        {
        }
        /*
                public InventoryItem(string itemName, ItemRarity rarity, int enhancementValue)
                {
                    this.itemName = itemName;
                    this.rarity = rarity;
                    this.enhancementValue = enhancementValue;
                    this.amount = 0;
                    //this.acquiredIndex = -1;
                }
        */
        public InventoryItem(InventoryItem item)
        {
            itemName = item.itemName;
            rarity = item.rarity;
            enhancementValue = item.enhancementValue;
            amount = item.amount;
            enhanceActions = item.enhanceActions;
            //acquiredIndex = item.acquiredIndex;
        }

        public InventoryItem(EquipmentItem item)
        {
            itemName = item.itemName;
            rarity = item.rarity;
            enhancementValue = item.enhancementValue;
            amount = 0;
            //acquiredIndex = -1;
        }
    }

}

