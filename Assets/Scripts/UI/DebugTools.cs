using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugTools : MonoBehaviour
{
    [Header("디버그 버튼")]
    [SerializeField] private Button addXpButton;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button forceShowUIButton;
    [SerializeField] private Button updateLevelTextButton;

    [Header("디버그 정보")]
    [SerializeField] private TextMeshProUGUI debugInfoText;

    [Header("설정")]
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
            Debug.Log($"[Debug] 경험치 {xpToAdd} 추가, 현재: {GameManager.Instance.playerExperience}/{GameManager.Instance.experienceToLevelUp}");
        }
    }

    private void ForceLevel()
    {
        if (GameManager.Instance != null)
        {
            float needed = GameManager.Instance.experienceToLevelUp - GameManager.Instance.playerExperience + 1;
            GameManager.Instance.AddExperience(needed);
            Debug.Log($"[Debug] 강제 레벨업, 현재 레벨: {GameManager.Instance.playerLevel}");
        }
    }

    private void ForceShowUI()
    {
        // FindObjectOfType 대신 FindAnyObjectByType 사용
        LevelUpUI levelUpUI = FindAnyObjectByType<LevelUpUI>();
        if (levelUpUI != null)
        {
            levelUpUI.TestShowLevelUpPanel();
            Debug.Log("[Debug] 강제로 레벨업 UI 표시 시도");
        }
        else
        {
            Debug.LogError("[Debug] LevelUpUI를 찾을 수 없습니다!");
        }
    }

    private void UpdateLevelText()
    {
        // 클래스 이름이 ExperienceBar에서 PlayerExpBar로 변경됨
        PlayerExpBar expBar = FindAnyObjectByType<PlayerExpBar>();
        if (expBar != null)
        {
            expBar.UpdateUI();
            Debug.Log("[Debug] 경험치 바 UI 업데이트 요청");
        }
        else
        {
            Debug.LogError("[Debug] PlayerExpBar를 찾을 수 없습니다!");
        }
    }

    private void UpdateDebugInfo()
    {
        if (debugInfoText != null && GameManager.Instance != null)
        {
            debugInfoText.text = string.Format(
                "레벨: {0}\n" +
                "경험치: {1}/{2}\n" +
                "일시정지: {3}\n" +
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