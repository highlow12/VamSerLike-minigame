using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugTools : MonoBehaviour
{
    [Header("����� ��ư")]
    [SerializeField] private Button addXpButton;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button forceShowUIButton;
    [SerializeField] private Button updateLevelTextButton;

    [Header("����� ����")]
    [SerializeField] private TextMeshProUGUI debugInfoText;

    [Header("����")]
    [SerializeField] private float xpToAdd = 50f;

    private void Start()
    {
        if (addXpButton != null)
            addXpButton.onClick.AddListener(AddXp);

        if (levelUpButton != null)
            levelUpButton.onClick.AddListener(ForceLevel);

        if (forceShowUIButton != null)
            forceShowUIButton.onClick.AddListener(ForceShowUI);

        if (updateLevelTextButton != null)
            updateLevelTextButton.onClick.AddListener(UpdateLevelText);

        InvokeRepeating("UpdateDebugInfo", 0f, 0.5f);
    }

    private void AddXp()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddExperience(xpToAdd);
            Debug.Log($"[Debug] ����ġ {xpToAdd} �߰�, ����: {GameManager.Instance.playerExperience}/{GameManager.Instance.experienceToLevelUp}");
        }
    }

    private void ForceLevel()
    {
        if (GameManager.Instance != null)
        {
            float needed = GameManager.Instance.experienceToLevelUp - GameManager.Instance.playerExperience + 1;
            GameManager.Instance.AddExperience(needed);
            Debug.Log($"[Debug] ���� ������, ���� ����: {GameManager.Instance.playerLevel}");
        }
    }

    private void ForceShowUI()
    {
        // FindObjectOfType ��� FindAnyObjectByType ���
        LevelUpUI levelUpUI = FindAnyObjectByType<LevelUpUI>();
        if (levelUpUI != null)
        {
            levelUpUI.TestShowLevelUpPanel();
            Debug.Log("[Debug] ������ ������ UI ǥ�� �õ�");
        }
        else
        {
            Debug.LogError("[Debug] LevelUpUI�� ã�� �� �����ϴ�!");
        }
    }

    private void UpdateLevelText()
    {
        // Ŭ���� �̸��� ExperienceBar���� PlayerExpBar�� �����
        PlayerExpBar expBar = FindAnyObjectByType<PlayerExpBar>();
        if (expBar != null)
        {
            expBar.UpdateUI();
            Debug.Log("[Debug] ����ġ �� UI ������Ʈ ��û");
        }
        else
        {
            Debug.LogError("[Debug] PlayerExpBar�� ã�� �� �����ϴ�!");
        }
    }

    private void UpdateDebugInfo()
    {
        if (debugInfoText != null && GameManager.Instance != null)
        {
            debugInfoText.text = string.Format(
                "����: {0}\n" +
                "����ġ: {1}/{2}\n" +
                "�Ͻ�����: {3}\n" +
                "TimeScale: {4}",
                GameManager.Instance.playerLevel,
                (int)GameManager.Instance.playerExperience,
                (int)GameManager.Instance.experienceToLevelUp,
                GameManager.IsGamePaused,
                Time.timeScale
            );
        }
    }
}