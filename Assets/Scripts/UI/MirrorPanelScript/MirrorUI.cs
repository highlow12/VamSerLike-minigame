using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class MirrorUI : MonoBehaviour
{
    [Header("References")]
    public GameObject mirrorPanel;
    public Button closeButton;
    public Image playerPreviewImage;
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
    public ScrollRect itemScrollRect;

    [Header("Sort Buttons")]
    public Button raritySort;
    public Button acquiredSort;

    [Header("Category Buttons")]
    public Button weaponCategory;
    public Button cloakCategory;

    [Header("Enhancement Button")]
    public Button enhancementButton;

    [Header("Item Info Panel")]
    public GameObject itemInfoPanelPrefab;

    [Header("Data")]
    public WeaponDataLoader weaponDataLoader;

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
        InitializeUI();

        // weaponDataLoader에서 실제 무기 데이터 로드
        if (weaponDataLoader != null)
        {
            Main_LoadEquipmentItems();
        }
        else
        {
            // 데이터 로더가 없으면 테스트 데이터 사용
            LoadMockItems();
        }

        Core_UpdateEquippedStats();
        Grid_PopulateItemGrid();
    }

    // 장비 아이템 로드 (WeaponDataLoader 사용)
    protected void Main_LoadEquipmentItems()
    {
        equipmentItems.Clear();
        acquireOrderMap.Clear(); // 획득 순서 맵 초기화

        if (weaponDataLoader != null)
        {
            List<EquipmentItem> weapons = weaponDataLoader.GetWeapons();

            if (weapons != null && weapons.Count > 0)
            {
                Debug.Log($"WeaponDataLoader에서 {weapons.Count}개의 무기 데이터를 로드했습니다.");

                // 획득 순서 설정 (인덱스 기반)
                for (int i = 0; i < weapons.Count; i++)
                {
                    acquireOrderMap[weapons[i]] = i + 1;
                    equipmentItems.Add(weapons[i]);
                }

                // 첫 번째 무기를 기본 장착
                if (weapons.Count > 0)
                {
                    // 무기 카테고리에서 첫 번째 아이템 찾기
                    EquipmentItem firstWeapon = weapons.Find(item => item.category == EquipmentCategory.Weapon);
                    if (firstWeapon != null)
                    {
                        equippedWeapon = firstWeapon;
                        Debug.Log($"기본 무기 '{firstWeapon.itemName}'이(가) 장착되었습니다.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("WeaponDataLoader에서 무기 데이터를 가져올 수 없습니다. 테스트 데이터를 사용합니다.");
                LoadMockItems();
            }
        }
        else
        {
            Debug.LogWarning("무기 데이터 로더를 찾을 수 없어 기본 아이템을 사용합니다.");
            LoadMockItems();
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
        enhancementButton.onClick.AddListener(Grid_OpenEnhancementMode);
        weaponSlot.GetComponent<Button>().onClick.AddListener(() => Item_ShowEquippedItemInfo(equippedWeapon, EquipmentCategory.Weapon));
        cloakSlot.GetComponent<Button>().onClick.AddListener(() => Item_ShowEquippedItemInfo(equippedCloak, EquipmentCategory.Cloak));
    }

    // 샘플 아이템 로드 (테스트용)
    private void LoadMockItems()
    {
        equipmentItems.Clear();
        acquireOrderMap.Clear();

        // 샘플 무기 아이템 추가
        var crossWeapon = new EquipmentItem("십자가", EquipmentCategory.Weapon, ItemRarity.A, "신성한 십자가 무기", 10, 0, 5, 0, 0, 0);
        var normalSword = new EquipmentItem("일반 검", EquipmentCategory.Weapon, ItemRarity.B, "평범한 검", 0, 0, 3, 5, 0, 0);
        var bigSword = new EquipmentItem("대검", EquipmentCategory.Weapon, ItemRarity.D, "무거운 대검", 0, 10, 8, 0, -5, 0);
        var dagger = new EquipmentItem("단검", EquipmentCategory.Weapon, ItemRarity.B, "가벼운 단검", 0, 0, 2, 15, 5, 0);
        var shotgun = new EquipmentItem("암시장 샷건", EquipmentCategory.Weapon, ItemRarity.S, "강력한 샷건", 200, 0, 5, 20, 0, 13);

        // 샘플 망토 아이템 추가
        var normalCloak = new EquipmentItem("일반 망토", EquipmentCategory.Cloak, ItemRarity.B, "평범한 망토", 50, 5, 0, 0, 0, 0);
        var leatherCloak = new EquipmentItem("가죽 망토", EquipmentCategory.Cloak, ItemRarity.C, "가죽 재질 망토", 30, 3, 0, 0, 5, 0);
        var enhancedCloak = new EquipmentItem("강화 망토", EquipmentCategory.Cloak, ItemRarity.A, "강화된 망토", 100, 10, 0, 0, 0, 5);

        // 아이템 추가 및 획득 순서 설정
        equipmentItems.Add(crossWeapon); acquireOrderMap[crossWeapon] = 1;
        equipmentItems.Add(normalSword); acquireOrderMap[normalSword] = 2;
        equipmentItems.Add(bigSword); acquireOrderMap[bigSword] = 3;
        equipmentItems.Add(dagger); acquireOrderMap[dagger] = 4;
        equipmentItems.Add(shotgun); acquireOrderMap[shotgun] = 5;
        equipmentItems.Add(normalCloak); acquireOrderMap[normalCloak] = 6;
        equipmentItems.Add(leatherCloak); acquireOrderMap[leatherCloak] = 7;
        equipmentItems.Add(enhancedCloak); acquireOrderMap[enhancedCloak] = 8;

        // 기본 장착 아이템 설정
        equippedWeapon = shotgun;       // 암시장 샷건
        equippedCloak = normalCloak;    // 일반 망토

        Debug.Log("테스트 아이템 데이터를 사용합니다.");
    }
}