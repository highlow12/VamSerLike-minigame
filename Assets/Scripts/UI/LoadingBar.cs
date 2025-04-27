using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingBar : MonoBehaviour
{
    [Header("로딩 바 설정")]
    [SerializeField] private Image fillImage;                // 채워질 이미지
    [SerializeField] private RectTransform fillRectTransform; // 채워질 이미지의 RectTransform
    [SerializeField] private TextMeshProUGUI percentText;    // 퍼센트 표시 텍스트 (선택적)
    [SerializeField] private float smoothSpeed = 10f;        // 부드러운 전환을 위한 속도

    [Header("애니메이션 설정")]
    [SerializeField] private bool useAnimation = true;       // 애니메이션 사용 여부
    [SerializeField] private float pulseAmount = 0.05f;      // 펄스 애니메이션 크기
    [SerializeField] private float pulseSpeed = 1f;          // 펄스 애니메이션 속도

    private float targetFillAmount = 0f;                     // 목표 채움 양
    private float currentFillAmount = 0f;                    // 현재 채움 양

    private void Awake()
    {
        // StageLoadManager가 있다면 로딩 진행 상황을 구독
        if (StageLoadManager.Instance != null)
        {
            StageLoadManager.Instance.AddLoadingProgressCallback(UpdateLoadingBar);
        }
    }

    void Start()
    {
        // 초기화
        SetProgress(0f);
    }

    void Update()
    {
        // 부드러운 로딩 바 업데이트
        SmoothUpdateFillAmount();

        // 애니메이션 효과 적용
        if (useAnimation && fillRectTransform != null && targetFillAmount < 1f)
        {
            ApplyPulseAnimation();
        }
    }

    /// <summary>
    /// 로딩 바의 진행률을 설정합니다.
    /// </summary>
    /// <param name="progress">0에서 1 사이의 진행률</param>
    public void SetProgress(float progress)
    {
        targetFillAmount = Mathf.Clamp01(progress);

        // 퍼센트 텍스트가 있으면 업데이트
        if (percentText != null)
        {
            percentText.text = $"{Mathf.Round(targetFillAmount * 100)}%";
        }
    }

    /// <summary>
    /// StageLoadManager로부터 로딩 진행 상황을 받아 로딩 바에 적용합니다.
    /// </summary>
    /// <param name="progress">로딩 진행 상황 (0~1)</param>
    public void UpdateLoadingBar(float progress)
    {
        SetProgress(progress);
    }

    /// <summary>
    /// 부드러운 로딩 바 업데이트
    /// </summary>
    private void SmoothUpdateFillAmount()
    {
        if (fillImage != null)
        {
            // 현재 값을 목표값으로 부드럽게 보간
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
            fillImage.fillAmount = currentFillAmount;
        }
    }

    /// <summary>
    /// 로딩 중 펄스 애니메이션 적용
    /// </summary>
    private void ApplyPulseAnimation()
    {
        float pulse = 1f + pulseAmount * Mathf.Sin(Time.time * pulseSpeed);
        fillRectTransform.localScale = new Vector3(1f, pulse, 1f);
    }

    private void OnDestroy()
    {
        // 구독 해제
        if (StageLoadManager.Instance != null)
        {
            StageLoadManager.Instance.OnLoadingProgressChanged -= UpdateLoadingBar;
        }
    }
}
