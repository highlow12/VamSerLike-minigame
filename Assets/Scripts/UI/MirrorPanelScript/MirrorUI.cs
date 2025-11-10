using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class MirrorUI : Singleton<MirrorUI>
{
    [Header("References")]
    public GameObject mirrorPanel;
    public Button closeButton;
    public GameObject weaponSlot;
    public GameObject cloakSlot;

    [Header("Stat Panel")]
    public GameObject statPanel;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI moveSpeedText;
    public TextMeshProUGUI visionRangeText;

    [Header("Equipment Grid")]
    public GameObject itemGridContainer;
    public GameObject itemSlotPrefab;
    public int gridColumns = 4;
    //public ScrollRect itemScrollRect;

    [Header("Sort Buttons")]
    public Button raritySort;
    public Button acquiredSort;

    [Header("Category Buttons")]
    public Button weaponCategory;
    public Button cloakCategory;

    [Header("Enhancement Button")]
    public Button enhancementButton;
    public Transform enhancementPanel; // Enhancement panel to be set in the inspector

    [Header("Item Info Panel")]
    public GameObject itemInfoPanelPrefab;

    [Header("Data")]
    public WeaponDataLoader weaponDataLoader;

    [Header("Player Preview")]
    public Image playerPreviewImage;
    public Image weaponPreviewImage;
    public float idleMultiplier = 1.2f;
    public float idleMovingMultiplier = 2f;
    public float idleInterval = 5f;





    protected List<EquipmentItem> equipmentItems = new List<EquipmentItem>();
    
    
    protected EquipmentItem equippedWeapon;
    protected EquipmentItem equippedCloak;

    protected EquipmentCategory currentCategory = EquipmentCategory.Weapon;
    protected SortType currentSortType = SortType.ByRarity;

    protected GameObject currentInfoPanel;

    // 아이템 획득 순서를 저장하는 Dictionary 추가
    protected Dictionary<EquipmentItem, int> acquireOrderMap = new Dictionary<EquipmentItem, int>();

    void Awake()
    {
        // Awake에서 WeaponDataLoader 컴포넌트 확인
        if (weaponDataLoader == null)
        {
            weaponDataLoader = GetComponent<WeaponDataLoader>();
            if (weaponDataLoader == null)
            {
                // 컴포넌트가 없으면 자동으로 추가
                weaponDataLoader = gameObject.AddComponent<WeaponDataLoader>();
                Debug.Log("WeaponDataLoader 컴포넌트가 자동으로 추가되었습니다.");
            }
        }
    }

    void Start()
    {
        Debug.Log("Start 메서드가 호출되었습니다."); // Start 메서드 호출 확인 로그 추가

        InitializeUI();

        Core_UpdateEquippedStats();
        Grid_PopulateItemGrid();//초기 그리드 카테고리와 정렬 기준에 따라 그리드 채우기
    }
    // 장비 아이템 로드 (WeaponDataLoader 사용)
    protected void Main_LoadEquipmentItems()
    {
        equipmentItems.Clear();
        acquireOrderMap.Clear(); // 획득 순서 맵 초기화

        if (weaponDataLoader != null)
        {
            //List<EquipmentItem> weapons = weaponDataLoader.GetWeapons();//임시코드인듯. weapons는 실제로 보유하고 있는 무기 리스트임. GetWeapons()는 가능한 모든 무기들의 리스트임
            List<EquipmentItem> weapons = HomeInventoryManager.Instance != null ? HomeInventoryManager.Instance.GetOrderedItems() : null;
            //List<EquipmentItem> weapons = HomeInventoryManager.Instance != null ? HomeInventoryManager.Instance.GetAllWeaponsDataList() : null;

            if (weapons != null && weapons.Count > 0)
            {
                Debug.Log($"WeaponDataLoader에서 {weapons.Count}개의 무기 데이터를 로드했습니다.");

                for (int i = 0; i < weapons.Count; i++)
                {
                    acquireOrderMap[weapons[i]] = i + 1;
                    equipmentItems.Add(weapons[i]);
                }

                string equippedWeaponCode = HomeInventoryManager.Instance.LoadEquippedItemCode(EquipmentCategory.Weapon);
                for (int i = 0; i < equipmentItems.Count; i++)
                {
                    EquipmentItem item = equipmentItems[i];
                    if (HomeInventoryManager.Instance.GetItemCode(item) == equippedWeaponCode)
                    {
                        EquipItem(weaponSlot.GetComponent<ItemSlot>(), item);
                        break;
                    }
                    else if (i == equipmentItems.Count - 1)
                    {
                        Debug.Log("저장된 장착 무기 데이터를 찾을 수 없습니다.");
                        //저장된 무기 데이터 없으면 장착 안함
                        UnequipItem(equippedWeapon);
                    }
                }

                string equippedCloakCode = PlayerPrefs.GetString("EquippedCloak", "");
                for (int i = 0; i < equipmentItems.Count; i++)
                {
                    EquipmentItem item = equipmentItems[i];
                    if (HomeInventoryManager.Instance.GetItemCode(item) == equippedCloakCode)
                    {
                        EquipItem(cloakSlot.GetComponent<ItemSlot>(), item);
                        break;
                    }
                    else if (i == equipmentItems.Count - 1)
                    {
                        Debug.Log("저장된 장착 망토 데이터를 찾을 수 없습니다.");
                        //저장된 무기 데이터 없으면 장착 안함
                        UnequipItem(equippedCloak);
                    }
                }
            }
            else
            {
                //Debug.LogWarning("WeaponDataLoader에서 무기 데이터를 가져올 수 없습니다. 샘플 아이템을 사용합니다.");
                //LoadMockItems();
                Debug.LogWarning("무기 데이터가 없습니다. 오류거나 HomeInventoryManager의 Inventory 데이터가 비어있습니다.");
            }
        }
        else
        {
            //Debug.LogWarning("WeaponDataLoader를 찾을 수 없어 샘플 아이템을 사용합니다.");
            //LoadMockItems();
            Debug.LogWarning("WeaponDataLoader를 찾을 수 없습니다.");
        }

        //필요 없는 코드인듯. 어짜피 카테고리와 정렬 기준에 따라 Grid_PopulateItemGrid에서 다시 채워짐
        //아니 필요한 코드였네. 아이템 슬롯 자체를 만들어주는 코드구나. 근데 왜 equipmentItems mapping까지 여기서 하지?
        AdjustItemGrid();
    }

    protected void AdjustItemGrid()
    {
        Debug.Log("AdjustItemGrid 메서드가 호출되었습니다.");

        int itemCount = equipmentItems.Count;
        Debug.Log($"로드된 아이템 개수: {itemCount}");

        // 최소값 설정
        int minItems = 16;

        // 기존 슬롯 제거
        if (itemGridContainer == null)
        {
            Debug.LogWarning("itemGridContainer가 null입니다. Prefab을 확인하세요.");
            return;
        }
        else
        {
            foreach (Transform child in itemGridContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }

        

        // 슬롯 동적 생성
        //장착된 아이템은 그리드에서 제외. equipmentItems에서 장착된 아이템 수만큼 빼주고, 해당하는 인덱스는 건너뜀
        int totalSlots = Mathf.Max(itemCount - (equippedWeapon != null ? 1 : 0) - (equippedCloak != null ? 1 : 0), minItems);
        Debug.Log($"생성할 슬롯 개수: {totalSlots}");

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject newSlot = Instantiate(itemSlotPrefab, itemGridContainer.transform);
            if (newSlot == null)
            {
                Debug.LogError("itemSlotPrefab이 null입니다. Prefab을 확인하세요.");
                continue;
            }

            Debug.Log($"슬롯 생성: {newSlot.name}");
            newSlot.name = $"ItemSlot_{i + 1}";


            //아이템 바인딩은 Grid_PopulateItemGrid에서 처리함
            /*// 슬롯에 아이템 데이터 바인딩
            ItemSlot itemSlot = newSlot.GetComponent<ItemSlot>();
            if (itemSlot == null)
            {
                Debug.LogError("ItemSlot 컴포넌트를 찾을 수 없습니다. Prefab을 확인하세요.");
                continue;
            }

            if (i < itemCount)
            {
                //EquipmentItem item = equipmentItems[i];
                if (equipmentItems[i] == null)
                {
                    Debug.LogError($"equipmentItems[{i}]가 null입니다. 데이터를 확인하세요.");
                    continue;
                }

                if(equipmentItems[i].isEquipped)
                {
                    Debug.Log($"장착된 아이템 건너뜀: {equipmentItems[i].itemName}");
                    continue; // 이미 장착된 아이템은 건너뜁니다.
                }

                try
                {
                    itemSlot.SetItem(equipmentItems[i], weaponDataLoader.GetItemSprite(equipmentItems[i].itemName));

                    //아이템 프리팹에서 관리?
                    //itemSlot.GetComponent<Button>().onClick.AddListener(() => Item_ShowEquippedItemInfo(equipmentItems[i], itemSlot.transform.position));
                    
                    Debug.Log($"아이템 바인딩 성공: {equipmentItems[i].itemName}, {weaponDataLoader.GetItemSprite(equipmentItems[i].itemName)}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"아이템 바인딩 중 오류 발생: {ex.Message}");
                }
            }*/
        }
    }

    // UI 초기화
    private void InitializeUI()
    {
        closeButton.onClick.AddListener(Grid_CloseMirror);
        weaponCategory.onClick.AddListener(() => Grid_SwitchCategory(EquipmentCategory.Weapon));
        cloakCategory.onClick.AddListener(() => Grid_SwitchCategory(EquipmentCategory.Cloak));
        raritySort.onClick.AddListener(() => Grid_SortItems(SortType.ByRarity));
        acquiredSort.onClick.AddListener(() => Grid_SortItems(SortType.ByAcquired));
        enhancementButton.onClick.AddListener(ToggleEnhancementPanel);
        //item만 할당해주면 자동 OnClick되게 설정해보자
        //weaponSlot.GetComponent<Button>().onClick.AddListener(() => Item_ShowEquippedItemInfo(equippedWeapon, weaponSlot.transform.position));
        //cloakSlot.GetComponent<Button>().onClick.AddListener(() => Item_ShowEquippedItemInfo(equippedCloak, cloakSlot.transform.position));
    }

    private void ToggleEnhancementPanel()
    {
        if (enhancementPanel != null)
        {
            bool isActive = enhancementPanel.gameObject.activeSelf;
            enhancementPanel.gameObject.SetActive(!isActive); // Toggle the panel's active state
        }
    }

    public void weaponCategory_Grid_SwitchCategory()
    {
        Grid_SwitchCategory(EquipmentCategory.Weapon);
        Debug.Log("무기 카테고리로 전환되었습니다.");
    }public void cloakCategory_Grid_SwitchCategory()
    {
        Grid_SwitchCategory(EquipmentCategory.Cloak);
        Debug.Log("망토 카테고리로 전환되었습니다.");
    }

    
}