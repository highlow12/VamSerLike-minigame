using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponsInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform weaponsContainer; // ���� �����ܵ��� ǥ�õ� �����̳�
    [SerializeField] private GameObject weaponSlotPrefab; // ���� ���� ������ (ItemSlot > Bg, Item, Text)

    [Header("Settings")]
    [SerializeField] private int maxSlots = 6; // �ִ� ǥ���� ���� ���� ��
    [SerializeField] private int maxSelectionCount = 5; // ���� ���� �ִ� ī��Ʈ

    // ���� ��� ����
    private PlayerAttack playerAttack;
    private LevelUpUI levelUpUI;
    private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
    private Dictionary<string, int> weaponSelectionCount = new Dictionary<string, int>(); // ���⺰ ���� Ƚ�� (���� �� ���� ����)

    // ��������Ʈ ĳ�̿� ��ųʸ� �߰�
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    // ���� ���� ���� ���� Ŭ����
    [System.Serializable]
    private class WeaponSlot
    {
        public GameObject slotObject;
        public Image bgImage;
        public Image itemImage;
        public TextMeshProUGUI countText;
        public string weaponId; // ���� �ĺ��� (Ÿ�� + ���)
        public bool isMainWeapon;
    }

    private void Awake()
    {
        // ���� ���� �ʱ�ȭ
        InitializeWeaponSlots();

        // ���� ���� �� ���� ���� ī��Ʈ �ʱ�ȭ
        weaponSelectionCount.Clear();
    }

    private void Start()
    {
        // �÷��̾� ���� ���� ��������
        if (GameManager.Instance != null && GameManager.Instance.playerAttack != null)
        {
            playerAttack = GameManager.Instance.playerAttack;
            Debug.Log("[WeaponsInventoryUI] PlayerAttack ������ ã�ҽ��ϴ�.");
        }
        else
        {
            Debug.LogError("[WeaponsInventoryUI] PlayerAttack ������ ã�� �� �����ϴ�!");
            return;
        }

        // LevelUpUI ã��
        levelUpUI = FindObjectOfType<LevelUpUI>();
        if (levelUpUI != null)
        {
            // �г� ���� �̺�Ʈ ����
            levelUpUI.onPanelClosed += OnLevelUpPanelClosed;
            Debug.Log("[WeaponsInventoryUI] LevelUpUI �̺�Ʈ�� �����߽��ϴ�.");
        }
        else
        {
            Debug.LogWarning("[WeaponsInventoryUI] LevelUpUI ������Ʈ�� ������ ã�� �� �����ϴ�!");
        }

        // ���� ���� �ʱ� ������Ʈ
        UpdateWeaponsDisplay();

        // ���� ���� ����
        DiagnoseWeaponResources();

        // ���� ���� ���� ���
        PrintWeaponLevels();
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (levelUpUI != null)
        {
            levelUpUI.onPanelClosed -= OnLevelUpPanelClosed;
        }

        // ĳ�� Ŭ����
        spriteCache.Clear();
        weaponSelectionCount.Clear();
    }

    // ������ �г��� ���� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯
    private void OnLevelUpPanelClosed()
    {
        Debug.Log("[WeaponsInventoryUI] ������ �г��� �������ϴ�. ���� ���� ������Ʈ");

        // ���� ���� ���¿� ���Ͽ� ���� �߰�/���׷��̵�� ���� ī��Ʈ ����
        UpdateWeaponCounts();

        // ���� UI ������Ʈ
        UpdateWeaponsDisplay();

        // ���� ���� ���� ��� (������)
        PrintWeaponLevels();
    }

    private void InitializeWeaponSlots()
    {
        if (weaponsContainer == null || weaponSlotPrefab == null)
        {
            Debug.LogError("[WeaponsInventoryUI] ���� �����̳� �Ǵ� ���� �������� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // ���� ���� ����
        foreach (Transform child in weaponsContainer)
        {
            Destroy(child.gameObject);
        }
        weaponSlots.Clear();

        // ���� ����
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObject = Instantiate(weaponSlotPrefab, weaponsContainer);
            slotObject.name = $"WeaponSlot_{i}";

            // ���� ������Ʈ ��������
            Image bgImage = null;
            Image itemImage = null;
            TextMeshProUGUI countText = null;

            // Bg ã��
            Transform bgTransform = slotObject.transform.Find("Bg");
            if (bgTransform != null)
            {
                bgImage = bgTransform.GetComponent<Image>();
            }

            // Item ã��
            Transform itemTransform = slotObject.transform.Find("Item");
            if (itemTransform != null)
            {
                itemImage = itemTransform.GetComponent<Image>();
            }

            // Text ã��
            Transform textTransform = slotObject.transform.Find("Text (TMP)"); // ��Ȯ�� �̸����� ����
            if (textTransform == null)
            {
                textTransform = slotObject.transform.Find("Text"); // ��ü �̸� �õ�
            }

            if (textTransform != null)
            {
                countText = textTransform.GetComponent<TextMeshProUGUI>();
            }

            // ������Ʈ ����
            if (bgImage == null)
            {
                Debug.LogError($"[WeaponsInventoryUI] ���� {i}�� Bg �̹����� ã�� �� �����ϴ�!");
                continue;
            }

            if (itemImage == null)
            {
                Debug.LogError($"[WeaponsInventoryUI] ���� {i}�� Item �̹����� ã�� �� �����ϴ�!");
                continue;
            }

            if (countText == null)
            {
                Debug.LogError($"[WeaponsInventoryUI] ���� {i}�� Text�� ã�� �� �����ϴ�!");
                continue;
            }

            // ��� ������Ʈ�� ��ȿ�� ��쿡�� ���� �߰�
            // �⺻ ���� ����
            itemImage.sprite = null;
            itemImage.gameObject.SetActive(false); // ��Ȱ��ȭ�� ����
            countText.gameObject.SetActive(false); // ��Ȱ��ȭ�� ����

            // ���� ���� ����
            WeaponSlot slot = new WeaponSlot
            {
                slotObject = slotObject,
                bgImage = bgImage,
                itemImage = itemImage,
                countText = countText,
                weaponId = "",
                isMainWeapon = false
            };

            weaponSlots.Add(slot);
            slotObject.SetActive(true); // ��� ������ Ȱ��ȭ ���·� ����

            Debug.Log($"[WeaponsInventoryUI] ���� {i} �ʱ�ȭ ����");
        }

        Debug.Log($"[WeaponsInventoryUI] �� {weaponSlots.Count}���� ���� ���� �ʱ�ȭ��");
    }

    // ���� ī��Ʈ ������Ʈ - ���� ���õ� ������ ī��Ʈ�� ������Ŵ
    private void UpdateWeaponCounts()
    {
        if (playerAttack == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerAttack != null)
            {
                playerAttack = GameManager.Instance.playerAttack;
            }
            else
            {
                Debug.LogError("[WeaponsInventoryUI] PlayerAttack ������ ã�� �� �����ϴ�!");
                return;
            }
        }

        // ���� ���õ� ���� �������� (LevelUpUI���� ������ ����)
        Weapon.SubWeapon selectedWeapon = GetLastSelectedWeapon();

        if (selectedWeapon != null)
        {
            string weaponType = selectedWeapon.weaponType.ToString();
            // ���� Ÿ�Ը����� ID ���� (��� ����) - ���� Ÿ���� ����� ��ް� ������� ���� ī��Ʈ ����
            string weaponId = $"Sub_{weaponType}";

            IncrementWeaponSelectionCount(weaponId);
            Debug.Log($"[WeaponsInventoryUI] ���õ� ���깫�� {weaponId}�� ���� Ƚ���� ������Ʈ�߽��ϴ�.");
        }
        else
        {
            Debug.LogWarning("[WeaponsInventoryUI] ���õ� ���⸦ ã�� �� �����ϴ�.");
        }
    }

    // ���������� ���õ� ���깫�� ã�� (�ӽ� ���� - ���� ���� �ý��ۿ� �°� ���� �ʿ�)
    private Weapon.SubWeapon GetLastSelectedWeapon()
    {
        // �ӽ� ����: �����δ� LevelUpUI�� �ٸ� �ý��ۿ��� ��� ������ ���� ������ �����;� ��
        // ���⼭�� ���� �ֱٿ� �߰��� ���⸦ ��ȯ (����)
        if (playerAttack != null && playerAttack.subWeapons.Count > 0)
        {
            return playerAttack.subWeapons[playerAttack.subWeapons.Count - 1];
        }

        return null;
    }

    // ���� ���� ��������Ʈ �ε� - ������ ����
    private Sprite LoadSubWeaponSprite(string displayName, int weaponGrade)
    {
        // ĳ�� Ű ����
        string cacheKey = $"SubWeapon_{displayName}_{weaponGrade}";

        // �α� ��� - ������
        Debug.Log($"[WeaponsInventoryUI] ���� ���� ��������Ʈ �ε� �õ�: {displayName}_{weaponGrade}, ĳ�� Ű: {cacheKey}");

        // ĳ�ÿ� ������ ��ȯ
        if (spriteCache.TryGetValue(cacheKey, out Sprite cachedSprite))
        {
            Debug.Log($"[WeaponsInventoryUI] ĳ�ÿ��� ���� ���� ��������Ʈ �ε� ����: {displayName}_{weaponGrade}");
            return cachedSprite;
        }

        Sprite weaponSprite = null;

        try
        {
            // 1. ScriptableObject���� ��������Ʈ �ε� �õ�
            SubWeaponSO subWeaponSO = Resources.Load<SubWeaponSO>($"ScriptableObjects/SubWeapon/{displayName}/{displayName}_{weaponGrade}");
            if (subWeaponSO != null && subWeaponSO.weaponSprite != null)
            {
                weaponSprite = subWeaponSO.weaponSprite;
                Debug.Log($"[WeaponsInventoryUI] SO���� ���� ���� ��������Ʈ �ε� ����: {displayName}_{weaponGrade}");
                spriteCache[cacheKey] = weaponSprite;
                return weaponSprite;
            }
            else
            {
                Debug.Log($"[WeaponsInventoryUI] SO���� ��������Ʈ �ε� ����: ScriptableObjects/SubWeapon/{displayName}/{displayName}_{weaponGrade}");
            }

            // 2. �Ϲ� ��������Ʈ ��� �õ�
            string[] pathsToTry = new string[]
            {
                $"Sprites/SubWeapons/{displayName}_{weaponGrade}",
                $"Sprites/Weapons/{displayName}_{weaponGrade}",
                $"Sprites/UI/SubWeapons/{displayName}_{weaponGrade}",
                $"Sprites/Items/{displayName}_{weaponGrade}",
                $"SubWeapons/{displayName}_{weaponGrade}",
                $"Weapons/{displayName}_{weaponGrade}",
                
                // ������ ������ �� �����Ƿ� ���� ���� ������ �õ�
                $"Sprites/SubWeapons/{displayName.Replace(" ", "")}_{weaponGrade}",
                $"Sprites/Weapons/{displayName.Replace(" ", "")}_{weaponGrade}",
                
                // ���� �̸��� �õ� (��� ����)
                $"Sprites/SubWeapons/{displayName}",
                $"Sprites/Weapons/{displayName}",
                $"SubWeapons/{displayName}",
                $"Weapons/{displayName}"
            };

            foreach (string path in pathsToTry)
            {
                weaponSprite = Resources.Load<Sprite>(path);
                if (weaponSprite != null)
                {
                    Debug.Log($"[WeaponsInventoryUI] ��� '{path}'���� ���� ���� ��������Ʈ �ε� ����");
                    spriteCache[cacheKey] = weaponSprite;
                    return weaponSprite;
                }
                else
                {
                    Debug.Log($"[WeaponsInventoryUI] ��� '{path}'���� ��������Ʈ �ε� ����");
                }
            }

            // 3. �⺻ ������ �õ�
            weaponSprite = Resources.Load<Sprite>("Sprites/UI/DefaultWeaponIcon");
            if (weaponSprite != null)
            {
                Debug.Log($"[WeaponsInventoryUI] �⺻ �������� ����մϴ�: {displayName}_{weaponGrade}");
                spriteCache[cacheKey] = weaponSprite;
                return weaponSprite;
            }

            Debug.LogError($"[WeaponsInventoryUI] ��� ������� ���� ���� ��������Ʈ �ε� ����: {displayName}_{weaponGrade}");

            // 4. Resources ���� Ž��
            LogResourcesPaths();
        }
        catch (Exception e)
        {
            Debug.LogError($"[WeaponsInventoryUI] ���� ���� ��������Ʈ �ε� �� ����: {e.Message}\n{e.StackTrace}");
        }

        // ������ ����: ��Ÿ�ӿ� �⺻ ��������Ʈ ����
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = new Color(0.8f, 0.3f, 0.3f, 1.0f); // ������ �÷��̽�Ȧ��
        texture.SetPixels(colors);
        texture.Apply();
        Sprite fallbackSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
        spriteCache[cacheKey] = fallbackSprite;
        return fallbackSprite;
    }

    // Resources ������ ��θ� �α��ϴ� ����� �޼���
    private void LogResourcesPaths()
    {
        Debug.Log("[WeaponsInventoryUI] Resources ���丮�� �˻��մϴ�...");
        string resourcesPath = System.IO.Path.Combine(Application.dataPath, "Resources");
        if (System.IO.Directory.Exists(resourcesPath))
        {
            Debug.Log($"[WeaponsInventoryUI] Resources ���丮 ���: {resourcesPath}");

            // ��� ���丮 �α� (�ִ� 2�ܰ� ����)
            try
            {
                Debug.Log("[WeaponsInventoryUI] Resources ���丮 ����:");
                foreach (string dir in System.IO.Directory.GetDirectories(resourcesPath))
                {
                    string dirName = System.IO.Path.GetFileName(dir);
                    Debug.Log($" - {dirName}/");

                    foreach (string subDir in System.IO.Directory.GetDirectories(dir))
                    {
                        string subDirName = System.IO.Path.GetFileName(subDir);
                        Debug.Log($"   - {dirName}/{subDirName}/");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaponsInventoryUI] ���丮 �˻� �� ����: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[WeaponsInventoryUI] Resources ���丮�� ã�� �� �����ϴ�");
        }
    }

    // ���� ���ҽ� ���� ����
    public void DiagnoseWeaponResources()
    {
        Debug.Log("=== ���� ���ҽ� ���� ���� ===");

        // Resources ���丮�� �����ϴ��� Ȯ��
        string resourcesPath = System.IO.Path.Combine(Application.dataPath, "Resources");
        bool resourcesExists = System.IO.Directory.Exists(resourcesPath);
        Debug.Log($"Resources ���丮 ���� ����: {resourcesExists}");

        if (resourcesExists)
        {
            try
            {
                // ��� SubWeaponSO ���ҽ� �α�
                SubWeaponSO[] soAssets = Resources.LoadAll<SubWeaponSO>("");
                Debug.Log($"�߰ߵ� SubWeaponSO ���ҽ�: {soAssets.Length}��");
                foreach (SubWeaponSO asset in soAssets)
                    Debug.Log($"  - {asset.name} (Ÿ��: {asset.weaponType}, ���: {asset.weaponGrade}, ��������Ʈ: {(asset.weaponSprite != null ? "����" : "����")})");

                // Enum�� �ִ� ���� Ÿ�� �α�
                Debug.Log("Enum�� �ִ� ���� Ÿ��:");
                foreach (Weapon.SubWeapon.WeaponType weaponType in Enum.GetValues(typeof(Weapon.SubWeapon.WeaponType)))
                    Debug.Log($"  - {weaponType}");

                // �� ���� Ÿ�Ժ��� ��������Ʈ �ε� �õ�
                Debug.Log("�� ���� Ÿ�Ժ� ��������Ʈ �ε� �õ�:");
                foreach (Weapon.SubWeapon.WeaponType weaponType in Enum.GetValues(typeof(Weapon.SubWeapon.WeaponType)))
                {
                    string weaponTypeName = weaponType.ToString();
                    for (int grade = 1; grade <= 5; grade++)
                    {
                        Sprite sprite = LoadSubWeaponSprite(weaponTypeName, grade);
                        Debug.Log($"  - {weaponTypeName} (��� {grade}): {(sprite != null ? "����" : "����")}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"���� �� ���� �߻�: {e.Message}\n{e.StackTrace}");
            }
        }

        Debug.Log("=== ���� �Ϸ� ===");
    }

    public void UpdateWeaponsDisplay()
    {
        Debug.Log("[WeaponsInventoryUI] ���� ���÷��� ������Ʈ ��...");

        if (playerAttack == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerAttack != null)
            {
                playerAttack = GameManager.Instance.playerAttack;
            }
            else
            {
                Debug.LogError("[WeaponsInventoryUI] PlayerAttack ������ ã�� �� �����ϴ�!");
                return;
            }
        }

        // ��� ������ �� ���·� �ʱ�ȭ
        foreach (WeaponSlot slot in weaponSlots)
        {
            slot.itemImage.sprite = null;
            slot.itemImage.gameObject.SetActive(false); // ��Ȱ��ȭ
            slot.countText.gameObject.SetActive(false); // ��Ȱ��ȭ
            slot.weaponId = "";
            slot.isMainWeapon = false;

            // ���� ��ü�� ��� Ȱ��ȭ ���·� ����
            slot.slotObject.SetActive(true);
        }

        int slotIndex = 0;

        // ���� ����� ǥ������ ���� (���� ���⸸ ǥ���ϵ��� ����)

        // ���� ���� ǥ�� - ���� Ÿ�Ժ��� �з�
        // ���� ���� Ÿ�Ժ��� ���� ���� ����� ���⸸ ǥ���ϵ��� ����
        var highestGradeWeapons = new Dictionary<string, Weapon.SubWeapon>();

        foreach (Weapon.SubWeapon subWeapon in playerAttack.subWeapons)
        {
            string weaponType = subWeapon.weaponType.ToString();

            // �̹� �� ���� Ÿ���� �ִ��� Ȯ���ϰ�, �� ���� ������� ������Ʈ
            if (highestGradeWeapons.ContainsKey(weaponType))
            {
                if (subWeapon.weaponGrade > highestGradeWeapons[weaponType].weaponGrade)
                {
                    highestGradeWeapons[weaponType] = subWeapon;
                }
            }
            else
            {
                highestGradeWeapons[weaponType] = subWeapon;
            }
        }

        // ���� ������ ���� ������� UI ������Ʈ
        foreach (var weaponEntry in highestGradeWeapons)
        {
            if (slotIndex < weaponSlots.Count)
            {
                WeaponSlot slot = weaponSlots[slotIndex];
                Weapon.SubWeapon subWeapon = weaponEntry.Value;
                string weaponType = subWeapon.weaponType.ToString();
                string displayName = GetSubWeaponDisplayName(subWeapon.weaponType);

                try
                {
                    // ���� ���� ��������Ʈ �ε�
                    Sprite weaponSprite = LoadSubWeaponSprite(displayName, subWeapon.weaponGrade);

                    // ��������Ʈ ����
                    if (weaponSprite != null)
                    {
                        slot.itemImage.sprite = weaponSprite;
                        slot.itemImage.gameObject.SetActive(true); // Ȱ��ȭ�� ����

                        // ���� Ÿ�Ը����� ID ���� (���� Ÿ���� ���� ����)
                        string weaponId = $"Sub_{weaponType}";
                        slot.weaponId = weaponId;
                        slot.isMainWeapon = false;

                        // ���� Ƚ��(����) ǥ��
                        int count = GetWeaponSelectionCount(weaponId);
                        Debug.Log($"[WeaponsInventoryUI] ���깫�� {weaponId}�� ���� Ƚ��(����): {count}");

                        if (count > 0)
                        {
                            slot.countText.text = count.ToString();
                            slot.countText.gameObject.SetActive(true); // Ȱ��ȭ�� ����
                            Debug.Log($"[WeaponsInventoryUI] ���깫�� {weaponId}�� ���� {count}�� ǥ���մϴ�.");
                        }
                        else
                        {
                            slot.countText.gameObject.SetActive(false);
                            Debug.Log($"[WeaponsInventoryUI] ���깫�� {weaponId}�� ������ 0�̹Ƿ� �ؽ�Ʈ�� ǥ������ �ʽ��ϴ�.");
                        }
                    }
                    else
                    {
                        // ��������Ʈ�� ��� ���� Ƚ���� ǥ��
                        string weaponId = $"Sub_{weaponType}";
                        int count = GetWeaponSelectionCount(weaponId);
                        Debug.Log($"[WeaponsInventoryUI] ��������Ʈ ����, ���깫�� {weaponId}�� ����: {count}");

                        if (count > 0)
                        {
                            // ��������Ʈ�� ������ �ؽ�Ʈ�� ǥ�� (�ӽ÷� �� �̹��� ǥ��)
                            slot.itemImage.gameObject.SetActive(true); // ���� �̹����� Ȱ��ȭ
                            slot.countText.text = count.ToString();
                            slot.countText.gameObject.SetActive(true);
                            Debug.Log($"[WeaponsInventoryUI] ���깫�� {weaponId} ��������Ʈ ����, ������ ǥ��: {count}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[WeaponsInventoryUI] ���� ���� ǥ�� �� ����: {e.Message}\n{e.StackTrace}");
                }

                slotIndex++;
            }
        }

        // ����� ���� ���
        DebugWeaponSelections();
        Debug.Log($"[WeaponsInventoryUI] {slotIndex}���� ���� ������ ������Ʈ�Ǿ����ϴ�.");
    }

    // Ư�� ������ ���� Ƚ�� ������Ʈ (�ִ� 5�� ����)
    public void IncrementWeaponSelectionCount(string weaponId)
    {
        // ���� ID���� ������ ī��Ʈ ����
        if (!weaponSelectionCount.ContainsKey(weaponId))
        {
            weaponSelectionCount[weaponId] = 0;
            Debug.Log($"[WeaponsInventoryUI] ���ο� ���� {weaponId}�� ī��Ʈ�� �ʱ�ȭ�߽��ϴ�.");
        }

        // �ִ� ī��Ʈ ������ ��쿡�� ����
        if (weaponSelectionCount[weaponId] < maxSelectionCount)
        {
            weaponSelectionCount[weaponId]++;
            Debug.Log($"[WeaponsInventoryUI] ���� {weaponId}�� ���� Ƚ���� {weaponSelectionCount[weaponId]}�� �����߽��ϴ�.");

            // ī��Ʈ�� ����Ǹ� ��� UI ������Ʈ (���������� Ȱ��ȭ ����)
            // UpdateWeaponsDisplay();
        }
        else
        {
            Debug.Log($"[WeaponsInventoryUI] ���� {weaponId}�� ���� Ƚ���� �̹� �ִ�({maxSelectionCount})�� �����߽��ϴ�.");
        }

        // ����׿� ���
        DebugWeaponSelections();
    }

    // ���� ���� Ƚ�� ��������
    private int GetWeaponSelectionCount(string weaponId)
    {
        if (weaponSelectionCount.ContainsKey(weaponId))
        {
            Debug.Log($"[WeaponsInventoryUI] ���� {weaponId}�� ���� ī��Ʈ: {weaponSelectionCount[weaponId]}");
            return weaponSelectionCount[weaponId];
        }

        // ����׸� ���� �α� �߰�
        Debug.Log($"[WeaponsInventoryUI] ���� {weaponId}�� ���� ī��Ʈ�� �����ϴ� (0 ��ȯ)");
        return 0;
    }

    // ���� ���� ǥ�ø� ��������
    private string GetSubWeaponDisplayName(Weapon.SubWeapon.WeaponType weaponType)
    {
        if (WeaponStatProvider.Instance != null)
        {
            var subWeaponStat = WeaponStatProvider.Instance.subWeaponStats.Find(x => x.weaponType == weaponType);

            // ����ü�� null�� ���� ���� �� �����Ƿ� displayName �Ӽ��� Ȯ��
            if (!string.IsNullOrEmpty(subWeaponStat.displayName))
            {
                // �α� �߰� - ����׿�
                Debug.Log($"[WeaponsInventoryUI] GetSubWeaponDisplayName - Ÿ��: {weaponType}, �̸�: {subWeaponStat.displayName}");
                return subWeaponStat.displayName;
            }
        }
        else
        {
            Debug.LogWarning("[WeaponsInventoryUI] WeaponStatProvider.Instance�� null�Դϴ�. �⺻ Ÿ�Ը��� ����մϴ�.");
        }

        // �⺻������ Ÿ�Ը� ��ȯ
        return weaponType.ToString();
    }

    // ���� ������ ������ ����� ���� ��� (�׽�Ʈ��)
    private void DebugWeaponSelections()
    {
        Debug.Log("===== ���� ���� ���� ī��Ʈ ���� =====");
        foreach (var kvp in weaponSelectionCount)
        {
            Debug.Log($"���� ID: {kvp.Key}, ���� Ƚ��: {kvp.Value}");
        }
        Debug.Log("===================================");
    }

    // ��� ���� ������ �ʱ�ȭ (�׽�Ʈ��)
    public void ResetWeaponSelectionCounts()
    {
        weaponSelectionCount.Clear();
        Debug.Log("[WeaponsInventoryUI] ��� ���� ���� ī��Ʈ�� �ʱ�ȭ�Ǿ����ϴ�.");
        UpdateWeaponsDisplay(); // UI ��� ������Ʈ
    }

    // ������Ʈ�� ���̾�׷� ��� �޼��� (����� ���� ��ȭ)
    public void PrintWeaponLevels()
    {
        Debug.Log("===== ���� ���� ���� ���� =====");

        if (weaponSelectionCount.Count == 0)
        {
            Debug.Log("���� ���� ������ �����ϴ�.");
            return;
        }

        foreach (var kvp in weaponSelectionCount)
        {
            string weaponId = kvp.Key;
            int level = kvp.Value;

            // ���� Ÿ�� ���� (Sub_PaperPlane���� PaperPlane ����)
            string weaponTypeName = "�� �� ����";
            if (weaponId.StartsWith("Sub_"))
            {
                weaponTypeName = weaponId.Substring(4);
            }

            // ���� �ð�ȭ
            string levelBar = "";
            for (int i = 0; i < maxSelectionCount; i++)
            {
                if (i < level)
                    levelBar += "0"; // ä���� ����
                else
                    levelBar += "-"; // ����ִ� ����
            }

            Debug.Log($"{weaponTypeName}: {levelBar} ({level}/{maxSelectionCount})");
        }

        Debug.Log("==============================");
    }
}