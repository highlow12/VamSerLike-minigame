using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class ExperienceBar : MonoBehaviour
{
    public Slider experienceSlider; // 경험치 슬라이더
    public TextMeshProUGUI levelText;          // 레벨 텍스트
    Coroutine slideCoroutine;   // 슬라이드 코루틴
    public float slideSpeed = 5f;   // 슬라이드 속도
    private long playerLevel = 1;    // 현재 플레이어 레벨
    private float currentXP = 0;     // 현재 경험치
    private float xpToNextLevel = 100; // 다음 레벨까지 필요한 경험치

    void Start()
    {
        UpdateUI();
    }

    // 현재 경험치를 반환
    public float GetCurrentXP()
    {
        return currentXP;
    }

    // 슬라이드 애니메이션 코루틴
    private IEnumerator SlideExperience(long targetLevel, float targetExp)
    {
        xpToNextLevel = GameManager.Instance.GetExperienceToNextLevel(playerLevel);
        while (playerLevel != targetLevel || currentXP != targetExp)
        {
            if (targetLevel == playerLevel)
            {
                currentXP = Mathf.Min(currentXP + Mathf.CeilToInt(slideSpeed * Time.deltaTime), targetExp);
            }
            else
            {
                currentXP = Mathf.Min(currentXP + Mathf.CeilToInt(slideSpeed * Time.deltaTime), xpToNextLevel);
            }
            if (currentXP >= xpToNextLevel)
            {
                playerLevel++;
                currentXP -= xpToNextLevel;
                xpToNextLevel = GameManager.Instance.GetExperienceToNextLevel(playerLevel);
            }
            UpdateUI();
            yield return null;
        }
        currentXP = targetExp;
        playerLevel = targetLevel;
        slideCoroutine = null;
    }

    public void Sync(long level, float experience)
    {
        slideCoroutine ??= StartCoroutine(SlideExperience(level, experience));
    }


    // UI 업데이트
    private void UpdateUI()
    {
        levelText.text = "Lv. " + playerLevel;
        experienceSlider.value = currentXP / xpToNextLevel;
    }
}
