using System;
using System.Collections;
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

    [Header("���� �ε������� ����")]
    [SerializeField] private List<Transform> levelIndicatorContainers = new List<Transform>();
    [SerializeField] private GameObject indicatorPrefab; // ���� �ε������� ������
    [SerializeField] private Color indicatorEmptyColor = Color.gray;
    [SerializeField] private Color indicatorFilledColor = Color.yellow;
    [SerializeField] private Color indicatorBlinkColor = Color.white;
    [SerializeField] private float blinkInterval = 0.5f; // �����̴� ���� (��)

    [Header("Settings")]
    [SerializeField] private int maxSkillLevel = 5; // �ִ� ���� ����
    [SerializeField] private int maxSkillCount = 6; // ���� ������ �ִ� ���� ����
    [SerializeField] private float panelShowDuration = 0.5f;
    [SerializeField] private AudioClip levelUpSound;

    private AudioSource audioSource;
    private List<Coroutine> blinkCoroutines = new List<Coroutine>();
    private WeaponsInventoryUI weaponsInventoryUI;
    private WeaponStatProvider.SubWeaponStat lastSelectedWeapon;

    // ���� �ε������� ĳ�ÿ� �迭
    private List<List<Image>> levelIndicators = new List<List<Image>>();

    // �г� ���� �̺�Ʈ �߰�
    public event Action onPanelClosed = delegate { };

    private void Awake()
    {
        // ����� �ҽ� ������Ʈ Ȯ��
        InitializeAudioSource();

        // ������ �г� �ʱ� ��Ȱ��ȭ
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[LevelUpUI] levelUpPanel is null!");
        }

        // ��ư �̺�Ʈ ����
        SetupButtonEvents();

        // ���� �ε������� �ʱ�ȭ
        InitializeLevelIndicators();
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
        // WeaponsInventoryUI ���� ã��
        weaponsInventoryUI = FindFirstObjectByType<WeaponsInventoryUI>();
        if (weaponsInventoryUI == null)
        {
            Debug.LogWarning("[LevelUpUI] WeaponsInventoryUI�� ã�� �� �����ϴ�!");
        }
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ����
        onPanelClosed = delegate { };

        // ������ �ڷ�ƾ ����
        StopAllBlinkCoroutines();
    }

    // ���� �ε������� �ʱ�ȭ
    private void InitializeLevelIndicators()
    {
        levelIndicators.Clear();

        // �� ���� �ɼǿ� ���� �ε������� �����̳� Ȯ��
        for (int i = 0; i < levelIndicatorContainers.Count; i++)
        {
            List<Image> indicators = new List<Image>();

            if (levelIndicatorContainers[i] != null)
            {
                // �̹� �����ϴ� �ε������� ���
                foreach (Transform child in levelIndicatorContainers[i])
                {
                    Image indicator = child.GetComponent<Image>();
                    if (indicator != null)
                    {
                        indicators.Add(indicator);
                        indicator.color = indicatorEmptyColor;
                    }
                }

                // �ε������Ͱ� ���ų� �����ϸ� ���������� ����
                if (indicators.Count < maxSkillLevel && indicatorPrefab != null)
                {
                    int toCreate = maxSkillLevel - indicators.Count;
                    for (int j = 0; j < toCreate; j++)
                    {
                        GameObject newIndicator = Instantiate(indicatorPrefab, levelIndicatorContainers[i]);
                        Image indicatorImage = newIndicator.GetComponent<Image>();
                        if (indicatorImage != null)
                        {
                            indicators.Add(indicatorImage);
                            indicatorImage.color = indicatorEmptyColor;
                        }
                    }
                }
            }

            levelIndicators.Add(indicators);
        }
    }

    // ��ư �̺�Ʈ ����
    private void SetupButtonEvents()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                int buttonIndex = i; // Ŭ������ ���� ���� ����
                optionButtons[i].onClick.AddListener(() => OnOptionButtonClicked(buttonIndex));
            }
            else
            {
                Debug.LogWarning($"[LevelUpUI] Option button at index {i} is null!");
            }
        }
    }

    // ������ �г� ǥ��
    public void ShowLevelUpPanel()
    {
        Debug.Log("[LevelUpUI] ShowLevelUpPanel �޼��� ȣ���");

        // �̹� �г��� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��
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

        // ��� ������ �ڷ�ƾ ����
        StopAllBlinkCoroutines();

        // ���� �Ͻ� ����
        GameManager.IsGamePaused = true;

        // ���� �ؽ�Ʈ ������Ʈ
        if (levelText != null && GameManager.Instance != null)
        {
            levelText.text = $"Level {GameManager.Instance.playerLevel}";
            Debug.Log($"[LevelUpUI] ���� �ؽ�Ʈ ������Ʈ: {levelText.text}");
        }

        // ������ ȿ���� ���
        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        // �ɼ� �ʱ�ȭ
        ClearOptionButtons();

        // �г� Ȱ��ȭ
        levelUpPanel.SetActive(true);
        Debug.Log("[LevelUpUI] ������ �г� Ȱ��ȭ��");
    }

    // ��� �ɼ� ��ư �ʱ�ȭ
    private void ClearOptionButtons()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                optionButtons[i].gameObject.SetActive(false);
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionButtonClicked(i));
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

            // ���� �ε������� �ʱ�ȭ
            if (i < levelIndicators.Count)
            {
                foreach (var indicator in levelIndicators[i])
                {
                    indicator.color = indicatorEmptyColor;
                    indicator.gameObject.SetActive(false);
                }
            }
        }
    }

    // ���� ���� �ɼ� ����
    public void SetSubWeaponItem(WeaponStatProvider.SubWeaponStat subWeaponStat, int buttonIndex, string displayName = null, Sprite icon = null)
    {
        if (buttonIndex >= optionButtons.Count || string.IsNullOrEmpty(subWeaponStat.displayName))
        {
            Debug.LogError($"[LevelUpUI] Invalid button index or subWeaponStat: {buttonIndex}");
            return;
        }

        // ���� �̸��� �������� ���� ��� ��������
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = subWeaponStat.displayName;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = subWeaponStat.weaponType.ToString();
                Debug.LogWarning($"[LevelUpUI] ���� ���� displayName�� ��� ����: {subWeaponStat.weaponType}");
            }
        }

        // �������� null�̰� LevelUpSelectManager�� ���� �ε� �õ�
        if (icon == null && LevelUpSelectManager.Instance != null)
        {
            icon = LevelUpSelectManager.Instance.LoadSubWeaponSprite(subWeaponStat);
        }

        // ��ư ����
        if (optionButtons[buttonIndex] != null)
        {
            optionButtons[buttonIndex].gameObject.SetActive(true);

            // ������ ĳ�� (��ư Ŭ�� �� ���)
            optionButtons[buttonIndex].onClick.RemoveAllListeners();
            optionButtons[buttonIndex].onClick.AddListener(() => {
                if (LevelUpSelectManager.Instance != null)
                {
                    // ���õ� ���� ����
                    lastSelectedWeapon = subWeaponStat;

                    // ����� �α� �߰�
                    Debug.Log($"[LevelUpUI] ���� ���õ�: {subWeaponStat.weaponType} (��� {subWeaponStat.weaponGrade})");

                    // ���� ���� �� �г� �ݱ�
                    LevelUpSelectManager.Instance.CreateSubWeaponItem(subWeaponStat);
                    ClosePanel();
                }
                else
                {
                    Debug.LogError("[LevelUpUI] LevelUpSelectManager.Instance is null!");
                }
            });

            // ������ ����
            if (buttonIndex < optionIcons.Count && optionIcons[buttonIndex] != null)
            {
                optionIcons[buttonIndex].sprite = icon;
                optionIcons[buttonIndex].gameObject.SetActive(icon != null);
            }

            // �ؽ�Ʈ ����
            if (buttonIndex < optionTexts.Count && optionTexts[buttonIndex] != null)
            {
                // ��޿� ���� �ؽ�Ʈ ������
                string gradeText = GetGradeText(subWeaponStat.weaponGrade);
                optionTexts[buttonIndex].text = $"{displayName} ({gradeText})";
                optionTexts[buttonIndex].gameObject.SetActive(true);
            }

            // ���� �ε������� ������Ʈ
            UpdateLevelIndicators(subWeaponStat, buttonIndex);
        }
    }

    // ���� �ε������� ������Ʈ
    private void UpdateLevelIndicators(WeaponStatProvider.SubWeaponStat subWeaponStat, int buttonIndex)
    {
        if (buttonIndex >= levelIndicators.Count) return;

        // ���� ID ���� (Ÿ�Ը����� ID �����Ͽ� ���� Ÿ���� ���� ����)
        string weaponId = $"Sub_{subWeaponStat.weaponType}";

        // ���� ���� ���� ��������
        int currentLevel = GetWeaponLevel(weaponId);

        // �ε������� Ȱ��ȭ �� ����
        for (int i = 0; i < levelIndicators[buttonIndex].Count; i++)
        {
            Image indicator = levelIndicators[buttonIndex][i];
            indicator.gameObject.SetActive(true);

            if (i < currentLevel)
            {
                // �̹� ȹ���� ����
                indicator.color = indicatorFilledColor;
            }
            else if (i == currentLevel && currentLevel < maxSkillLevel)
            {
                // ������ ȹ���� ���� (������)
                indicator.color = indicatorEmptyColor;
                Coroutine blinkCoroutine = StartCoroutine(BlinkIndicator(indicator));
                blinkCoroutines.Add(blinkCoroutine);
            }
            else
            {
                // ���� ȹ������ ���� ����
                indicator.color = indicatorEmptyColor;
            }
        }
    }

    // ��ư Ŭ�� ó��
    private void OnOptionButtonClicked(int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] Option button {buttonIndex} clicked");
        // �̹� ��ư�� �����ʸ� ���������Ƿ� �߰� ó�� �ʿ� ����
    }

    // �г� �ݱ�
    public void ClosePanel()
    {
        Debug.Log("[LevelUpUI] ClosePanel ȣ���");

        // ���õ� ���Ⱑ ������ �κ��丮 ������Ʈ
        if (lastSelectedWeapon.weaponType != 0 && weaponsInventoryUI != null)
        {
            // ���� ���� ���� ���� (���� Ÿ�Ը����� ID ����)
            string weaponId = $"Sub_{lastSelectedWeapon.weaponType}";

            // ����� ���� �߰�
            Debug.Log($"[LevelUpUI] ���õ� ����: {lastSelectedWeapon.weaponType}, ID: {weaponId}, ���: {lastSelectedWeapon.weaponGrade}");

            // ���� ���� ����
            weaponsInventoryUI.IncrementWeaponSelectionCount(weaponId);
            Debug.Log($"[LevelUpUI] ���� {weaponId}�� ������ ������");

            // ���õ� ���� �ʱ�ȭ
            lastSelectedWeapon = new WeaponStatProvider.SubWeaponStat();
        }
        else
        {
            Debug.Log("[LevelUpUI] ���õ� ���Ⱑ ���ų� weaponsInventoryUI�� null�Դϴ�.");
        }

        // �г� ��Ȱ��ȭ
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // ������ �ڷ�ƾ ����
        StopAllBlinkCoroutines();

        // �̺�Ʈ �߻�
        onPanelClosed?.Invoke();
        Debug.Log("[LevelUpUI] onPanelClosed �̺�Ʈ �߻�");

        // ���� �簳
        GameManager.IsGamePaused = false;
    }

    // ��� ������ �ڷ�ƾ ����
    private void StopAllBlinkCoroutines()
    {
        foreach (Coroutine coroutine in blinkCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        blinkCoroutines.Clear();
    }

    // �ε������� ������ ȿ��
    private IEnumerator BlinkIndicator(Image indicator)
    {
        bool isBlinking = false;

        while (true)
        {
            isBlinking = !isBlinking;
            indicator.color = isBlinking ? indicatorBlinkColor : indicatorEmptyColor;

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // ���� ���� ��������
    private int GetWeaponLevel(string weaponId)
    {
        if (weaponsInventoryUI != null)
        {
            try
            {
                // ���÷������� private �޼��� ȣ�� (�ʿ��)
                var selectionCountsField = weaponsInventoryUI.GetType().GetField("weaponSelectionCount",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (selectionCountsField != null)
                {
                    var selectionCounts = selectionCountsField.GetValue(weaponsInventoryUI) as Dictionary<string, int>;
                    if (selectionCounts != null && selectionCounts.ContainsKey(weaponId))
                    {
                        int level = selectionCounts[weaponId];
                        Debug.Log($"[LevelUpUI] ���� {weaponId}�� ���� ����: {level}");
                        return level;
                    }
                    else
                    {
                        Debug.Log($"[LevelUpUI] ���� {weaponId}�� ���� ������ �����ϴ� (0 ��ȯ)");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LevelUpUI] weaponSelectionCount ���� �� ����: {e.Message}");
            }
        }

        return 0;
    }

    // ���� ��� �ؽ�Ʈ ��ȯ
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