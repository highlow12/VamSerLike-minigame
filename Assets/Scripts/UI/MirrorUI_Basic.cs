using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MirrorUI : MonoBehaviour
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
    
    private List<EquipmentItem> equipmentItems = new List<EquipmentItem>();
    private EquipmentItem equippedWeapon;
    private EquipmentItem equippedCloak;
    
    private EquipmentCategory currentCategory = EquipmentCategory.Weapon;
    private SortType currentSortType = SortType.ByRarity;
    
    private GameObject currentInfoPanel;
    private MirrorUIFunctions uiFunctions;
    
    void Awake()
    {
        uiFunctions = GetComponent<MirrorUIFunctions>();
        if (uiFunctions == null)
        {
            uiFunctions = gameObject.AddComponent<MirrorUIFunctions>();
        }
    }
    
    void Start()
    {
        InitializeUI();
        LoadMockItems();
        UpdateEquippedStats();
        PopulateItemGrid();
    }
    
    // UI 초기화
    private void InitializeUI()
    {
        // 닫기 버튼 초기화
        closeButton.onClick.AddListener(CloseMirror);
        
        // 카테고리 버튼 초기화
        weaponCategory.onClick.AddListener(() => SwitchCategory(EquipmentCategory.Weapon));
        cloakCategory.onClick.AddListener(() => SwitchCategory(EquipmentCategory.Cloak));
        
        // 정렬 버튼 초기화
        raritySort.onClick.AddListener(() => SortItems(SortType.ByRarity));
        acquiredSort.onClick.AddListener(() => SortItems(SortType.ByAcquired));
        
        // 강화 버튼 초기화
        enhancementButton.onClick.AddListener(OpenEnhancementMode);
        
        // 장비 슬롯 버튼 초기화
        weaponSlot.GetComponent<Button>().onClick.AddListener(() => ShowEquippedItemInfo(equippedWeapon, EquipmentCategory.Weapon));
        cloakSlot.GetComponent<Button>().onClick.AddListener(() => ShowEquippedItemInfo(equippedCloak, EquipmentCategory.Cloak));
    }
    
    // 샘플 아이템 로드
    private void LoadMockItems()
    {
        // 샘플 무기 아이템 추가
        equipmentItems.Add(new EquipmentItem("십자가", EquipmentCategory.Weapon, ItemRarity.A, "신성한 십자가 무기", 10, 0, 5, 0, 0, 0));
        equipmentItems.Add(new EquipmentItem("일반 검", EquipmentCategory.Weapon, ItemRarity.B, "평범한 검", 0, 0, 3, 5, 0, 0));
        equipmentItems.Add(new EquipmentItem("대검", EquipmentCategory.Weapon, ItemRarity.D, "무거운 대검", 0, 10, 8, 0, -5, 0));
        equipmentItems.Add(new EquipmentItem("단검", EquipmentCategory.Weapon, ItemRarity.B, "가벼운 단검", 0, 0, 2, 15, 5, 0));
        equipmentItems.Add(new EquipmentItem("암시장 샷건", EquipmentCategory.Weapon, ItemRarity.S, "강력한 샷건", 200, 0, 5, 20, 0, 13));
        
        // 샘플 망토 아이템 추가
        equipmentItems.Add(new EquipmentItem("일반 망토", EquipmentCategory.Cloak, ItemRarity.B, "평범한 망토", 50, 5, 0, 0, 0, 0));
        equipmentItems.Add(new EquipmentItem("가죽 망토", EquipmentCategory.Cloak, ItemRarity.C, "가죽 재질 망토", 30, 3, 0, 0, 5, 0));
        equipmentItems.Add(new EquipmentItem("강화 망토", EquipmentCategory.Cloak, ItemRarity.A, "강화된 망토", 100, 10, 0, 0, 0, 5));
        
        // 기본 장착 아이템 설정
        equippedWeapon = equipmentItems[4]; // 암시장 샷건
        equippedCloak = equipmentItems[5];  // 일반 망토
    }
}