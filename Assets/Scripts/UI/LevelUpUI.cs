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
            Debug.Log("[LevelUpUI] AudioSource 컴포넌트 추가됨");
        }
    }

    private void Start()
    {
        weaponsInventoryUI = FindFirstObjectByType<WeaponsInventoryUI>();
        if (weaponsInventoryUI == null)
        {
            Debug.LogWarning("[LevelUpUI] WeaponsInventoryUI를 찾을 수 없습니다!");
        }
    }

    private void OnDestroy()
    {
        onPanelClosed = delegate { };
    }

    /// <summary>
    /// 버튼 클릭 이벤트 초기 설정
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
        Debug.Log("[LevelUpUI] ShowLevelUpPanel 메서드 호출됨");

        if (levelUpPanel != null && levelUpPanel.activeSelf)
        {
            Debug.Log("[LevelUpUI] 패널이 이미 활성화되어 있습니다. 중복 호출 무시.");
            return;
        }

        if (levelUpPanel == null)
        {
            Debug.LogError("[LevelUpUI] levelUpPanel이 null입니다!");
            return;
        }

        GameManager.IsGamePaused = true;

        if (levelText != null && GameManager.Instance != null)
        {
            levelText.text = $"Level {GameManager.Instance.playerLevel}";
            Debug.Log($"[LevelUpUI] 레벨 텍스트 업데이트: {levelText.text}");
        }

        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        ClearOptionButtons();

        levelUpPanel.SetActive(true);
        Debug.Log("[LevelUpUI] 레벨업 패널 활성화됨");
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
                Debug.LogWarning($"[LevelUpUI] 서브 무기 displayName이 비어 있음: {subWeaponStat.weaponType}");
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
                    Debug.Log($"[LevelUpUI] 무기 선택됨: {subWeaponStat.weaponType} (등급 {subWeaponStat.weaponGrade})");
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

    public void SetSupportItem(SupportItemSO supportItem, int buttonIndex, string displayName, Sprite icon, int currentLevel)
    {
        if (buttonIndex >= optionButtons.Count || supportItem == null)
            return;

        if (optionButtons[buttonIndex] != null)
        {
            optionButtons[buttonIndex].gameObject.SetActive(true);
            optionButtons[buttonIndex].onClick.RemoveAllListeners();
            optionButtons[buttonIndex].onClick.AddListener(() => {
                if (LevelUpSelectManager.Instance != null)
                {
                    LevelUpSelectManager.Instance.CreateSupportItem(supportItem);
                    ClosePanel();
                }
            });
        }

        if (buttonIndex < optionIcons.Count && optionIcons[buttonIndex] != null)
        {
            optionIcons[buttonIndex].sprite = icon;
            optionIcons[buttonIndex].gameObject.SetActive(icon != null);
        }

        if (buttonIndex < optionTexts.Count && optionTexts[buttonIndex] != null)
        {
            optionTexts[buttonIndex].text = $"{displayName} (Lv.{currentLevel + 1})";
            optionTexts[buttonIndex].gameObject.SetActive(true);
        }
    }

    private void OnOptionButtonClicked(int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] Option button {buttonIndex} clicked");
    }

    public void ClosePanel()
    {
        Debug.Log("[LevelUpUI] ClosePanel 호출됨");

        if (lastSelectedWeapon.weaponType != 0 && weaponsInventoryUI != null)
        {
            string weaponId = $"Sub_{lastSelectedWeapon.weaponType}";
            Debug.Log($"[LevelUpUI] 선택된 무기: {lastSelectedWeapon.weaponType}, ID: {weaponId}, 등급: {lastSelectedWeapon.weaponGrade}");
            weaponsInventoryUI.IncrementWeaponSelectionCount(weaponId);
            Debug.Log($"[LevelUpUI] 무기 {weaponId}의 레벨이 증가됨");
            lastSelectedWeapon = new WeaponStatProvider.SubWeaponStat();
        }
        else
        {
            Debug.Log("[LevelUpUI] 선택된 무기가 없거나 weaponsInventoryUI가 null입니다.");
        }

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        onPanelClosed?.Invoke();
        Debug.Log("[LevelUpUI] onPanelClosed 이벤트 발생");

        GameManager.IsGamePaused = false;
    }

    private string GetGradeText(int grade)
    {
        switch (grade)
        {
            case 0: return "신규";
            case 1: return "레벨 1";
            case 2: return "레벨 2";
            case 3: return "레벨 3";
            case 4: return "레벨 4";
            case 5: return "레벨 5";
            default: return $"레벨 {grade}";
        }
    }
}
