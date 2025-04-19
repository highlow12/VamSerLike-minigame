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
    *   `SaveGameAsync()`: 현재 게임 상태를 비동기적으로 저장합니다. (JSON 직렬화 -> Base64 인코딩 -> 파일 저장)
    *   `LoadGameDataAsync()`: 저장된 데이터를 비동기적으로 로드합니다. (파일 읽기 -> Base64 디코딩 -> JSON 역직렬화)
    *   `RestoreLoadedData(SaveData data)`: `LoadGameDataAsync`로 로드된 데이터를 사용하여 게임 상태를 복원합니다. (메인 스레드에서 호출 필요)

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

게임 상태를 저장하려면 `SaveManager`의 `SaveGameAsync` 메서드를 호출합니다. 비동기 작업이므로 `async/await`를 사용하는 것이 좋습니다.

```csharp
using UnityEngine;
using System.Threading.Tasks;

public class GameController : MonoBehaviour
{
    public async void SaveGame()
    {
#if UNITY_EDITOR
        Debug.Log("Saving game...");
#endif
        // UI 로딩 표시 등

        await SaveManager.Instance.SaveGameAsync();

        // UI 로딩 숨김 등
#if UNITY_EDITOR
        Debug.Log("Game saved!");
#endif
    }
}
```

### 3. 게임 로드하기

게임을 로드하려면 먼저 `LoadGameDataAsync`를 호출하여 `SaveData` 객체를 비동기적으로 가져온 다음, **메인 스레드에서** `RestoreLoadedData`를 호출하여 실제 상태를 복원합니다.

```csharp
using UnityEngine;
using System.Threading.Tasks;

public class GameController : MonoBehaviour
{
    public async void LoadGame()
    {
#if UNITY_EDITOR
        Debug.Log("Loading game data...");
#endif
        // UI 로딩 표시 등

        SaveData loadedData = await SaveManager.Instance.LoadGameDataAsync();

        if (loadedData != null)
        {
            // 씬 전환 등이 필요하다면 여기서 처리
            // ...

            // 메인 스레드에서 상태 복원 실행
            SaveManager.Instance.RestoreLoadedData(loadedData);
#if UNITY_EDITOR
            Debug.Log("Game loaded and restored!");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("Failed to load game data or no save file found.");
#endif
            // 로드 실패 처리 (예: 새 게임 시작)
        }

        // UI 로딩 숨김 등
    }
}
```

## 저장 파일 정보

*   **형식:** 데이터는 JSON으로 직렬화된 후 Base64로 인코딩되어 저장됩니다. (간단한 난독화)
*   **위치:** `Application.persistentDataPath` 폴더 내에 `gameSave.sav` 파일로 저장됩니다. (경로는 `SaveManager.cs`에서 변경 가능)
*   **로그:** 저장/로드 관련 상세 로그는 Unity 에디터에서만 출력됩니다 (`#if UNITY_EDITOR`).

## 주의사항

*   `ISaveable`을 구현하는 모든 GameObject에는 `UniqueID` 컴포넌트가 반드시 필요합니다.
*   `CaptureState`에서 반환하는 객체는 반드시 `[System.Serializable]` 속성을 가지거나 Newtonsoft.Json이 직렬화할 수 있는 타입이어야 합니다. 복잡한 참조(예: 다른 MonoBehaviour) 대신 ID를 저장하는 것을 권장합니다.
*   런타임(게임 실행 중)에 동적으로 생성되는 `ISaveable` 객체는 `UniqueID`가 자동으로 생성되지 않으므로, 생성 시점에 고유 ID를 할당하고 관리하는 별도의 로직이 필요할 수 있습니다.
*   `RestoreLoadedData`는 씬의 모든 `ISaveable` 객체를 찾아서 상태를 복원하므로, 로드 시점에 필요한 모든 객체가 씬에 로드되어 있어야 합니다.
