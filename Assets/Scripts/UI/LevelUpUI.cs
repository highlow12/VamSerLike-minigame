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

    [Header("Settings")]
    [SerializeField] private float panelShowDuration = 0.5f;
    [SerializeField] private AudioClip levelUpSound;

    private AudioSource audioSource;

    // �г� ���� �̺�Ʈ �߰�
    public event Action onPanelClosed = delegate { };

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ������ �г� �ʱ� ��Ȱ��ȭ
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // ��ư �̺�Ʈ ����
        SetupButtonEvents();
    }

    private void Start()
    {
        // ������ �̺�Ʈ ����
        if (GameManager.Instance != null)
        {
            Debug.Log("[LevelUpUI] GameManager.Instance �̺�Ʈ�� �����մϴ�.");
            GameManager.Instance.onPlayerLevelChanged += ShowLevelUpPanel;
        }
        else
        {
            Debug.LogError("[LevelUpUI] GameManager.Instance�� null�Դϴ�!");
        }
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlayerLevelChanged -= ShowLevelUpPanel;
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

        // ���� �Ͻ� ����
        GameManager.IsGamePaused = true;

        // ���� �ؽ�Ʈ ������Ʈ
        if (levelText != null)
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
            }

            if (i < optionIcons.Count && optionIcons[i] != null)
            {
                optionIcons[i].sprite = null;
            }

            if (i < optionTexts.Count && optionTexts[i] != null)
            {
                optionTexts[i].text = string.Empty;
            }
        }
    }

    // ���� ���� �ɼ� ����
    public void SetMainWeaponItem(JsonData userData, int buttonIndex, string displayName = null, Sprite icon = null)
    {
        if (buttonIndex >= optionButtons.Count || userData == null)
        {
            Debug.LogError($"[LevelUpUI] Invalid button index or userData: {buttonIndex}");
            return;
        }

        // ���� Ÿ�� �Ľ�
        Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(
            typeof(Weapon.MainWeapon.WeaponType),
            userData["weaponType"].ToString()
        );

        // ���� �̸��� �������� ���� ��� ��������
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = WeaponStatProvider.Instance.weaponStats.Find(x => x.weaponType == weaponType).displayName;
        }

        // ���� �������� �������� ���� ��� ��������
        if (icon == null)
        {
            GameObject weaponPrefab = Resources.Load<GameObject>($"Prefabs/Player/Weapon/MainWeapon/{weaponType}");
            if (weaponPrefab != null && weaponPrefab.GetComponent<SpriteRenderer>() != null)
            {
                icon = weaponPrefab.GetComponent<SpriteRenderer>().sprite;
            }
        }

        // ��ư ����
        if (optionButtons[buttonIndex] != null)
        {
            optionButtons[buttonIndex].gameObject.SetActive(true);

            // ������ ĳ�� (��ư Ŭ�� �� ���)
            optionButtons[buttonIndex].onClick.RemoveAllListeners();
            optionButtons[buttonIndex].onClick.AddListener(() => {
                LevelUpSelectManager.Instance.CreateMainWeaponItem(userData);
                ClosePanel();
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
                optionTexts[buttonIndex].text = displayName;
            }
        }
    }

    // ���� ���� �ɼ� ����
    public void SetSubWeaponItem(WeaponStatProvider.SubWeaponStat subWeaponStat, int buttonIndex, string displayName = null, Sprite icon = null)
    {
        if (buttonIndex >= optionButtons.Count || Equals(subWeaponStat, default))
        {
            Debug.LogError($"[LevelUpUI] Invalid button index or subWeaponStat: {buttonIndex}");
            return;
        }

        // ���� �̸��� �������� ���� ��� ��������
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = subWeaponStat.displayName;
        }

        // ���� �������� �������� ���� ��� ��������
        if (icon == null)
        {
            SubWeaponSO subWeaponSO = Resources.Load<SubWeaponSO>($"ScriptableObjects/SubWeapon/{displayName}/{displayName}_{subWeaponStat.weaponGrade}");
            if (subWeaponSO != null)
            {
                icon = subWeaponSO.weaponSprite;
            }
        }

        // ��ư ����
        if (optionButtons[buttonIndex] != null)
        {
            optionButtons[buttonIndex].gameObject.SetActive(true);

            // ������ ĳ�� (��ư Ŭ�� �� ���)
            optionButtons[buttonIndex].onClick.RemoveAllListeners();
            optionButtons[buttonIndex].onClick.AddListener(() => {
                LevelUpSelectManager.Instance.CreateSubWeaponItem(subWeaponStat);
                ClosePanel();
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
            }
        }
    }

    // ��ư Ŭ�� ó��
    private void OnOptionButtonClicked(int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] Option button {buttonIndex} clicked");
        // �̹� ��ư�� �����ʸ� ���������Ƿ� �߰� ó���� �ʿ� ����
    }

    // �г� �ݱ�
    public void ClosePanel()
    {
        Debug.Log("[LevelUpUI] ClosePanel ȣ���");

        // �г� ��Ȱ��ȭ (���� ����)
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // �̺�Ʈ �߻�
        onPanelClosed?.Invoke();
        Debug.Log("[LevelUpUI] onPanelClosed �̺�Ʈ �߻�");

        // GameManager�� �˸� (�������� ����)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelUpPanelClosed();
        }
        else
        {
            Debug.LogError("[LevelUpUI] GameManager.Instance�� null�Դϴ�!");
        }

        // ���� �簳 (���� �������� ����)
        GameManager.IsGamePaused = false;
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