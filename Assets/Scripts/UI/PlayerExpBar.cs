using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerExpBar : MonoBehaviour
{
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private float fillSpeed = 2f;

    private float targetFillAmount = 0f;

    private void Start()
    {
        // Slider 컴포넌트가 없다면 자동으로 가져오기 시도
        if (experienceSlider == null)
        {
            experienceSlider = GetComponent<Slider>();
            if (experienceSlider == null)
            {
                Debug.LogError("PlayerExpBar에 Slider 컴포넌트가 없습니다!");
            }
        }

        UpdateUI();
        Debug.Log("PlayerExpBar 초기화 완료");
    }

    private void Update()
    {
        // 부드러운 경험치 바 증가 효과
        if (GameManager.Instance != null && GameManager.Instance.gameState == GameManager.GameState.InGame)
        {
            targetFillAmount = GameManager.Instance.playerExperience / GameManager.Instance.experienceToLevelUp;

            if (Mathf.Abs(experienceSlider.value - targetFillAmount) > 0.005f)
            {
                experienceSlider.value = Mathf.Lerp(experienceSlider.value, targetFillAmount, Time.deltaTime * fillSpeed);

                // 목표 값에 충분히 가까워지면 정확한 값으로 설정
                if (Mathf.Abs(experienceSlider.value - targetFillAmount) < 0.01f)
                {
                    experienceSlider.value = targetFillAmount;
                }
            }

            // 레벨 텍스트 업데이트 추가
            if (levelText != null)
            {
                levelText.text = GameManager.Instance.playerLevel.ToString();
            }
        }
    }

    // 메서드를 public으로 변경하여 외부에서 접근 가능하게 함
    public void UpdateUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance가 null입니다!");
            return;
        }

        UpdateLevelText();
        targetFillAmount = GameManager.Instance.playerExperience / GameManager.Instance.experienceToLevelUp;
        experienceSlider.value = targetFillAmount;

        Debug.Log($"경험치 바 수동 업데이트: {targetFillAmount * 100:F1}% (경험치: {GameManager.Instance.playerExperience}/{GameManager.Instance.experienceToLevelUp})");
    }

    // 레벨 텍스트 업데이트
    private void UpdateLevelText()
    {
        Debug.Log($"UpdateLevelText 호출됨. GameManager null 체크: {GameManager.Instance != null}");

        if (levelText != null && GameManager.Instance != null)
        {
            Debug.Log($"현재 레벨: {GameManager.Instance.playerLevel}");
            levelText.text = "Lv. " + GameManager.Instance.playerLevel.ToString();
        }
        else
        {
            Debug.LogError("levelText 또는 GameManager.Instance가 null입니다!");
        }
    }
}