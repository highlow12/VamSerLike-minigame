using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponsInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform weaponsContainer;  // 무기 슬롯을 배치할 컨테이너
    [SerializeField] private GameObject weaponSlotPrefab; // 무기 슬롯 프리팹 (ItemSlot)

    [Header("References")]
    [SerializeField] private LevelUpUI levelUpUI;        // 레벨업 UI 참조 (이벤트 구독용)

    [Header("Settings")]
    [SerializeField] private int maxSlots = 6;           // 최대 슬롯 개수

    private PlayerAttack playerAttack;
    private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
    private Dictionary<string, int> weaponSelectionCount = new Dictionary<string, int>();

    [Serializable]
    private class WeaponSlot
    {
        public GameObject slotObject;
        public Image itemImage;
        public TextMeshProUGUI countText;
        public string weaponId;
    }

    private void Awake()
    {
        InitializeWeaponSlots();
    }

    private void Start()
    {
        // LevelUpUI 이벤트 구독
        if (levelUpUI == null)
            levelUpUI = FindObjectOfType<LevelUpUI>();
        if (levelUpUI != null)
            levelUpUI.onPanelClosed += UpdateWeaponsDisplay;
        else
            Debug.LogWarning("[WeaponsInventoryUI] LevelUpUI를 찾을 수 없습니다. 인벤토리 자동 갱신이 동작하지 않습니다.");

        // PlayerAttack 참조 가져오기
        if (GameManager.Instance?.playerAttack != null)
            playerAttack = GameManager.Instance.playerAttack;
        else
            Debug.LogError("[WeaponsInventoryUI] PlayerAttack 참조를 찾을 수 없습니다!");

        // 초기 표시
        UpdateWeaponsDisplay();
    }

    private void OnDestroy()
    {
        // 구독 해제
        if (levelUpUI != null)
            levelUpUI.onPanelClosed -= UpdateWeaponsDisplay;
    }

    private void InitializeWeaponSlots()
    {
        if (weaponsContainer == null || weaponSlotPrefab == null)
        {
            Debug.LogError("[WeaponsInventoryUI] weaponsContainer 또는 weaponSlotPrefab이 할당되지 않았습니다!");
            return;
        }

        // 기존 슬롯 제거
        foreach (Transform child in weaponsContainer)
            Destroy(child.gameObject);
        weaponSlots.Clear();

        // 슬롯 생성
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(weaponSlotPrefab, weaponsContainer);
            slotObj.name = $"WeaponSlot_{i}";

            // 컴포넌트 가져오기
            Image itemImg = slotObj.transform.Find("Item").GetComponent<Image>();
            TextMeshProUGUI countTxt = slotObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

            // 초기 비활성화
            itemImg.gameObject.SetActive(false);
            countTxt.gameObject.SetActive(false);

            weaponSlots.Add(new WeaponSlot
            {
                slotObject = slotObj,
                itemImage = itemImg,
                countText = countTxt,
                weaponId = string.Empty
            });
        }
    }

    /// <summary>
    /// 인벤토리 UI 갱신
    /// </summary>
    public void UpdateWeaponsDisplay()
    {
        Debug.Log("[WeaponsInventoryUI] UpdateWeaponsDisplay 호출됨");

        // 모든 슬롯 초기화
        foreach (var slot in weaponSlots)
        {
            slot.itemImage.sprite = null;
            slot.itemImage.gameObject.SetActive(false);
            slot.countText.text = string.Empty;
            slot.countText.gameObject.SetActive(false);
            slot.weaponId = string.Empty;
        }

        if (playerAttack == null) return;

        // 같은 타입 중 최고 등급 무기만 표시
        var highest = new Dictionary<string, Weapon.SubWeapon>();
        foreach (var sw in playerAttack.subWeapons)
        {
            string key = sw.weaponType.ToString();
            if (!highest.ContainsKey(key) || sw.weaponGrade > highest[key].weaponGrade)
                highest[key] = sw;
        }

        int idx = 0;
        foreach (var entry in highest)
        {
            if (idx >= weaponSlots.Count) break;

            var sw = entry.Value;
            var slot = weaponSlots[idx];

            // SO에 할당된 스프라이트 사용
            Sprite icon = sw.weaponData?.weaponSprite;
            if (icon != null)
            {
                slot.itemImage.sprite = icon;
                slot.itemImage.gameObject.SetActive(true);
            }

            // 레벨(선택 횟수) 표시
            string id = $"Sub_{sw.weaponType}";
            slot.weaponId = id;
            if (weaponSelectionCount.TryGetValue(id, out int count) && count > 0)
            {
                slot.countText.text = count.ToString();
                slot.countText.gameObject.SetActive(true);
            }

            idx++;
        }
    }

    /// <summary>
    /// 선택 카운트 증가 및 UI 갱신
    /// </summary>
    public void IncrementWeaponSelectionCount(string weaponId)
    {
        if (!weaponSelectionCount.ContainsKey(weaponId))
            weaponSelectionCount[weaponId] = 0;
        weaponSelectionCount[weaponId]++;
        UpdateWeaponsDisplay();
    }
}
