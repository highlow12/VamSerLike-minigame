using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private LoadingBar loadingBar;
    [SerializeField] private TextMeshProUGUI loadingTipText;
    [SerializeField] private TextMeshProUGUI loadingStatusText;
    
    [Header("설정")]
    [SerializeField] private string[] loadingTips;
    [SerializeField] private float tipChangeDuration = 3f;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private int currentTipIndex = 0;
    
    private void Awake()
    {
        // StageLoadManager가 있는지 확인
        if (StageLoadManager.Instance == null)
        {
            Debug.LogError("StageLoadManager 인스턴스가 없습니다.");
        }
    }
    
    private void Start()
    {
        // UI 초기화
        InitializeUI();
        
        // 팁 텍스트 변경 시작
        if (loadingTips != null && loadingTips.Length > 0 && loadingTipText != null)
        {
            StartCoroutine(ChangeTipRoutine());
        }
        
        // 페이드 인 효과
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
        
        // 로딩 상태 표시 업데이트
        if (loadingStatusText != null)
        {
            StageLoadManager.Instance.AddLoadingProgressCallback(UpdateLoadingStatus);
        }
    }
    
    private void InitializeUI()
    {
        // 로딩바 초기화
        if (loadingBar != null)
        {
            loadingBar.SetProgress(0f);
        }
        
        // 팁 텍스트 초기화
        if (loadingTipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            currentTipIndex = Random.Range(0, loadingTips.Length);
            loadingTipText.text = loadingTips[currentTipIndex];
        }
        
        // 페이드 캔버스 그룹 초기화
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
        }
    }
    
    private IEnumerator ChangeTipRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(tipChangeDuration);
            
            // 다음 팁으로 변경
            currentTipIndex = (currentTipIndex + 1) % loadingTips.Length;
            
            // 페이드 효과로 팁 텍스트 변경
            StartCoroutine(FadeTipText(loadingTips[currentTipIndex]));
        }
    }
    
    private IEnumerator FadeTipText(string newTip)
    {
        float duration = 0.5f;
        float time = 0f;
        
        // 페이드 아웃
        while (time < duration)
        {
            time += Time.deltaTime;
            loadingTipText.alpha = Mathf.Lerp(1f, 0f, time / duration);
            yield return null;
        }
        
        // 텍스트 변경
        loadingTipText.text = newTip;
        
        // 페이드 인
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            loadingTipText.alpha = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }
    }
    
    private void UpdateLoadingStatus(float progress)
    {
        if (loadingStatusText != null)
        {
            if (progress < 0.25f)
            {
                loadingStatusText.text = "데이터 로딩 중...";
            }
            else if (progress < 0.5f)
            {
                loadingStatusText.text = "에셋 로딩 중...";
            }
            else if (progress < 0.75f)
            {
                loadingStatusText.text = "씬 구성 중...";
            }
            else if (progress < 1f)
            {
                loadingStatusText.text = "마무리 작업 중...";
            }
            else
            {
                loadingStatusText.text = "완료!";
            }
        }
    }
    
    private IEnumerator FadeIn()
    {
        float time = 0f;
        fadeCanvasGroup.alpha = 1f;
        
        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeInDuration);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 0f;
    }
    
    private IEnumerator FadeOut()
    {
        float time = 0f;
        fadeCanvasGroup.alpha = 0f;
        
        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeOutDuration);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1f;
    }
}