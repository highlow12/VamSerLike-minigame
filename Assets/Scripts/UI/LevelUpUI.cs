using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitJson;

public class LevelUpUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] public List<Button> optionButtons = new List<Button>();
    [SerializeField] public List<Image> optionIcons = new List<Image>();
    [SerializeField] public List<TextMeshProUGUI> optionTexts = new List<TextMeshProUGUI>();

    [Header("Settings")]
    [SerializeField] private float panelShowDuration = 0.5f;
    [SerializeField] private AudioClip levelUpSound;

    private AudioSource audioSource;
    private WeaponsInventoryUI weaponsInventoryUI;
    private WeaponStatProvider.SubWeaponStat lastSelectedWeapon;

    public event Action onPanelClosed = delegate { };

    private void Awake()
    {
        InitializeAudioSource();

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[LevelUpUI] levelUpPanel is null!");
        }

        SetupButtonEvents();
    }

    private void InitializeAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[LevelUpUI] AudioSource ������Ʈ �߰���");
        }
    }

    private void Start()
    {
        weaponsInventoryUI = FindFirstObjectByType<WeaponsInventoryUI>();
        if (weaponsInventoryUI == null)
        {
            Debug.LogWarning("[LevelUpUI] WeaponsInventoryUI�� ã�� �� �����ϴ�!");
        }
    }

    private void OnDestroy()
    {
        onPanelClosed = delegate { };
    }

    /// <summary>
    /// ��ư Ŭ�� �̺�Ʈ �ʱ� ����
    /// </summary>
    private void SetupButtonEvents()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                int index = i;
                optionButtons[i].onClick.AddListener(() => OnOptionButtonClicked(index));
            }
            else
            {
                Debug.LogWarning($"[LevelUpUI] Option button at index {i} is null!");
            }
        }
    }

    public void ShowLevelUpPanel()
    {
        Debug.Log("[LevelUpUI] ShowLevelUpPanel �޼��� ȣ���");

        if (levelUpPanel != null && levelUpPanel.activeSelf)
        {
            Debug.Log("[LevelUpUI] �г��� �̹� Ȱ��ȭ�Ǿ� �ֽ��ϴ�. �ߺ� ȣ�� ����.");
            return;
        }

        if (levelUpPanel == null)
        {
            Debug.LogError("[LevelUpUI] levelUpPanel�� null�Դϴ�!");
            return;
        }

        GameManager.IsGamePaused = true;

        if (levelText != null && GameManager.Instance != null)
        {
            levelText.text = $"Level {GameManager.Instance.playerLevel}";
            Debug.Log($"[LevelUpUI] ���� �ؽ�Ʈ ������Ʈ: {levelText.text}");
        }

        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        ClearOptionButtons();

        levelUpPanel.SetActive(true);
        Debug.Log("[LevelUpUI] ������ �г� Ȱ��ȭ��");
    }

    private void ClearOptionButtons()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                optionButtons[i].gameObject.SetActive(false);
                optionButtons[i].onClick.RemoveAllListeners();
                int index = i;
                optionButtons[i].onClick.AddListener(() => OnOptionButtonClicked(index));
            }

            if (i < optionIcons.Count && optionIcons[i] != null)
            {
                optionIcons[i].sprite = null;
                optionIcons[i].gameObject.SetActive(false);
            }

            if (i < optionTexts.Count && optionTexts[i] != null)
            {
                optionTexts[i].text = string.Empty;
                optionTexts[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetSubWeaponItem(WeaponStatProvider.SubWeaponStat subWeaponStat, int buttonIndex, string displayName = null, Sprite icon = null)
    {
        if (buttonIndex >= optionButtons.Count || string.IsNullOrEmpty(subWeaponStat.displayName))
        {
            Debug.LogError($"[LevelUpUI] Invalid button index or subWeaponStat: {buttonIndex}");
            return;
        }

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = subWeaponStat.displayName;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = subWeaponStat.weaponType.ToString();
                Debug.LogWarning($"[LevelUpUI] ���� ���� displayName�� ��� ����: {subWeaponStat.weaponType}");
            }
        }

        if (icon == null && LevelUpSelectManager.Instance != null)
        {
            icon = LevelUpSelectManager.Instance.LoadSubWeaponSprite(subWeaponStat);
        }

        if (optionButtons[buttonIndex] != null)
        {
            optionButtons[buttonIndex].gameObject.SetActive(true);

            optionButtons[buttonIndex].onClick.RemoveAllListeners();
            optionButtons[buttonIndex].onClick.AddListener(() => {
                if (LevelUpSelectManager.Instance != null)
                {
                    lastSelectedWeapon = subWeaponStat;
                    Debug.Log($"[LevelUpUI] ���� ���õ�: {subWeaponStat.weaponType} (��� {subWeaponStat.weaponGrade})");
                    LevelUpSelectManager.Instance.CreateSubWeaponItem(subWeaponStat);
                    ClosePanel();
                }
                else
                {
                    Debug.LogError("[LevelUpUI] LevelUpSelectManager.Instance is null!");
                }
            });

            if (buttonIndex < optionIcons.Count && optionIcons[buttonIndex] != null)
            {
                optionIcons[buttonIndex].sprite = icon;
                optionIcons[buttonIndex].gameObject.SetActive(icon != null);
            }

            if (buttonIndex < optionTexts.Count && optionTexts[buttonIndex] != null)
            {
                string gradeText = GetGradeText(subWeaponStat.weaponGrade);
                optionTexts[buttonIndex].text = $"{displayName} ({gradeText})";
                optionTexts[buttonIndex].gameObject.SetActive(true);
            }
        }
    }

    private void OnOptionButtonClicked(int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] Option button {buttonIndex} clicked");
    }

    public void ClosePanel()
    {
        Debug.Log("[LevelUpUI] ClosePanel ȣ���");

        if (lastSelectedWeapon.weaponType != 0 && weaponsInventoryUI != null)
        {
            string weaponId = $"Sub_{lastSelectedWeapon.weaponType}";
            Debug.Log($"[LevelUpUI] ���õ� ����: {lastSelectedWeapon.weaponType}, ID: {weaponId}, ���: {lastSelectedWeapon.weaponGrade}");
            weaponsInventoryUI.IncrementWeaponSelectionCount(weaponId);
            Debug.Log($"[LevelUpUI] ���� {weaponId}�� ������ ������");
            lastSelectedWeapon = new WeaponStatProvider.SubWeaponStat();
        }
        else
        {
            Debug.Log("[LevelUpUI] ���õ� ���Ⱑ ���ų� weaponsInventoryUI�� null�Դϴ�.");
        }

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        onPanelClosed?.Invoke();
        Debug.Log("[LevelUpUI] onPanelClosed �̺�Ʈ �߻�");

        GameManager.IsGamePaused = false;
    }

    private string GetGradeText(int grade)
    {
        switch (grade)
        {
            case 0: return "�ű�";
            case 1: return "���� 1";
            case 2: return "���� 2";
            case 3: return "���� 3";
            case 4: return "���� 4";
            case 5: return "���� 5";
            default: return $"���� {grade}";
        }
    }
}
