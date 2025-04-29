using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponsInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform weaponsContainer;  // ���� ������ ��ġ�� �����̳�
    [SerializeField] private GameObject weaponSlotPrefab; // ���� ���� ������ (ItemSlot)

    [Header("References")]
    [SerializeField] private LevelUpUI levelUpUI;        // ������ UI ���� (�̺�Ʈ ������)

    [Header("Settings")]
    [SerializeField] private int maxSlots = 6;           // �ִ� ���� ����

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
        // LevelUpUI �̺�Ʈ ����
        if (levelUpUI == null)
            levelUpUI = FindObjectOfType<LevelUpUI>();
        if (levelUpUI != null)
            levelUpUI.onPanelClosed += UpdateWeaponsDisplay;
        else
            Debug.LogWarning("[WeaponsInventoryUI] LevelUpUI�� ã�� �� �����ϴ�. �κ��丮 �ڵ� ������ �������� �ʽ��ϴ�.");

        // PlayerAttack ���� ��������
        if (GameManager.Instance?.playerAttack != null)
            playerAttack = GameManager.Instance.playerAttack;
        else
            Debug.LogError("[WeaponsInventoryUI] PlayerAttack ������ ã�� �� �����ϴ�!");

        // �ʱ� ǥ��
        UpdateWeaponsDisplay();
    }

    private void OnDestroy()
    {
        // ���� ����
        if (levelUpUI != null)
            levelUpUI.onPanelClosed -= UpdateWeaponsDisplay;
    }

    private void InitializeWeaponSlots()
    {
        if (weaponsContainer == null || weaponSlotPrefab == null)
        {
            Debug.LogError("[WeaponsInventoryUI] weaponsContainer �Ǵ� weaponSlotPrefab�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // ���� ���� ����
        foreach (Transform child in weaponsContainer)
            Destroy(child.gameObject);
        weaponSlots.Clear();

        // ���� ����
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(weaponSlotPrefab, weaponsContainer);
            slotObj.name = $"WeaponSlot_{i}";

            // ������Ʈ ��������
            Image itemImg = slotObj.transform.Find("Item").GetComponent<Image>();
            TextMeshProUGUI countTxt = slotObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

            // �ʱ� ��Ȱ��ȭ
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
    /// �κ��丮 UI ����
    /// </summary>
    public void UpdateWeaponsDisplay()
    {
        Debug.Log("[WeaponsInventoryUI] UpdateWeaponsDisplay ȣ���");

        // ��� ���� �ʱ�ȭ
        foreach (var slot in weaponSlots)
        {
            slot.itemImage.sprite = null;
            slot.itemImage.gameObject.SetActive(false);
            slot.countText.text = string.Empty;
            slot.countText.gameObject.SetActive(false);
            slot.weaponId = string.Empty;
        }

        if (playerAttack == null) return;

        // ���� Ÿ�� �� �ְ� ��� ���⸸ ǥ��
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

            // SO�� �Ҵ�� ��������Ʈ ���
            Sprite icon = sw.weaponData?.weaponSprite;
            if (icon != null)
            {
                slot.itemImage.sprite = icon;
                slot.itemImage.gameObject.SetActive(true);
            }

            // ����(���� Ƚ��) ǥ��
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
    /// ���� ī��Ʈ ���� �� UI ����
    /// </summary>
    public void IncrementWeaponSelectionCount(string weaponId)
    {
        if (!weaponSelectionCount.ContainsKey(weaponId))
            weaponSelectionCount[weaponId] = 0;
        weaponSelectionCount[weaponId]++;
        UpdateWeaponsDisplay();
    }
}
