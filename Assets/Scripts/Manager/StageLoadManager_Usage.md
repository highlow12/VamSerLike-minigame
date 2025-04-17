# StageLoadManager 사용 가이드

## 개요
`StageLoadManager`는 Unity 씬을 비동기적으로 로딩하기 위한 싱글톤 클래스입니다. 이 클래스를 사용하면 게임 내에서 씬 전환 시 로딩 화면을 표시하고 로딩 진행 상황을 추적할 수 있습니다.

## 주요 기능
- 비동기적 씬 로딩
- 로딩 화면을 통한 씬 전환
- 로딩 진행 상황 추적 및 콜백 제공
- 씬 유효성 검사

## 기본 사용법

### 인스턴스 접근하기
`StageLoadManager`는 싱글톤 패턴으로 구현되어 있어 다음과 같이 접근할 수 있습니다:

```csharp
StageLoadManager.Instance
```

### 씬 로딩하기

#### 기본 씬 로딩 (로딩 화면 없이)
```csharp
StageLoadManager.Instance.LoadSceneAsync("GameScene");
```

#### 로딩 화면을 사용한 씬 로딩
```csharp
// 기본 "LoadingScene"을 사용
StageLoadManager.Instance.LoadSceneAsync("GameScene", true);

// 사용자 지정 로딩 씬 사용
StageLoadManager.Instance.LoadSceneAsync("GameScene", true, "MyCustomLoadingScene");
```

### 로딩 진행 상황 추적하기

#### 로딩 진행률 표시하기
로딩 진행 상황을 UI에 표시하려면 다음과 같이 콜백을 등록합니다:

```csharp
// 로딩 진행 상황을 받아 슬라이더에 표시
StageLoadManager.Instance.AddLoadingProgressCallback((progress) => {
    loadingSlider.value = progress; // 0.0 ~ 1.0 사이의 값
    loadingPercentText.text = (progress * 100).ToString("F0") + "%";
});
```

#### 로딩 완료 시 작업 수행하기
로딩이 완료된 후 특정 작업을 수행하려면 다음과 같이 콜백을 등록합니다:

```csharp
StageLoadManager.Instance.AddLoadingCompletedCallback(() => {
    Debug.Log("씬 로딩 완료!");
    // 로딩 완료 후 수행할 작업
    gameManager.StartGame();
});
```

### 로딩 상태 확인하기

현재 로딩 중인지 확인:
```csharp
if (StageLoadManager.Instance.IsLoading)
{
    // 로딩 중인 경우
    playerInput.enabled = false;
}
```

현재 로딩 진행률 확인:
```csharp
float currentProgress = StageLoadManager.Instance.LoadingProgress;
Debug.Log($"현재 로딩 진행률: {currentProgress * 100}%");
```

### 씬 유효성 검사
존재하지 않는 씬을 로드하려고 할 때 오류를 방지하기 위해 씬 이름의 유효성을 검사할 수 있습니다:

```csharp
if (StageLoadManager.Instance.IsSceneValid("MyScene"))
{
    StageLoadManager.Instance.LoadSceneAsync("MyScene", true);
}
else
{
    Debug.LogError("존재하지 않는 씬입니다: MyScene");
}
```

## 구현 예시

### 간단한 씬 전환 매니저 예시

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Text loadingPercentText;
    [SerializeField] private Button startButton;
    
    private void Start()
    {
        // 시작 버튼 클릭 시 게임 씬으로 이동
        startButton.onClick.AddListener(() => {
            LoadGameScene();
        });
    }
    
    private void LoadGameScene()
    {
        // 로딩 진행 상황 콜백 등록
        StageLoadManager.Instance.AddLoadingProgressCallback(UpdateLoadingUI);
        
        // 로딩 완료 콜백 등록
        StageLoadManager.Instance.AddLoadingCompletedCallback(OnLoadingCompleted);
        
        // 로딩 화면을 사용하여 게임 씬 로드
        StageLoadManager.Instance.LoadSceneAsync("GameScene", true);
    }
    
    private void UpdateLoadingUI(float progress)
    {
        // 로딩 UI 업데이트
        loadingSlider.value = progress;
        loadingPercentText.text = (progress * 100).ToString("F0") + "%";
    }
    
    private void OnLoadingCompleted()
    {
        Debug.Log("게임 씬 로딩 완료!");
    }
}
```

### 로딩 화면 구현 예시

```csharp
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private Text tipText;
    
    [SerializeField] private string[] loadingTips;
    
    private void Start()
    {
        // 랜덤 팁 표시
        if (loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }
        
        // 로딩 진행 상황 업데이트 콜백 등록
        StageLoadManager.Instance.AddLoadingProgressCallback(UpdateProgressBar);
    }
    
    private void UpdateProgressBar(float progress)
    {
        progressBar.value = progress;
        progressText.text = (progress * 100).ToString("F0") + "%";
    }
}
```

## 주의사항

1. `StageLoadManager`는 싱글톤으로 구현되어 있으므로 씬이 변경되어도 유지됩니다.
2. 로딩 화면을 사용할 경우, 해당 로딩 씬에 UI 요소(슬라이더, 텍스트 등)를 구현해야 합니다.
3. `LoadSceneAsync` 호출 전에 콜백을 등록하는 것을 권장합니다.
4. 로딩 씬은 빌드 설정(Build Settings)에 포함되어 있어야 합니다.

## 씬 관리 팁

1. 씬 이름을 상수로 관리하는 것을 권장합니다:
   ```csharp
   public static class SceneNames
   {
       public const string MAIN_MENU = "MainMenu";
       public const string LOADING = "LoadingScene";
       public const string GAME = "GameScene";
   }
   ```

2. 대규모 씬의 경우 `LoadSceneMode.Additive`를 사용하여 씬을 추가로 로드하는 것을 고려하세요.

---

## 싱글톤 패턴 활용 방법

`StageLoadManager`는 `Singleton<T>` 클래스를 상속받아 구현되었습니다. 다음은 싱글톤 패턴을 활용하는 방법입니다:

1. 새로운 매니저 클래스 생성 시 `Singleton<T>`를 상속받습니다:
   ```csharp
   public class MyManager : Singleton<MyManager>
   {
       // 구현 내용
   }
   ```

2. 씬이 변경되어도 객체를 유지하고 싶다면 `Awake` 메서드를 오버라이드하고 base.Awake()를 호출하세요:
   ```csharp
   public override void Awake()
   {
       base.Awake(); // DontDestroyOnLoad 호출
       // 추가 초기화 코드
   }
   ```

3. 싱글톤 인스턴스 접근 방법:
   ```csharp
   MyManager.Instance.DoSomething();
   ```