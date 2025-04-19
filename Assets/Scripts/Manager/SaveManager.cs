using UnityEngine;
using System.Collections.Generic;
using System.IO;
// using System.Runtime.Serialization.Formatters.Binary; // 더 이상 사용 안 함
using System.Linq;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json; // Newtonsoft.Json 사용
using System.Text;     // Encoding (UTF8, Base64) 사용
using System.Threading; // SemaphoreSlim 사용

public class SaveManager : Singleton<SaveManager>
{
    // 파일 확장자 변경 (선택 사항)
    private string saveFileName = "gameSave.sav";
    
    // SemaphoreSlim을 사용한 스레드 안전 동기화
    private readonly SemaphoreSlim saveLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim loadLock = new SemaphoreSlim(1, 1);
    
    // 연산 상태 추적
    private bool isSaving = false;
    private bool isLoading = false;
    
    // 저장 가능한 객체 캐시, Awake 또는 씬 로드 시 초기화
    private Dictionary<string, ISaveable> saveableObjectsCache = null;
    
    // 재시도 설정
    private const int MaxRetryCount = 3;
    private const int RetryDelayMs = 500; // 500ms
    
    // 상태 복원 결과를 위한 델리게이트
    public delegate void RestoreStateResultCallback(bool success, string errorMessage);

    // Newtonsoft.Json 설정 (타입 정보 포함, 들여쓰기 없음)
    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto, // 객체 타입 정보 포함 (중요!)
        Formatting = Formatting.None // 파일 크기 줄이기 위해 들여쓰기 없음
        // 필요에 따라 다른 설정 추가 가능 (ReferenceLoopHandling 등)
    };

    private void Awake()
    {
        // 싱글톤 초기화 이후에 캐시 초기화
        CacheAllSaveableObjects();
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }
    
    /// <summary>
    /// 현재 씬에서 모든 ISaveable 객체를 찾아 캐싱합니다.
    /// 씬 로드 후나 객체 풀 변경 후 호출해야 합니다.
    /// </summary>
    public void CacheAllSaveableObjects()
    {
#if UNITY_EDITOR
        Debug.Log("ISaveable 객체 캐싱 중...");
#endif
        // 새 딕셔너리 생성
        Dictionary<string, ISaveable> newCache = new Dictionary<string, ISaveable>();
        
        // 모든 ISaveable 객체 찾기 (이 작업은 비용이 많이 들지만 한 번만 수행)
        IEnumerable<ISaveable> saveableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISaveable>();
            
        foreach (ISaveable saveable in saveableObjects)
        {
            string id = saveable.GetUniqueIdentifier();
            if (!string.IsNullOrEmpty(id))
            {
                if (!newCache.ContainsKey(id))
                {
                    newCache.Add(id, saveable);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"중복된 고유 식별자 발견: {id} (GameObject: '{((MonoBehaviour)saveable).gameObject.name}'). " +
                                    "캐싱 중 충돌이 발생했습니다.", (MonoBehaviour)saveable);
#endif
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"GameObject '{((MonoBehaviour)saveable).gameObject.name}'에 저장용 고유 식별자가 없습니다.", 
                                (MonoBehaviour)saveable);
#endif
            }
        }
        
        // 캐시 교체
        saveableObjectsCache = newCache;
        
#if UNITY_EDITOR
        Debug.Log($"{saveableObjectsCache.Count}개의 ISaveable 객체가 캐싱되었습니다.");
#endif
    }

    /// <summary>
    /// 게임 상태를 비동기적으로 JSON으로 직렬화하고 Base64 인코딩하여 저장합니다.
    /// 최대 MaxRetryCount번 재시도합니다.
    /// </summary>
    /// <returns>성공 여부를 나타내는 Task<bool></returns>
    public async Task<bool> SaveGameAsync()
    {
        // SemaphoreSlim으로 스레드 안전성 확보
        await saveLock.WaitAsync();
        
        try
        {
            if (isSaving)
            {
#if UNITY_EDITOR
                Debug.LogWarning("저장이 이미 진행 중입니다.");
#endif
                return false;
            }
            isSaving = true;
            
#if UNITY_EDITOR
            Debug.Log("비동기 저장 시작 (JSON + Base64)...");
#endif

            // 캐시가 비어 있으면 초기화
            if (saveableObjectsCache == null || saveableObjectsCache.Count == 0)
            {
                CacheAllSaveableObjects();
            }

            // 1. (메인 스레드) 저장할 데이터 캡처
            SaveData saveData = new SaveData();
            saveData.lastSaved = DateTime.UtcNow;

            foreach (var kvp in saveableObjectsCache)
            {
                string id = kvp.Key;
                ISaveable saveable = kvp.Value;
                
                try
                {
                    saveData.objectStates[id] = saveable.CaptureState();
                }
                catch (Exception e)
                {
#if UNITY_EDITOR
                    Debug.LogError($"객체 {id} ({((MonoBehaviour)saveable).gameObject.name})의 상태 캡처 오류: {e.Message}\n{e.StackTrace}", (MonoBehaviour)saveable);
#endif
                }
            }

            // 2. (백그라운드 스레드) JSON 직렬화, Base64 인코딩 및 파일 쓰기
            bool success = false;
            Exception lastException = null;
            
            for (int attempt = 0; attempt < MaxRetryCount && !success; attempt++)
            {
                if (attempt > 0)
                {
#if UNITY_EDITOR
                    Debug.Log($"저장 재시도 중... (시도 {attempt + 1}/{MaxRetryCount})");
#endif
                    // 재시도 전 잠시 대기
                    await Task.Delay(RetryDelayMs);
                }
                
                try
                {
                    string path = GetSavePath();
                    await Task.Run(async () =>
                    {
                        // 임시 파일에 먼저 쓰기 (파일 손상 방지)
                        string tempPath = path + ".tmp";
                        
                        // Newtonsoft.Json으로 직렬화
                        string jsonString = JsonConvert.SerializeObject(saveData, jsonSettings);

                        // JSON 문자열을 Base64로 인코딩
                        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                        string base64String = Convert.ToBase64String(jsonBytes);

                        // Base64 문자열을 임시 파일에 비동기적으로 쓰기
                        using (StreamWriter writer = new StreamWriter(tempPath, false, Encoding.UTF8)) // UTF8 인코딩 명시
                        {
                            await writer.WriteAsync(base64String);
                        }
                        
                        // 기존 파일이 있으면 백업 만들기
                        if (File.Exists(path))
                        {
                            string backupPath = path + ".bak";
                            if (File.Exists(backupPath))
                            {
                                File.Delete(backupPath);
                            }
                            File.Move(path, backupPath);
                        }
                        
                        // 임시 파일을 실제 파일로 이동 (원자적 작업)
                        File.Move(tempPath, path);
                    });
                    
                    success = true;
#if UNITY_EDITOR
                    Debug.Log($"게임이 {path}에 비동기적으로 저장되었습니다 (JSON + Base64).");
#endif
                }
                catch (Exception e)
                {
                    lastException = e;
#if UNITY_EDITOR
                    Debug.LogWarning($"저장 시도 {attempt + 1}/{MaxRetryCount} 실패: {e.Message}");
#endif
                }
            }
            
            if (!success && lastException != null)
            {
#if UNITY_EDITOR
                Debug.LogError($"모든 저장 시도 실패 ({MaxRetryCount}회): {lastException.Message}\n{lastException.StackTrace}");
#endif
            }
            
            return success;
        }
        finally
        {
            isSaving = false;
            saveLock.Release();
        }
    }

    /// <summary>
    /// Base64로 인코딩된 JSON 파일을 비동기적으로 로드하고 역직렬화하여 SaveData 객체를 반환합니다.
    /// 최대 MaxRetryCount번 재시도합니다.
    /// </summary>
    /// <returns>로드된 SaveData 객체 또는 로드 실패 시 null.</returns>
    public async Task<SaveData> LoadGameDataAsync()
    {
        // SemaphoreSlim으로 스레드 안전성 확보
        await loadLock.WaitAsync();
        
        try
        {
            if (isLoading)
            {
#if UNITY_EDITOR
                Debug.LogWarning("로드가 이미 진행 중입니다.");
#endif
                return null;
            }
            isLoading = true;
            
#if UNITY_EDITOR
            Debug.Log("비동기 로드 시작 (JSON + Base64)...");
#endif

            string path = GetSavePath();
            if (!File.Exists(path))
            {
                // 백업 파일이 있는지 확인
                string backupPath = path + ".bak";
                if (File.Exists(backupPath))
                {
#if UNITY_EDITOR
                    Debug.Log("주 저장 파일을 찾을 수 없습니다. 백업에서 복원 중...");
#endif
                    File.Copy(backupPath, path, true);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("저장 파일을 찾을 수 없습니다.");
#endif
                    return null;
                }
            }

            SaveData saveData = null;
            Exception lastException = null;
            
            for (int attempt = 0; attempt < MaxRetryCount && saveData == null; attempt++)
            {
                if (attempt > 0)
                {
#if UNITY_EDITOR
                    Debug.Log($"로드 재시도 중... (시도 {attempt + 1}/{MaxRetryCount})");
#endif
                    // 재시도 전 잠시 대기
                    await Task.Delay(RetryDelayMs);
                }
                
                try
                {
                    // (백그라운드 스레드) 파일 읽기, Base64 디코딩, JSON 역직렬화
                    saveData = await Task.Run(async () =>
                    {
                        string base64String;
                        // Base64 문자열을 파일에서 비동기적으로 읽기
                        using (StreamReader reader = new StreamReader(path, Encoding.UTF8)) // UTF8 인코딩 명시
                        {
                            base64String = await reader.ReadToEndAsync();
                        }

                        // Base64 문자열을 JSON 바이트로 디코딩
                        byte[] jsonBytes = Convert.FromBase64String(base64String);
                        // JSON 바이트를 문자열로 변환
                        string jsonString = Encoding.UTF8.GetString(jsonBytes);

                        // Newtonsoft.Json으로 역직렬화
                        return JsonConvert.DeserializeObject<SaveData>(jsonString, jsonSettings);
                    });
                    
                    if (saveData != null)
                    {
#if UNITY_EDITOR
                        Debug.Log($"게임 데이터가 {path}에서 비동기적으로 로드되었습니다 (JSON + Base64).");
#endif
                    }
                }
                catch (FormatException fe) // Base64 디코딩 실패
                {
                    lastException = fe;
#if UNITY_EDITOR
                    Debug.LogWarning($"Base64 디코딩 실패 (시도 {attempt + 1}/{MaxRetryCount}): {fe.Message}");
#endif
                    // 백업 파일로 시도
                    if (attempt == MaxRetryCount - 1 && File.Exists(path + ".bak"))
                    {
#if UNITY_EDITOR
                        Debug.Log("메인 파일에서 로드 실패, 백업에서 시도 중...");
#endif
                        path = path + ".bak";
                    }
                }
                catch (JsonException je) // JSON 역직렬화 실패
                {
                    lastException = je;
#if UNITY_EDITOR
                    Debug.LogWarning($"JSON 역직렬화 실패 (시도 {attempt + 1}/{MaxRetryCount}): {je.Message}");
#endif
                    // 백업 파일로 시도
                    if (attempt == MaxRetryCount - 1 && File.Exists(path + ".bak"))
                    {
#if UNITY_EDITOR
                        Debug.Log("메인 파일에서 로드 실패, 백업에서 시도 중...");
#endif
                        path = path + ".bak";
                    }
                }
                catch (Exception e) // 기타 예외
                {
                    lastException = e;
#if UNITY_EDITOR
                    Debug.LogWarning($"로드 실패 (시도 {attempt + 1}/{MaxRetryCount}): {e.Message}");
#endif
                }
            }
            
            if (saveData == null && lastException != null)
            {
#if UNITY_EDITOR
                Debug.LogError($"모든 로드 시도 실패 ({MaxRetryCount}회): {lastException.Message}\n{lastException.StackTrace}");
#endif
            }
            
            return saveData;
        }
        finally
        {
            isLoading = false;
            loadLock.Release();
        }
    }

    /// <summary>
    /// 로드된 SaveData를 사용하여 게임 상태를 복원합니다. (메인 스레드에서 호출해야 함)
    /// 복원 결과를 선택적으로 콜백을 통해 알립니다.
    /// </summary>
    /// <param name="saveData">LoadGameDataAsync에서 반환된 SaveData 객체.</param>
    /// <param name="callback">복원 결과를 받을 선택적 콜백.</param>
    public void RestoreLoadedData(SaveData saveData, RestoreStateResultCallback callback = null)
    {
        if (saveData == null)
        {
#if UNITY_EDITOR
            Debug.LogError("null SaveData로부터 상태를 복원할 수 없습니다.");
#endif
            callback?.Invoke(false, "복원할 저장 데이터가 없습니다.");
            return;
        }

#if UNITY_EDITOR
        Debug.Log("로드된 데이터로부터 게임 상태 복원 중...");
#endif

        // (메인 스레드) ISaveable 객체 찾기 및 상태 복원
        // 캐시가 비어 있으면 초기화
        if (saveableObjectsCache == null || saveableObjectsCache.Count == 0)
        {
            CacheAllSaveableObjects();
        }

        List<string> failedObjects = new List<string>();
        int restoredCount = 0;
        
        foreach (var kvp in saveData.objectStates)
        {
            string id = kvp.Key;
            if (saveableObjectsCache.TryGetValue(id, out ISaveable saveable))
            {
                try
                {
                    // RestoreState는 여전히 object를 받으므로, Newtonsoft가 올바른 타입으로
                    // 역직렬화했는지 확인하는 것이 중요 (TypeNameHandling 설정 덕분에 가능)
                    saveable.RestoreState(kvp.Value);
                    restoredCount++;
                }
                catch (Exception e)
                {
#if UNITY_EDITOR
                    Debug.LogError($"객체 {id} ({((MonoBehaviour)saveable).gameObject.name})의 상태 복원 오류: {e.Message}\n{e.StackTrace}", (MonoBehaviour)saveable);
#endif
                    failedObjects.Add(id);
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"현재 씬에서 고유 식별자 '{id}'를 가진 객체를 찾을 수 없어 상태를 복원할 수 없습니다.");
#endif
                failedObjects.Add(id);
            }
        }

        bool isFullSuccess = failedObjects.Count == 0;
        string errorMessage = isFullSuccess ? "" : $"{failedObjects.Count}개의 객체 복원 실패";
        
#if UNITY_EDITOR
        Debug.Log($"게임 상태가 복원되었습니다. 성공: {restoredCount}, 실패: {failedObjects.Count}");
#endif

        // 결과 콜백 호출
        callback?.Invoke(isFullSuccess, errorMessage);
    }

    // 씬 변경 시 캐시 리셋 (예: 씬 이벤트 리스너)
    public void ResetCache()
    {
        saveableObjectsCache = null;
    }

    // Application 종료 시 리소스 해제
    private void OnDestroy()
    {
        saveLock.Dispose();
        loadLock.Dispose();
    }

    // --- 사용 예시 (다른 스크립트에서 호출) ---
    /*
    public async void HandleSaveButtonClicked()
    {
        // UI 비활성화 또는 로딩 인디케이터 표시
#if UNITY_EDITOR
        Debug.Log("저장 버튼 클릭됨. 저장 시작...");
#endif
        bool success = await SaveManager.Instance.SaveGameAsync();
        if (success)
        {
            // 저장 성공 메시지 표시
        }
        else
        {
            // 저장 실패 메시지 표시
        }
        // 저장 완료 후 UI 활성화 또는 인디케이터 숨김
#if UNITY_EDITOR
        Debug.Log("저장 작업 완료.");
#endif
    }

    public async void HandleLoadButtonClicked()
    {
        // UI 비활성화 또는 로딩 인디케이터 표시
#if UNITY_EDITOR
        Debug.Log("로드 버튼 클릭됨. 로드 시작...");
#endif
        SaveData loadedData = await SaveManager.Instance.LoadGameDataAsync();
        if (loadedData != null)
        {
            // 씬 전환 등이 필요하다면 여기서 처리
            // ...

            // 데이터 로드가 완료된 후, 메인 스레드에서 상태 복원
            SaveManager.Instance.RestoreLoadedData(loadedData, (success, error) => {
                if (success)
                {
                    // 복원 성공 메시지 표시
#if UNITY_EDITOR
                    Debug.Log("로드 및 복원 작업 완료.");
#endif
                }
                else
                {
                    // 복원 실패 또는 일부 실패 메시지 표시
#if UNITY_EDITOR
                    Debug.LogWarning($"복원 작업 일부 실패: {error}");
#endif
                }
            });
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("로드 작업 실패 또는 저장 데이터 없음.");
#endif
            // 로드 실패 처리 (예: 새 게임 시작)
        }
        // 로드 완료/실패 후 UI 활성화 또는 인디케이터 숨김
    }
    */
}
