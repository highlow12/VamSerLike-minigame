using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ExperienceBar : MonoBehaviour
{
    public Slider experienceSlider;
    public TextMeshProUGUI levelText;
    Coroutine slideCoroutine;
    public float slideSpeed = 5f;
    private long playerLevel = 1;
    private float currentXP = 0;
    private float xpToNextLevel = 100;

    void Start()
    {
        // GameManager의 레벨 변경 이벤트에 구독
        GameManager.Instance.onPlayerLevelChanged += OnPlayerLevelChanged;

        // 초기 값 동기화
        SyncWithGameManager(false);

        UpdateUI();
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlayerLevelChanged -= OnPlayerLevelChanged;
        }
    }

    void Update()
    {
        // 매 프레임마다 GameManager와 동기화
        // 슬라이드 애니메이션 중이 아닐 때만 즉시 동기화
        if (slideCoroutine == null)
        {
            SyncWithGameManager(false);
        }
    }

    private void OnPlayerLevelChanged()
    {
        // 레벨업이 발생했을 때 애니메이션으로 동기화
        SyncWithGameManager(true);
    }

    public float GetCurrentXP()
    {
        return currentXP;
    }

    private void SyncWithGameManager(bool animate)
    {
        long gameManagerLevel = GameManager.Instance.playerLevel;
        float gameManagerXP = GameManager.Instance.playerExperience;
        xpToNextLevel = GameManager.Instance.experienceToLevelUp;

        if (animate)
        {
            Sync(gameManagerLevel, gameManagerXP);
        }
        else
        {
            // 즉시 동기화
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }

            playerLevel = gameManagerLevel;
            currentXP = gameManagerXP;
            UpdateUI();
        }
    }

    private IEnumerator SlideExperience(long targetLevel, float targetExp)
    {
        while (playerLevel != targetLevel || currentXP != targetExp)
        {
            // 현재 레벨에서 목표 경험치까지 슬라이딩
            if (targetLevel == playerLevel)
            {
                currentXP = Mathf.Min(currentXP + Mathf.CeilToInt(slideSpeed * Time.deltaTime), targetExp);
            }
            else
            {
                // 레벨업 진행 애니메이션
                xpToNextLevel = GameManager.Instance.experienceToLevelUp;
                currentXP = Mathf.Min(currentXP + Mathf.CeilToInt(slideSpeed * Time.deltaTime), xpToNextLevel);

                if (currentXP >= xpToNextLevel)
                {
                    playerLevel++;
                    currentXP -= xpToNextLevel;
                }
            }

            UpdateUI();
            yield return null;
        }

        // 최종 값 동기화 보장
        currentXP = targetExp;
        playerLevel = targetLevel;
        slideCoroutine = null;
    }

    public void Sync(long level, float experience)
    {
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }

        slideCoroutine = StartCoroutine(SlideExperience(level, experience));
    }

    private void UpdateUI()
    {
        levelText.text = "Lv. " + playerLevel;
        experienceSlider.value = currentXP / xpToNextLevel;
    }
}