using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class StageLoadManager : Singleton<StageLoadManager>
{
    // 로딩 상태를 추적하기 위한 변수들
    private bool isLoading = false;
    private float loadingProgress = 0f;
    
    // 로딩 완료 콜백을 위한 이벤트
    public event Action<float> OnLoadingProgressChanged;
    public event Action OnLoadingCompleted;
    
    // 현재 로딩 진행 상황을 가져오는 속성
    public float LoadingProgress => loadingProgress;
    public bool IsLoading => isLoading;
    
    /// <summary>
    /// 씬을 비동기적으로 로드합니다.
    /// </summary>
    /// <param name="sceneName">로드할 씬 이름</param>
    /// <param name="useLoadingScene">로딩 씬 사용 여부</param>
    /// <param name="loadingSceneName">사용할 로딩 씬 이름 (기본값: "LoadingScene")</param>
    public void LoadSceneAsync(string sceneName, bool useLoadingScene = false, string loadingSceneName = "LoadingScene")
    {
        if (isLoading)
        {
            Debug.LogWarning("Already loading a scene!");
            return;
        }
        
        if (useLoadingScene)
        {
            StartCoroutine(LoadSceneWithLoadingScreen(sceneName, loadingSceneName));
        }
        else
        {
            StartCoroutine(LoadSceneDirectly(sceneName));
        }
    }
    
    /// <summary>
    /// 로딩 씬 없이 씬을 직접 로드합니다.
    /// </summary>
    private IEnumerator LoadSceneDirectly(string sceneName)
    {
        isLoading = true;
        loadingProgress = 0f;
        
        // 씬 로드 시작
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;
        
        // 로딩이 완료될 때까지 진행 상황 업데이트
        while (!asyncOperation.isDone)
        {
            // 로딩 진행률은 0.9까지만 진행됨 (마지막 0.1은 씬 활성화 시 진행)
            loadingProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            OnLoadingProgressChanged?.Invoke(loadingProgress);
            
            // 로딩이 거의 완료되었을 때
            if (asyncOperation.progress >= 0.9f)
            {
                loadingProgress = 1f;
                OnLoadingProgressChanged?.Invoke(loadingProgress);
                asyncOperation.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        isLoading = false;
        OnLoadingCompleted?.Invoke();
    }
    
    /// <summary>
    /// 로딩 씬을 먼저 로드한 후 목표 씬을 로드합니다.
    /// </summary>
    private IEnumerator LoadSceneWithLoadingScreen(string targetSceneName, string loadingSceneName)
    {
        isLoading = true;
        loadingProgress = 0f;
        
        // 먼저 로딩 씬 로드
        AsyncOperation loadLoadingScene = SceneManager.LoadSceneAsync(loadingSceneName);
        
        while (!loadLoadingScene.isDone)
        {
            yield return null;
        }
        
        // 로딩 씬이 로드된 후 실제 씬 로드 시작
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetSceneName);
        asyncOperation.allowSceneActivation = false;
        
        // 로딩이 완료될 때까지 진행 상황 업데이트
        while (!asyncOperation.isDone)
        {
            // 로딩 진행률은 0.9까지만 진행됨 (마지막 0.1은 씬 활성화 시 진행)
            loadingProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            OnLoadingProgressChanged?.Invoke(loadingProgress);
            
            // 로딩이 거의 완료되었을 때
            if (asyncOperation.progress >= 0.9f)
            {
                loadingProgress = 1f;
                OnLoadingProgressChanged?.Invoke(loadingProgress);
                
                // 잠시 대기 후 씬 활성화
                yield return new WaitForSeconds(0.5f);
                asyncOperation.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        isLoading = false;
        OnLoadingCompleted?.Invoke();
    }
    
    /// <summary>
    /// 로딩 진행 상황에 대한 추가 작업을 수행하는 콜백을 추가합니다.
    /// </summary>
    public void AddLoadingProgressCallback(Action<float> callback)
    {
        OnLoadingProgressChanged += callback;
    }
    
    /// <summary>
    /// 로딩 완료 후 실행할 작업을 추가합니다.
    /// </summary>
    public void AddLoadingCompletedCallback(Action callback)
    {
        OnLoadingCompleted += callback;
    }
    
    /// <summary>
    /// 씬 이름으로 씬이 유효한지 확인합니다.
    /// </summary>
    public bool IsSceneValid(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            
            if (name == sceneName)
            {
                return true;
            }
        }
        
        return false;
    }
}
