using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 씬 로딩과 로딩 바를 실제로 테스트하기 위한 도구
/// </summary>
public class SceneLoadTester : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private LoadingBar loadingBar;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TMP_Dropdown sceneDropdown;
    [SerializeField] private Toggle useLoadingSceneToggle;

    [Header("설정")]
    [SerializeField] private string[] testSceneNames;
    [SerializeField] private string loadingSceneName = "LoadingScene";

    private void Start()
    {
        // 드롭다운 초기화
        InitializeSceneDropdown();

        // 로딩 씬 토글 초기화
        if (useLoadingSceneToggle != null)
        {
            useLoadingSceneToggle.isOn = true;
        }

        // 상태 텍스트 초기화
        if (statusText != null)
        {
            statusText.text = "테스트할 씬을 선택하세요";
        }

        // 로딩 바 초기화
        if (loadingBar != null)
        {
            loadingBar.SetProgress(0f);
        }

        // 진행 상태 업데이트 콜백 등록
        if (StageLoadManager.Instance != null)
        {
            StageLoadManager.Instance.AddLoadingProgressCallback(UpdateLoadingProgress);
        }
        else
        {
            Debug.LogError("StageLoadManager 인스턴스가 없습니다!");
        }
    }

    /// <summary>
    /// 사용 가능한 씬 목록으로 드롭다운 초기화
    /// </summary>
    private void InitializeSceneDropdown()
    {
        if (sceneDropdown == null) return;

        // 드롭다운 옵션 초기화
        sceneDropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        // 기본 안내 옵션
        options.Add(new TMP_Dropdown.OptionData("씬 선택"));

        // 테스트 씬 목록 추가
        foreach (string sceneName in testSceneNames)
        {
            if (StageLoadManager.Instance.IsSceneValid(sceneName))
            {
                options.Add(new TMP_Dropdown.OptionData(sceneName));
            }
            else
            {
                Debug.LogWarning($"Scene '{sceneName}' is not in the build settings!");
            }
        }

        // 옵션 설정
        sceneDropdown.AddOptions(options);
        sceneDropdown.value = 0;
    }

    /// <summary>
    /// 선택한 씬 로드 시작
    /// </summary>
    public void LoadSelectedScene()
    {
        // 드롭다운에서 선택한 씬 이름 가져오기
        if (sceneDropdown == null || sceneDropdown.value == 0)
        {
            if (statusText != null)
            {
                statusText.text = "로드할 씬을 선택하세요!";
            }
            return;
        }

        string selectedScene = sceneDropdown.options[sceneDropdown.value].text;
        bool useLoadingScene = useLoadingSceneToggle != null && useLoadingSceneToggle.isOn;

        if (statusText != null)
        {
            statusText.text = $"{selectedScene} 로딩 중...";
        }

        // 선택한 씬 로드
        StageLoadManager.Instance.LoadSceneAsync(selectedScene, useLoadingScene, loadingSceneName);
    }

    /// <summary>
    /// 로딩 진행 상황 업데이트
    /// </summary>
    private void UpdateLoadingProgress(float progress)
    {
        if (loadingBar != null)
        {
            loadingBar.SetProgress(progress);
        }

        if (statusText != null)
        {
            statusText.text = $"로딩 중... {Mathf.Round(progress * 100)}%";
        }
    }

    /// <summary>
    /// 현재 씬 정보 수집
    /// </summary>
    public void GatherBuildSceneInfo()
    {
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        List<string> sceneNames = new List<string>();

        for (int i = 0; i < sceneCount; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            sceneNames.Add(name);
            Debug.Log($"빌드 씬 #{i}: {name}");
        }

        if (statusText != null && sceneNames.Count > 0)
        {
            statusText.text = $"{sceneNames.Count}개의 씬 발견, 콘솔 확인";
        }
        else if (statusText != null)
        {
            statusText.text = "빌드 세팅에 씬이 없습니다";
        }
    }

    private void OnDestroy()
    {
        // 콜백 해제
        if (StageLoadManager.Instance != null)
        {
            StageLoadManager.Instance.OnLoadingProgressChanged -= UpdateLoadingProgress;
        }
    }
}