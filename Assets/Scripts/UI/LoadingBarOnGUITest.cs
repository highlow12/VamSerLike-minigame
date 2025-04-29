using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// OnGUI를 사용하여 로딩 바 테스트를 위한 UI 컴포넌트
/// </summary>
public class LoadingBarOnGUITest : MonoBehaviour
{
    [Header("로딩 바 설정")]
    [SerializeField] private LoadingBar loadingBar;          // 테스트할 로딩 바
    [SerializeField] private float testDuration = 10f;       // 테스트 총 시간 (초)
    [SerializeField] private bool useRealLoadManager = true; // StageLoadManager 사용 여부
    [SerializeField] private bool randomJitter = false;      // 진행 속도에 불규칙성 추가 여부
    [SerializeField] private float jitterAmount = 0.1f;      // 불규칙성 정도
    
    [Header("씬 로딩 설정")]
    [SerializeField] private string loadingSceneName = "LoadingScene";  // 로딩 씬 이름
    [SerializeField] private string[] availableScenes;       // 로드 가능한 씬 목록
    
    private bool isRunningTest = false;                      // 테스트 실행 중인지 여부
    private float testProgress = 0f;                         // 현재 테스트 진행도
    private float simulationSpeed = 1.0f;                    // 시뮬레이션 속도 배율
    private string statusText = "테스트 준비 완료";          // 상태 텍스트
    private float sliderValue = 0.5f;                        // 슬라이더 값 (0.5~2.0 범위로 매핑)
    private bool useLoadingScene = true;                     // 로딩 씬 사용 여부
    private int selectedSceneIndex = 0;                      // 선택된 씬 인덱스
    private bool showSceneDropdown = false;                  // 씬 드롭다운 표시 여부
    private Vector2 scrollPosition;                          // 스크롤 위치
    
    private void Start()
    {
        // 로딩 바 초기화
        if (loadingBar != null)
        {
            loadingBar.SetProgress(0f);
        }
        
        // 빌드 설정에 있는 모든 씬 정보 가져오기
        if (availableScenes == null || availableScenes.Length == 0)
        {
            LoadAvailableScenesFromBuild();
        }
        
        // StageLoadManager 콜백 등록
        if (StageLoadManager.Instance != null)
        {
            StageLoadManager.Instance.AddLoadingProgressCallback(UpdateLoadingProgress);
            StageLoadManager.Instance.AddLoadingCompletedCallback(OnLoadingCompleted);
        }
    }
    
    private void OnGUI()
    {
        // GUI 스타일 설정
        SetupGUIStyles();
        
        // 화면 중앙 기준으로 UI 위치 설정
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f + 100f; // 로딩 바 아래에 위치
        
        // 레이아웃 시작
        GUILayout.BeginArea(new Rect(centerX - 180f, centerY, 360f, 400f));
        GUILayout.BeginVertical(GUI.skin.box);
        
        // 제목
        GUILayout.Label("로딩 바 테스트", GUI.skin.GetStyle("Box"), GUILayout.Height(30));
        
        // 상태 텍스트
        GUILayout.Label($"상태: {statusText}");
        
        GUILayout.Space(10);
        
        // 테스트 유형 선택 탭
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(useRealLoadManager == false, "로딩 시뮬레이션", GUI.skin.GetStyle("Button"), GUILayout.Height(30)))
        {
            useRealLoadManager = false;
        }
        if (GUILayout.Toggle(useRealLoadManager == true, "실제 씬 로딩", GUI.skin.GetStyle("Button"), GUILayout.Height(30)))
        {
            useRealLoadManager = true;
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // 시뮬레이션 설정
        if (!useRealLoadManager)
        {
            ShowSimulationOptions();
        }
        // 실제 씬 로딩 설정
        else
        {
            ShowSceneLoadingOptions();
        }
        
        GUILayout.Space(10);
        
        // 테스트 시작 버튼
        GUI.enabled = !isRunningTest; // 테스트 중이면 버튼 비활성화
        if (GUILayout.Button(useRealLoadManager ? "선택한 씬 로드" : "로딩 테스트 시작 (10초)", GUILayout.Height(40)))
        {
            StartLoadingTest();
        }
        GUI.enabled = true;
        
        GUILayout.Space(10);
        
        // 진행률 표시
        if (isRunningTest)
        {
            GUILayout.Label($"진행률: {Mathf.Round(testProgress * 100)}%");
            
            // 진행 상황 바 표시
            Rect progressRect = GUILayoutUtility.GetRect(18, 18, "TextField", GUILayout.ExpandWidth(true));
            EditorProgressBar(progressRect, testProgress);
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// GUI 스타일 설정
    /// </summary>
    private void SetupGUIStyles()
    {
        GUI.skin.button.fontSize = 14;
        GUI.skin.label.fontSize = 14;
        GUI.skin.toggle.fontSize = 14;
        GUI.skin.box.fontSize = 16;
        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
    }
    
    /// <summary>
    /// 시뮬레이션 옵션 UI 표시
    /// </summary>
    private void ShowSimulationOptions()
    {
        // 시뮬레이션 속도 슬라이더
        GUILayout.BeginHorizontal();
        GUILayout.Label("속도: ", GUILayout.Width(50));
        sliderValue = GUILayout.HorizontalSlider(sliderValue, 0f, 1f);
        simulationSpeed = Mathf.Lerp(0.5f, 2.0f, sliderValue);
        GUILayout.Label($"{simulationSpeed:F1}x", GUILayout.Width(40));
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // 불규칙적 진행 옵션
        randomJitter = GUILayout.Toggle(randomJitter, "불규칙적 진행 시뮬레이션");
        
        // 불규칙성 강도 슬라이더 (불규칙성이 활성화된 경우에만 표시)
        if (randomJitter)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("불규칙성: ", GUILayout.Width(70));
            jitterAmount = GUILayout.HorizontalSlider(jitterAmount, 0.01f, 0.2f);
            GUILayout.Label($"{jitterAmount:F2}", GUILayout.Width(40));
            GUILayout.EndHorizontal();
        }
        
        GUILayout.Space(5);
        
        // 테스트 시간 조절
        GUILayout.BeginHorizontal();
        GUILayout.Label("테스트 시간: ", GUILayout.Width(100));
        float testDurationTemp = (float)System.Math.Round(
            GUILayout.HorizontalSlider(testDuration, 3f, 20f), 0);
        if (testDurationTemp != testDuration)
        {
            testDuration = testDurationTemp;
        }
        GUILayout.Label($"{testDuration:F0}초", GUILayout.Width(40));
        GUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 씬 로딩 옵션 UI 표시
    /// </summary>
    private void ShowSceneLoadingOptions()
    {
        // 로딩 씬 사용 옵션
        useLoadingScene = GUILayout.Toggle(useLoadingScene, "로딩 씬 사용");
        
        GUILayout.Space(5);
        
        // 씬 선택 드롭다운
        GUILayout.BeginHorizontal();
        GUILayout.Label("씬 선택: ", GUILayout.Width(60));
        if (GUILayout.Button(selectedSceneIndex >= 0 && selectedSceneIndex < availableScenes.Length ? 
                            availableScenes[selectedSceneIndex] : "씬 선택", GUILayout.ExpandWidth(true)))
        {
            showSceneDropdown = !showSceneDropdown;
        }
        GUILayout.EndHorizontal();
        
        // 드롭다운 표시
        if (showSceneDropdown && availableScenes != null && availableScenes.Length > 0)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect dropdownRect = new Rect(lastRect.x, lastRect.y + lastRect.height, lastRect.width, 
                                        Mathf.Min(200f, availableScenes.Length * 25f));
            
            // 스크롤 가능한 씬 목록 표시
            GUI.Box(dropdownRect, "");
            
            scrollPosition = GUI.BeginScrollView(dropdownRect, scrollPosition, 
                                               new Rect(0, 0, dropdownRect.width - 20, availableScenes.Length * 25));
            
            for (int i = 0; i < availableScenes.Length; i++)
            {
                if (GUI.Button(new Rect(0, i * 25, dropdownRect.width - 20, 25), availableScenes[i]))
                {
                    selectedSceneIndex = i;
                    showSceneDropdown = false;
                }
            }
            
            GUI.EndScrollView();
            
            // 드롭다운 외부 클릭 감지
            if (Event.current.type == EventType.MouseDown && !dropdownRect.Contains(Event.current.mousePosition))
            {
                showSceneDropdown = false;
                GUI.changed = true;
            }
        }
        
        GUILayout.Space(10);
        
        // 빌드 설정의 모든 씬 표시
        if (GUILayout.Button("빌드 설정의 씬 정보 갱신"))
        {
            LoadAvailableScenesFromBuild();
        }
    }
    
    /// <summary>
    /// 진행 상황 바 그리기
    /// </summary>
    private void EditorProgressBar(Rect rect, float value)
    {
        // 배경 영역
        GUI.Box(rect, "");
        
        // 채워진 영역
        Rect fillRect = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * value, rect.height - 4);
        Color oldColor = GUI.color;
        GUI.color = Color.green;
        GUI.Box(fillRect, "");
        GUI.color = oldColor;
        
        // 퍼센트 텍스트
        string percentText = $"{Mathf.Round(value * 100)}%";
        GUIStyle centeredText = new GUIStyle(GUI.skin.label);
        centeredText.alignment = TextAnchor.MiddleCenter;
        GUI.Label(rect, percentText, centeredText);
    }
    
    /// <summary>
    /// 빌드 설정에서 사용 가능한 씬 목록 로드
    /// </summary>
    private void LoadAvailableScenesFromBuild()
    {
        List<string> scenes = new List<string>();
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        
        for (int i = 0; i < sceneCount; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            
            // 현재 씬은 제외
            if (sceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                scenes.Add(sceneName);
            }
        }
        
        availableScenes = scenes.ToArray();
        
        if (availableScenes.Length > 0)
        {
            selectedSceneIndex = 0;
            statusText = $"{availableScenes.Length}개의 씬을 찾았습니다";
        }
        else
        {
            selectedSceneIndex = -1;
            statusText = "빌드 설정에 다른 씬이 없습니다";
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
        
        // 실제 씬 로딩
        if (useRealLoadManager)
        {
            // 씬이 선택되지 않은 경우
            if (selectedSceneIndex < 0 || availableScenes == null || selectedSceneIndex >= availableScenes.Length)
            {
                statusText = "로드할 씬을 선택하세요";
                isRunningTest = false;
                return;
            }
            
            string selectedScene = availableScenes[selectedSceneIndex];
            statusText = $"{selectedScene} 로딩 중...";
            
            // StageLoadManager로 씬 로드
            if (StageLoadManager.Instance != null)
            {
                StageLoadManager.Instance.LoadSceneAsync(selectedScene, useLoadingScene, loadingSceneName);
            }
            else
            {
                statusText = "StageLoadManager가 없습니다!";
                isRunningTest = false;
            }
        }
        // 로딩 시뮬레이션
        else
        {
            statusText = "로딩 테스트 진행 중...";
            StartCoroutine(RunLoadingTestSimulation());
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
        // 현재 진행도 업데이트
        testProgress = progress;
        
        // 로딩 바 업데이트
        if (loadingBar != null)
        {
            loadingBar.SetProgress(progress);
        }
    }
    
    /// <summary>
    /// 로딩 완료 처리
    /// </summary>
    private void OnLoadingCompleted()
    {
        if (!useRealLoadManager) // 시뮬레이션인 경우에만 상태 변경
        {
            isRunningTest = false;
            statusText = "로딩 완료!";
            
            // 3초 후 초기 상태로 복원
            StartCoroutine(ResetAfterDelay());
        }
    }
    
    /// <summary>
    /// 지연 후 상태 초기화
    /// </summary>
    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        
        if (!isRunningTest)
        {
            statusText = "테스트 준비 완료";
        }
    }
    
    private void OnDestroy()
    {
        // 콜백 해제
        if (StageLoadManager.Instance != null)
        {
            StageLoadManager.Instance.OnLoadingProgressChanged -= UpdateLoadingProgress;
            StageLoadManager.Instance.OnLoadingCompleted -= OnLoadingCompleted;
        }
    }
}