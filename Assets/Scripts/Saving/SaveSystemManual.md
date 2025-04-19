# 게임 저장 시스템 사용 매뉴얼

이 문서는 현재 프로젝트의 게임 저장 시스템 사용 방법을 설명합니다. 시스템은 `ISaveable` 인터페이스, `UniqueID` 컴포넌트, `SaveData` 클래스, 그리고 `SaveManager` 싱글톤을 기반으로 합니다.

## 주요 구성 요소

1.  **`ISaveable` (인터페이스):**
    *   저장 및 로드가 필요한 모든 컴포넌트가 구현해야 하는 인터페이스입니다.
    *   `CaptureState()`: 객체의 현재 상태를 직렬화 가능한 데이터로 반환합니다.
    *   `RestoreState(object state)`: `CaptureState`에서 반환된 데이터로 객체의 상태를 복원합니다.
    *   `GetUniqueIdentifier()`: 객체를 식별하는 고유 ID를 반환합니다.

2.  **`UniqueID` (컴포넌트):**
    *   GameObject에 부착되어 세션 간에 유지되는 고유 ID를 제공합니다.
    *   에디터에서 GameObject에 추가하면 ID가 **자동으로 생성**되고 저장됩니다.
    *   `ISaveable` 구현 시 이 컴포넌트의 `ID` 프로퍼티를 `GetUniqueIdentifier()`에서 반환해야 합니다.

3.  **`SaveData` (클래스):**
    *   게임의 모든 저장 데이터를 담는 컨테이너 클래스입니다.
    *   `ISaveable` 객체들의 상태 데이터(`CaptureState` 결과)를 고유 ID를 키로 하는 딕셔너리(`objectStates`)에 저장합니다.
    *   `[Serializable]` 속성이 있어 직렬화가 가능합니다.

4.  **`SaveManager` (싱글톤):**
    *   게임 저장 및 로드 프로세스를 관리하는 중앙 관리자입니다.
    *   `SaveGameAsync()`: 현재 게임 상태를 비동기적으로 저장합니다. (JSON 직렬화 → Base64 인코딩 → 파일 저장)
    *   `LoadGameDataAsync()`: 저장된 데이터를 비동기적으로 로드합니다. (파일 읽기 → Base64 디코딩 → JSON 역직렬화)
    *   `RestoreLoadedData(SaveData data, RestoreStateResultCallback callback)`: `LoadGameDataAsync`로 로드된 데이터를 사용하여 게임 상태를 복원하고, 결과를 콜백으로 알립니다.
    *   `CacheAllSaveableObjects()`: 모든 `ISaveable` 객체를 찾아 캐싱하여 성능을 최적화합니다.

## 개선된 기능

최근 개선된 저장 시스템은 다음과 같은 추가 기능을 제공합니다:

1.  **스레드 안전성:**
    *   `SemaphoreSlim`을 사용한 스레드 안전 저장/로드 작업
    *   동시에 여러 저장/로드 요청을 안전하게 처리
    *   저장 중 로드, 또는 로드 중 저장에 대한 보호 메커니즘

2.  **오류 처리 및 재시도 로직:**
    *   저장/로드 실패 시 자동 재시도 (기본 3회)
    *   상세한 오류 로깅 및 예외 정보
    *   복원 실패 객체에 대한 추적 및 보고

3.  **파일 안전성:**
    *   임시 파일을 사용한 원자적 파일 쓰기 (저장 중 충돌 시 파일 손상 방지)
    *   자동 백업 파일 생성 및 복원 기능

4.  **성능 최적화:**
    *   `ISaveable` 객체 캐싱으로 `FindObjectsByType` 호출 최소화
    *   `SaveManager.CacheAllSaveableObjects()`를 씬 로드 후 호출하여 캐시 갱신

## 사용 방법

### 1. 컴포넌트를 저장 가능하게 만들기

게임 상태의 일부를 저장해야 하는 컴포넌트(예: 플레이어 체력, 인벤토리, 위치 등)는 다음 단계를 따릅니다.

1.  **`UniqueID` 컴포넌트 추가:** 해당 컴포넌트가 부착된 GameObject에 `UniqueID` 컴포넌트를 추가합니다. (에디터에서 ID 자동 생성)
2.  **`ISaveable` 인터페이스 구현:** 컴포넌트 스크립트에서 `ISaveable` 인터페이스를 구현합니다.
3.  **`GetUniqueIdentifier()` 구현:** `UniqueID` 컴포넌트를 참조하여 해당 ID를 반환합니다.
4.  **`CaptureState()` 구현:** 저장할 상태 데이터를 담는 **직렬화 가능한** 클래스 또는 구조체를 정의하고, 현재 상태를 이 객체에 담아 반환합니다.
5.  **`RestoreState(object state)` 구현:** `CaptureState`에서 반환한 타입의 객체로 `state` 파라미터를 캐스팅하고, 해당 데이터로 컴포넌트의 상태를 복원합니다.

**예시: `PlayerHealth.cs`**

```csharp
using UnityEngine;
// using Saving; // 네임스페이스가 있다면 추가

[RequireComponent(typeof(UniqueID))] // UniqueID 컴포넌트 강제
public class PlayerHealth : MonoBehaviour, ISaveable
{
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    private UniqueID uniqueID;

    private void Awake()
    {
        uniqueID = GetComponent<UniqueID>();
        if (uniqueID == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"UniqueID component missing on {gameObject.name}. Saving requires a UniqueID.", this);
#endif
        }
    }

    // --- ISaveable 구현 ---

    public string GetUniqueIdentifier()
    {
        // UniqueID 컴포넌트에서 ID 반환
        return uniqueID != null ? uniqueID.ID : "";
    }

    // 저장할 데이터 구조체 (직렬화 가능해야 함)
    [System.Serializable]
    private struct HealthData
    {
        public float health;
        // 필요하다면 다른 상태 추가
    }

    public object CaptureState()
    {
        // 현재 상태를 캡처하여 반환
        return new HealthData
        {
            health = currentHealth
        };
    }

    public void RestoreState(object state)
    {
        // state를 HealthData로 캐스팅하여 상태 복원
        if (state is HealthData healthData)
        {
            currentHealth = healthData.health;
            // 필요하다면 UI 업데이트 등 추가 작업
        }
        else
        {
#if UNITY_EDITOR
             Debug.LogError($"Incorrect state type provided for PlayerHealth on {gameObject.name}. Expected HealthData, got {state?.GetType()}", this);
#endif
        }
    }

    // --- 기타 체력 관련 로직 ---
    // ...
}
```

### 2. 게임 저장하기

게임 상태를 저장하려면 `SaveManager`의 `SaveGameAsync` 메서드를 호출합니다. 비동기 작업이므로 `async/await`를 사용하는 것이 좋습니다. 새 저장 시스템은 실패 시 최대 3회 자동 재시도 기능을 제공합니다.

```csharp
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI; // UI 요소를 위한 네임스페이스

public class GameController : MonoBehaviour
{
    // 옵션: UI 요소
    [SerializeField] private GameObject savingIndicator;
    [SerializeField] private Text statusText;

    public async void SaveGame()
    {
        // UI 로딩 표시
        if (savingIndicator != null) savingIndicator.SetActive(true);
        if (statusText != null) statusText.text = "저장 중...";
        
#if UNITY_EDITOR
        Debug.Log("게임 저장 시작...");
#endif

        bool success = await SaveManager.Instance.SaveGameAsync();

        // 결과에 따른 UI 처리
        if (success)
        {
            if (statusText != null) statusText.text = "저장 완료!";
#if UNITY_EDITOR
            Debug.Log("게임 저장 성공!");
#endif
        }
        else
        {
            if (statusText != null) statusText.text = "저장 실패!";
#if UNITY_EDITOR
            Debug.LogError("게임 저장에 실패했습니다!");
#endif
        }

        // UI 로딩 숨김 (딜레이 후)
        await Task.Delay(1000); // 1초 딜레이 (선택 사항)
        if (savingIndicator != null) savingIndicator.SetActive(false);
    }
}
```

### 3. 게임 로드하기

게임을 로드하려면 먼저 `LoadGameDataAsync`를 호출하여 `SaveData` 객체를 비동기적으로 가져온 다음, **메인 스레드에서** `RestoreLoadedData`를 호출하여 실제 상태를 복원합니다. 개선된 시스템에서는 결과 콜백을 제공하여 복원 성공/실패 여부를 알립니다.

```csharp
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI; // UI 요소를 위한 네임스페이스

public class GameController : MonoBehaviour
{
    // 옵션: UI 요소
    [SerializeField] private GameObject loadingIndicator;
    [SerializeField] private Text statusText;

    public async void LoadGame()
    {
        // UI 로딩 표시
        if (loadingIndicator != null) loadingIndicator.SetActive(true);
        if (statusText != null) statusText.text = "로드 중...";
        
#if UNITY_EDITOR
        Debug.Log("게임 데이터 로드 시작...");
#endif

        SaveData loadedData = await SaveManager.Instance.LoadGameDataAsync();

        if (loadedData != null)
        {
            // 씬 전환 등이 필요하다면 여기서 처리
            // ...

            // 메인 스레드에서 상태 복원 실행 (콜백으로 결과 처리)
            SaveManager.Instance.RestoreLoadedData(loadedData, (success, errorMessage) => 
            {
                if (success)
                {
                    if (statusText != null) statusText.text = "로드 완료!";
#if UNITY_EDITOR
                    Debug.Log("게임이 성공적으로 로드되고 복원되었습니다!");
#endif
                }
                else
                {
                    if (statusText != null) statusText.text = $"로드 일부 실패! {errorMessage}";
#if UNITY_EDITOR
                    Debug.LogWarning($"게임 상태 복원 일부 실패: {errorMessage}");
#endif
                }
            });
        }
        else
        {
            if (statusText != null) statusText.text = "로드 실패 또는 저장 데이터 없음";
#if UNITY_EDITOR
            Debug.LogWarning("게임 데이터를 로드하는 데 실패했거나 저장 파일을 찾을 수 없습니다.");
#endif
            // 로드 실패 처리 (예: 새 게임 시작)
        }

        // UI 로딩 숨김 (딜레이 후)
        await Task.Delay(1000); // 1초 딜레이 (선택 사항)
        if (loadingIndicator != null) loadingIndicator.SetActive(false);
    }
}
```

### 4. 성능 최적화를 위한 캐싱

씬을 로드한 후나 새로운 저장 가능 객체가 생성 또는 파괴된 경우 `ISaveable` 객체 캐시를 업데이트해야 합니다. 이는 성능 최적화에 중요합니다.

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystemInitializer : MonoBehaviour
{
    private void Start()
    {
        // 시작 시 캐싱 수행
        SaveManager.Instance.CacheAllSaveableObjects();
    }

    // 씬 전환 완료 후 캐싱 새로고침
    private void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드 후 캐시 리셋
        SaveManager.Instance.ResetCache();
        // 새 씬에서 캐싱 수행
        SaveManager.Instance.CacheAllSaveableObjects();
    }
}
```

## 저장 파일 정보

*   **형식:** 데이터는 Newtonsoft.Json으로 직렬화된 후 Base64로 인코딩되어 저장됩니다.
*   **위치:** `Application.persistentDataPath` 폴더 내에 `gameSave.sav` 파일로 저장됩니다.
*   **백업:** 저장 시 자동으로 `gameSave.sav.bak` 백업 파일이 생성됩니다.
*   **로그:** 저장/로드 관련 상세 로그는 Unity 에디터에서만 출력됩니다 (`#if UNITY_EDITOR`).

## 스레드 안전성

개선된 저장 시스템은 `SemaphoreSlim`을 사용하여 다음과 같은 스레드 안전 기능을 제공합니다:

*   동시에 여러 저장 요청이 들어오더라도 한 번에 하나만 처리
*   동시에 여러 로드 요청이 들어오더라도 한 번에 하나만 처리
*   저장 중 로드 또는 로드 중 저장 작업을 안전하게 처리

이러한 동기화는 UI 버튼을 빠르게 여러 번 클릭하거나, 자동 저장과 수동 저장이 동시에 시도되는 경우에도 데이터 손상을 방지합니다.

## 오류 처리 및 재시도

*   **자동 재시도:** 저장/로드 실패 시 최대 3회까지 자동으로 재시도합니다. (간격: 500ms)
*   **백업 활용:** 메인 저장 파일 로드에 실패하면 백업 파일에서 복원을 시도합니다.
*   **복원 콜백:** `RestoreLoadedData`에 콜백을 제공하여 복원 결과를 처리할 수 있습니다.
*   **실패 추적:** 복원에 실패한 객체들의 ID 목록을 제공합니다.

## 주의사항

*   `ISaveable`을 구현하는 모든 GameObject에는 `UniqueID` 컴포넌트가 반드시 필요합니다.
*   `CaptureState`에서 반환하는 객체는 반드시 `[System.Serializable]` 속성을 가지거나 Newtonsoft.Json이 직렬화할 수 있는 타입이어야 합니다.
*   복잡한 참조(예: 다른 MonoBehaviour) 대신 ID를 저장하는 것을 권장합니다.
*   런타임에 동적으로 생성되는 `ISaveable` 객체는 `UniqueID`가 자동으로 생성되지 않으므로, 생성 시점에 고유 ID를 할당하고 관리하는 별도의 로직이 필요할 수 있습니다.
*   `RestoreLoadedData`는 캐시된 `ISaveable` 객체를 사용하여 상태를 복원하므로, 로드 시점에 필요한 모든 객체가 씬에 로드되어 있어야 합니다.
*   씬 전환 후에는 반드시 `SaveManager.Instance.CacheAllSaveableObjects()`를 호출하여 캐시를 새로고침해야 합니다.
