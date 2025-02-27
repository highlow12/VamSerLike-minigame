using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Image experienceBarFill; // �����̴� ��� Image�� ����� ���
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Item Selection UI")]
    [SerializeField] private GameObject[] itemButtons; // 3���� ���� ��ư
    [SerializeField] private Image[] itemIcons;
    [SerializeField] private TextMeshProUGUI[] itemNames;
    [SerializeField] private TextMeshProUGUI[] itemDescriptions;

    // ������ ������ ����
    [SerializeField] private Sprite[] mainWeaponIcons;
    [SerializeField] private Sprite[] subWeaponIcons;
    [SerializeField] private Sprite[] supportItemIcons;

    // ���� ����, ���� ����, ����Ʈ ������ ������ ����
    private LitJson.JsonData mainWeaponData;
    private WeaponStatProvider.SubWeaponStat subWeaponStat;
    private int selectedItemIndex = -1;


    // Awake �޼��� �߰� - ���� ���� �����ϱ� ����
    private void Awake()
    {
        Debug.Log("[LevelUpUI] Awake ȣ���");
        // ���� ������Ʈ�� ������Ʈ�� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
        Debug.Log($"[LevelUpUI] ���� ������Ʈ Ȱ��ȭ ����: {gameObject.activeInHierarchy}");

        // Awake���� �̺�Ʈ ���� (���� ���� ����)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlayerLevelChanged += OnPlayerLevelUp;
            Debug.Log("[LevelUpUI] GameManager.onPlayerLevelChanged �̺�Ʈ ���� (Awake)");
        }
        else
        {
            Debug.LogError("[LevelUpUI] Awake���� GameManager.Instance�� null�Դϴ�!");
        }
    }
    private void OnEnable()
    {
        Debug.Log("[LevelUpUI] OnEnable ȣ���");
        // OnEnable������ �̺�Ʈ �籸��
        if (GameManager.Instance != null)
        {
            // �ߺ� ���� ������ ���� ���� ���� ����
            GameManager.Instance.onPlayerLevelChanged -= OnPlayerLevelUp;
            GameManager.Instance.onPlayerLevelChanged += OnPlayerLevelUp;
            Debug.Log("[LevelUpUI] GameManager.onPlayerLevelChanged �̺�Ʈ �籸�� (OnEnable)");
        }
    }

    private void Start()
    {
        Debug.Log("[LevelUpUI] Start ȣ���");

        // ������ �гΰ� ���� �г� �ʱ� ���� Ȯ��
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
            Debug.Log("[LevelUpUI] levelUpPanel �ʱ� ���� ����");
        }
        else
        {
            Debug.LogError("[LevelUpUI] levelUpPanel ������ �����ϴ�!");
        }

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
            Debug.Log("[LevelUpUI] selectionPanel �ʱ� ���� ����");
        }
        else
        {
            Debug.LogError("[LevelUpUI] selectionPanel ������ �����ϴ�!");
        }
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlayerLevelChanged -= OnPlayerLevelUp;
            Debug.Log("[LevelUpUI] GameManager.onPlayerLevelChanged �̺�Ʈ ���� ����");
        }
    }

    private void Update()
    {
        // ���� �÷��� �߿� ����ġ �� ������Ʈ
        if (GameManager.Instance != null && GameManager.Instance.gameState == GameManager.GameState.InGame)
        {
            UpdateExperienceBar();
        }
    }

    // ����ġ �� ������Ʈ �޼���
    private void UpdateExperienceBar()
    {
        // ����ġ �ٸ� PlayerExpBar ������Ʈ�� �����ϹǷ� ���⼭�� ���ʿ�
        // ���� �ؽ�Ʈ�� ������Ʈ
        if (levelText != null)
        {
            levelText.text = "Lv. " + GameManager.Instance.playerLevel;
        }
    }

    // ������ �̺�Ʈ �ڵ鷯
    private void OnPlayerLevelUp()
    {
        Debug.Log("[LevelUpUI] ������ ȣ���");

        GameManager.IsGamePaused = true;

        // ������ �г� ǥ��
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            Debug.Log("[LevelUpUI] levelUpPanel Ȱ��ȭ");
        }

        // ���� �г� ǥ��
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            Debug.Log("[LevelUpUI] selectionPanel Ȱ��ȭ");
            GenerateItemOptions();
        }
    }

    //private IEnumerator ShowLevelUpAnimation()
    //{
    //    Debug.Log("[LevelUpUI] ShowLevelUpAnimation ����");

    //    // ������ �ִϸ��̼� ��� (��: 1�� ���)
    //    yield return new WaitForSecondsRealtime(1f);

    //    // ���� ���� �г� ǥ��
    //    if (selectionPanel != null)
    //    {
    //        selectionPanel.SetActive(true);
    //        Debug.Log("[LevelUpUI] selectionPanel Ȱ��ȭ");

    //        // ���� ���� �ɼ� ����
    //        GenerateItemOptions();
    //    }
    //    else
    //    {
    //        Debug.LogError("[LevelUpUI] selectionPanel ������ �����ϴ�!");
    //    }
    //}

    // ������ ������ ����
    public void GenerateItemOptions()
    {
        Debug.Log("[LevelUpUI] GenerateItemOptions ����");

        // ������ ��ư �ʱ�ȭ
        ResetItemButtons();

        // ������ ��ư Ȱ��ȭ (�׽�Ʈ��)
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] != null)
            {
                itemButtons[i].SetActive(true);
                Debug.Log($"[LevelUpUI] ���� ��ư Ȱ��ȭ: {i}�� ��ư");
            }
        }

        // ������ ���� �Ŵ����� ���� ������ ����Ʈ ����
        if (LevelUpSelectManager.Instance != null)
        {
            LevelUpSelectManager.Instance.CreateItemList();
            Debug.Log("[LevelUpUI] LevelUpSelectManager.CreateItemList ȣ��");
        }
        else
        {
            Debug.LogError("[LevelUpUI] LevelUpSelectManager.Instance is null!");
        }

        // ��ư Ŭ�� ������ ����
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] != null)
            {
                int buttonIndex = i;
                Button button = itemButtons[i].GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SelectItem(buttonIndex));
                    Debug.Log($"[LevelUpUI] {i}�� ��ư ������ ����");
                }
            }
        }
    }

    // ������ ���� ó��
    public void SelectItem(int index)
    {
        Debug.Log($"[LevelUpUI] SelectItem({index}) ȣ���");
        selectedItemIndex = index;

        // ���õ� ������ ����
        ApplySelectedItem();

        // UI �ݱ�
        CloseSelectionUI();
    }

    // ���õ� ������ ����
    private void ApplySelectedItem()
    {
        Debug.Log($"[LevelUpUI] ApplySelectedItem ȣ���, ���� �ε���: {selectedItemIndex}");

        // ���� ���������� LevelUpSelectManager�� �޼��带 ȣ���Ͽ� ������ ����
        // �� �κ��� ������ ������ Ÿ�Կ� ���� �޶���
    }

    // ���� UI �ݱ�
    private void CloseSelectionUI()
    {
        Debug.Log("[LevelUpUI] CloseSelectionUI ȣ���");

        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // ���� �簳
        GameManager.IsGamePaused = false;  // �̰��� Time.timeScale�� 1�� ����
        Debug.Log($"[LevelUpUI] ���� �簳: IsGamePaused={GameManager.IsGamePaused}, TimeScale={Time.timeScale}");

        // ����ġ �� ������Ʈ
        UpdateExperienceBar();
    }

    // ������ ��ư �ʱ�ȭ �޼��� ����
    private void ResetItemButtons()
    {
        Debug.Log("[LevelUpUI] ResetItemButtons ����");

        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] != null)
            {
                // ������ �ʱ�ȭ
                if (itemIcons.Length > i && itemIcons[i] != null)
                {
                    itemIcons[i].sprite = null;
                }

                // �̸� �ʱ�ȭ
                if (itemNames.Length > i && itemNames[i] != null)
                {
                    itemNames[i].text = $"���� {i + 1}";
                }

                // ���� �ʱ�ȭ
                if (itemDescriptions.Length > i && itemDescriptions[i] != null)
                {
                    itemDescriptions[i].text = "���ο� ���⸦ �����ϼ���.";
                }

                // ������ ��ư Ȱ��ȭ
                itemButtons[i].SetActive(true);
            }
        }

        selectedItemIndex = -1;
    }

    // ���� ���� ������ UI ����
    public void SetMainWeaponItem(LitJson.JsonData weaponData, int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] SetMainWeaponItem ����, ��ư �ε���: {buttonIndex}");

        // ��ư �ε��� ��ȿ�� �˻�
        if (buttonIndex < 0 || buttonIndex >= itemButtons.Length)
        {
            Debug.LogError($"[LevelUpUI] ��ư �ε��� ���� �ʰ�: {buttonIndex}");
            return;
        }

        // itemButtons �� üũ
        if (itemButtons[buttonIndex] == null)
        {
            Debug.LogError($"[LevelUpUI] itemButtons[{buttonIndex}]��(��) null�Դϴ�!");
            return;
        }

        mainWeaponData = weaponData;

        Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)System.Enum.Parse(
            typeof(Weapon.MainWeapon.WeaponType),
            weaponData["weaponType"]["N"].ToString()
        );

        // ������ ��ư Ȱ��ȭ
        itemButtons[buttonIndex].SetActive(true);

        // ������ ���� (�ּ����� ó��)
        if (itemIcons.Length > buttonIndex && itemIcons[buttonIndex] != null)
        {
            itemIcons[buttonIndex].sprite = null; // ������ ��� ����
        }

        // �̸� ����
        if (itemNames.Length > buttonIndex && itemNames[buttonIndex] != null)
        {
            itemNames[buttonIndex].text = weaponType.ToString();
        }

        // ���� ����
        if (itemDescriptions.Length > buttonIndex && itemDescriptions[buttonIndex] != null)
        {
            itemDescriptions[buttonIndex].text = "���ο� �ֹ���";
        }

        // ��ư Ŭ�� ������ ����
        Button button = itemButtons[buttonIndex].GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                Debug.Log("[LevelUpUI] �ֹ��� ��ư Ŭ����");
                if (LevelUpSelectManager.Instance != null)
                {
                    LevelUpSelectManager.Instance.CreateMainWeaponItem(weaponData);
                }
                CloseSelectionUI();
            });
        }
        else
        {
            Debug.LogWarning("[LevelUpUI] ��ư ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    // ���� ���� ������ UI ����
    public void SetSubWeaponItem(WeaponStatProvider.SubWeaponStat stat, int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] SetSubWeaponItem ȣ���, ��ư �ε���: {buttonIndex}");

        if (buttonIndex < 0 || buttonIndex >= itemButtons.Length) return;

        subWeaponStat = stat;
        itemButtons[buttonIndex].SetActive(true);

        // ���� Ÿ�Կ� ���� ������ ����
        int iconIndex = (int)stat.weaponType;
        if (iconIndex < subWeaponIcons.Length)
        {
            itemIcons[buttonIndex].sprite = subWeaponIcons[iconIndex];
        }

        itemNames[buttonIndex].text = stat.weaponType.ToString();

        // �� �������� ���׷��̵������� ���� ���� ����
        if (stat.weaponGrade == 0)
        {
            itemDescriptions[buttonIndex].text = "���ο� ���� ���⸦ �߰��մϴ�.";
        }
        else
        {
            itemDescriptions[buttonIndex].text = $"���� ���⸦ ���� {stat.weaponGrade}�� ���׷��̵��մϴ�.";
        }

        // ��ư Ŭ�� �̺�Ʈ ������Ʈ
        Button button = itemButtons[buttonIndex].GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            Debug.Log("[LevelUpUI] ���� ���� ��ư Ŭ����");
            if (LevelUpSelectManager.Instance != null)
            {
                LevelUpSelectManager.Instance.CreateSubWeaponItem(stat);
            }
            CloseSelectionUI();
        });
    }

    // ����׿� �޼��� - �ν����Ϳ��� �׽�Ʈ������ ȣ�� ����
    public void TestShowLevelUpPanel()
    {
        Debug.Log("[LevelUpUI] TestShowLevelUpPanel ȣ���");
        OnPlayerLevelUp();
    }
}