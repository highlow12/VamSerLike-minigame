using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 로딩 바 시뮬레이션을 테스트하기 위한 UI 컴포넌트
/// </summary>
public class LoadingBarTestUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private LoadingBar loadingBar;          // 테스트할 로딩 바
    [SerializeField] private Button testButton;              // 테스트 시작 버튼
    [SerializeField] private TextMeshProUGUI statusText;     // 상태 표시 텍스트
    [SerializeField] private Slider speedSlider;             // 로딩 속도 조절 슬라이더 (선택 사항)
    [SerializeField] private TextMeshProUGUI speedText;      // 로딩 속도 표시 텍스트 (선택 사항)
    
    [Header("테스트 설정")]
    [SerializeField] private float testDuration = 10f;       // 테스트 총 시간 (초)
    [SerializeField] private bool useRealLoadManager = true; // StageLoadManager 사용 여부
    [SerializeField] private bool randomJitter = false;      // 진행 속도에 불규칙성 추가 여부
    [SerializeField] private float jitterAmount = 0.1f;      // 불규칙성 정도
    
    private bool isRunningTest = false;                      // 테스트 실행 중인지 여부
    private float testProgress = 0f;                         // 현재 테스트 진행도
    private float simulationSpeed = 1.0f;                    // 시뮬레이션 속도 배율
    
    private void Start()
    {
        // 버튼 클릭 이벤트 등록
        if (testButton != null)
        {
            testButton.onClick.AddListener(StartLoadingTest);
        }
        
        // 슬라이더 값 변경 이벤트 등록
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(UpdateSimulationSpeed);
            UpdateSimulationSpeed(speedSlider.value); // 초기화
        }
        
        // 로딩 바 초기화
        if (loadingBar != null)
        {
            loadingBar.SetProgress(0f);
        }
        
        // 상태 텍스트 초기화
        if (statusText != null)
        {
            statusText.text = "테스트 준비 완료";
        }
    }
    
    /// <summary>
    /// 로딩 테스트를 시작합니다.
    /// </summary>
    public void StartLoadingTest()
    {
        if (isRunningTest)
        {
            return; // 이미 테스트 중이면 무시
        }
        
        isRunningTest = true;
        testProgress = 0f;
        
        if (statusText != null)
        {
            statusText.text = "로딩 테스트 진행 중...";
        }
        
        if (testButton != null)
        {
            testButton.interactable = false; // 테스트 중에는 버튼 비활성화
        }
        
        // 실제 StageLoadManager를 사용할 경우
        if (useRealLoadManager && StageLoadManager.Instance != null)
        {
            StartCoroutine(RunLoadingTestWithManager());
        }
        // 직접 시뮬레이션하는 경우
        else
        {
            StartCoroutine(RunLoadingTestSimulation());
        }
    }
    
    /// <summary>
    /// StageLoadManager를 사용한 로딩 테스트
    /// </summary>
    private IEnumerator RunLoadingTestWithManager()
    {
        // 콜백 등록
        StageLoadManager.Instance.AddLoadingProgressCallback(UpdateLoadingProgress);
        StageLoadManager.Instance.AddLoadingCompletedCallback(OnLoadingCompleted);
        
        // 가상 로딩 시작
        StartCoroutine(SimulateLoadingInManager());
        
        while (isRunningTest)
        {
            yield return null;
        }
        
        // 테스트 종료 후 콜백 제거
        StageLoadManager.Instance.OnLoadingProgressChanged -= UpdateLoadingProgress;
        StageLoadManager.Instance.OnLoadingCompleted -= OnLoadingCompleted;
    }
    
    /// <summary>
    /// StageLoadManager 내부 로딩 프로세스 시뮬레이션
    /// </summary>
    private IEnumerator SimulateLoadingInManager()
    {
        float elapsed = 0f;
        
        // 필드에 직접 접근할 수 없으므로 리플렉션 사용
        System.Reflection.FieldInfo isLoadingField = typeof(StageLoadManager).GetField("isLoading", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        System.Reflection.FieldInfo progressField = typeof(StageLoadManager).GetField("loadingProgress", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (isLoadingField != null && progressField != null)
        {
            isLoadingField.SetValue(StageLoadManager.Instance, true);
            
            // 테스트 시간 동안 진행
            while (elapsed < testDuration)
            {
                elapsed += Time.deltaTime * simulationSpeed;
                
                // 진행률 계산 (0~1)
                float progress = elapsed / testDuration;
                
                // 불규칙성 추가 (선택 사항)
                if (randomJitter)
                {
                    progress += Random.Range(-jitterAmount, jitterAmount) * Time.deltaTime;
                    progress = Mathf.Clamp01(progress);
                }
                
                // StageLoadManager의 진행률 필드 업데이트
                progressField.SetValue(StageLoadManager.Instance, progress);
                
                // OnLoadingProgressChanged 이벤트 호출
                StageLoadManager.Instance.AddLoadingProgressCallback(UpdateLoadingProgress);
                
                yield return null;
            }
            
            // 완료
            progressField.SetValue(StageLoadManager.Instance, 1f);
            StageLoadManager.Instance.AddLoadingProgressCallback(UpdateLoadingProgress);
            
            yield return new WaitForSeconds(0.5f);
            
            // 리소스 정리
            isLoadingField.SetValue(StageLoadManager.Instance, false);
            
            // 완료 이벤트 호출
            StageLoadManager.Instance.AddLoadingCompletedCallback(OnLoadingCompleted);
        }
        else
        {
            Debug.LogError("StageLoadManager 내부 필드에 접근할 수 없습니다.");
            isRunningTest = false;
            if (statusText != null)
            {
                statusText.text = "테스트 실패: 로드 매니저 접근 오류";
            }
        }
    }
    
    /// <summary>
    /// 단순 시뮬레이션을 통한 로딩 테스트
    /// </summary>
    private IEnumerator RunLoadingTestSimulation()
    {
        float elapsed = 0f;
        
        // 테스트 시간 동안 진행
        while (elapsed < testDuration)
        {
            elapsed += Time.deltaTime * simulationSpeed;
            
            // 진행률 계산 (0~1)
            testProgress = elapsed / testDuration;
            
            // 불규칙성 추가 (선택 사항)
            if (randomJitter)
            {
                testProgress += Random.Range(-jitterAmount, jitterAmount) * Time.deltaTime;
                testProgress = Mathf.Clamp01(testProgress);
            }
            
            // 로딩 바 업데이트
            UpdateLoadingProgress(testProgress);
            
            yield return null;
        }
        
        // 완료
        UpdateLoadingProgress(1f);
        yield return new WaitForSeconds(0.5f);
        OnLoadingCompleted();
    }
    
    /// <summary>
    /// 로딩 진행 상황 업데이트
    /// </summary>
    private void UpdateLoadingProgress(float progress)
    {
        // 로딩 바 업데이트
        if (loadingBar != null)
        {
            loadingBar.SetProgress(progress);
        }
        
        // 상태 텍스트 업데이트
        if (statusText != null)
        {
            statusText.text = $"로딩 중... {Mathf.Round(progress * 100)}%";
        }
    }
    
    /// <summary>
    /// 로딩 완료 처리
    /// </summary>
    private void OnLoadingCompleted()
    {
        isRunningTest = false;
        
        if (statusText != null)
        {
            statusText.text = "로딩 완료!";
        }
        
        if (testButton != null)
        {
            testButton.interactable = true; // 버튼 다시 활성화
        }
        
        // 1초 후 초기 상태로 복원
        StartCoroutine(ResetAfterDelay());
    }
    
    /// <summary>
    /// 시뮬레이션 속도 업데이트
    /// </summary>
    private void UpdateSimulationSpeed(float value)
    {
        // 0.5 ~ 2.0 범위로 매핑
        simulationSpeed = Mathf.Lerp(0.5f, 2.0f, value);
        
        // 텍스트 업데이트
        if (speedText != null)
        {
            speedText.text = $"속도: {simulationSpeed:F1}x";
        }
    }
    
    /// <summary>
    /// 지연 후 상태 초기화
    /// </summary>
    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        
        if (!isRunningTest)
        {
            if (statusText != null)
            {
                statusText.text = "테스트 준비 완료";
            }
        }
    }
}