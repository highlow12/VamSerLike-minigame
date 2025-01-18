using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExperienceBar : MonoBehaviour
{
    public Slider experienceSlider; // 경험치 슬라이더
    public Text levelText;          // 레벨 텍스트
    public float slideSpeed = 5f;   // 슬라이드 속도

    private int playerLevel = 1;    // 현재 플레이어 레벨
    private int currentXP = 0;      // 현재 경험치
    private int xpToNextLevel = 100; // 다음 레벨까지 필요한 경험치

    void Start()
    {
        UpdateUI();
    }

    // 현재 경험치를 반환
    public int GetCurrentXP()
    {
        return currentXP;
    }

    // 경험치 정산 및 UI 업데이트
    public void AddExperience(int amount)
    {
        StartCoroutine(SlideExperience(amount));
    }

    // 슬라이드 애니메이션 코루틴
    private IEnumerator SlideExperience(int amount)
    {
        int targetXP = currentXP + amount;

        while (currentXP < targetXP)
        {
            // 슬라이더 값 증가
            currentXP = Mathf.Min(currentXP + Mathf.CeilToInt(slideSpeed * Time.deltaTime), targetXP);
            experienceSlider.value = (float)currentXP / xpToNextLevel;

            // 레벨 업 체크
            if (currentXP >= xpToNextLevel)
            {
                // 초과된 경험치 계산
                int overflowXP = currentXP - xpToNextLevel;
                currentXP = 0;
                LevelUp();

                // 초과 경험치를 다음 단계에 반영
                targetXP = overflowXP;
            }

            yield return null;
        }

        UpdateUI();
    }

    // 레벨 업 처리
    private void LevelUp()
    {
        playerLevel++;
        xpToNextLevel = Mathf.FloorToInt(xpToNextLevel * 1.2f); // 레벨업 시 필요 경험치 증가
    }

    // UI 업데이트
    private void UpdateUI()
    {
        levelText.text = "Lv. " + playerLevel;
        experienceSlider.value = (float)currentXP / xpToNextLevel;
    }
}
