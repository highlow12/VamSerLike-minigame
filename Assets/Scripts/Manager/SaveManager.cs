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
using CustomEncryption; // Rijndael 암호화 사용
using LitJson; // LitJson 사용

public class SaveManager : Singleton<SaveManager>
{
    // 파일 확장자 변경
    private string saveFileName = "gameSave.enc"; // 암호화된 파일임을 나타내는 확장자로 변경
    
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

    // 암호화 키 (LocalDataManager와 동일한 키 사용)
    private const string EncryptionKey = "EnRBcwL791f3oEf/AH2D0D2EhbajQ0yBimSUbLHDTA8=";
    
    // 상태 복원 결과를 위한 델리게이트
    public delegate void RestoreStateResultCallback(bool success, string errorMessage);

    // 특수 키 접두사 (내부 관리 데이터를 식별하기 위한 용도)
    private const string InternalKeyPrefix = "LocalPlayer";
    
    // 저장 시 데이터 정리 여부 설정 (퍼포먼스 최적화를 위해 비활성화할 수 있음)
    [SerializeField] private bool cleanupSaveDataOnSave = true;

    // Newtonsoft.Json 설정 (타입 정보 포함, 들여쓰기 없음)
    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto, // 객체 타입 정보 포함 (중요!)
        Formatting = Formatting.None // 파일 크기 줄이기 위해 들여쓰기 없음
        // 필요에 따라 다른 설정 추가 가능 (ReferenceLoopHandling 등)
    };

    public override void Awake()
    {
        // 싱글톤 초기화 이후에 캐시 초기화
        EnsureCacheInitialized();
    }

    private string GetSavePath()
    {
        string saveDir = Path.Combine(Application.persistentDataPath, "SaveData");
        
        // 디렉토리가 없으면 생성
        if (!Directory.Exists(saveDir))
        {
            Directory.CreateDirectory(saveDir);
        }
        
        return Path.Combine(saveDir, saveFileName);
    }

    /// <summary>
    /// 백업 파일 경로를 반환합니다.
    /// </summary>
    private string GetBackupPath()
    {
        return GetSavePath() + ".bak";
    }
    
    /// <summary>
    /// saveableObjectsCache가 초기화되었는지 확인하고, 초기화되지 않았다면 초기화합니다.
    /// 이 메서드는 중복 검사를 방지하기 위한 유틸리티 함수입니다.
    /// </summary>
    private void EnsureCacheInitialized()
    {
        if (saveableObjectsCache == null || saveableObjectsCache.Count == 0)
        {
            CacheAllSaveableObjects();
        }
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
    /// 저장 데이터에서 더 이상 게임에 존재하지 않는 객체의 상태를 정리합니다.
    /// 내부 관리 데이터(InternalKeyPrefix로 시작하는 키)는 정리하지 않습니다.
    /// </summary>
    /// <param name="saveData">정리할 SaveData 객체</param>
    /// <returns>제거된 항목 수</returns>
    private int CleanupOrphanedData(SaveData saveData)
    {
        if (saveData == null || saveData.objectStates == null || saveableObjectsCache == null)
        {
            return 0;
        }

        int removedCount = 0;
        List<string> keysToRemove = new List<string>();

        // 제거할 키 식별
        foreach (string key in saveData.objectStates.Keys)
        {
            // 내부 관리 데이터는 보존 (LocalPlayer로 시작하는 키 등)
            if (key.StartsWith(InternalKeyPrefix))
            {
                continue;
            }

            // 캐시에 없는 객체의 데이터는 제거 대상
            if (!saveableObjectsCache.ContainsKey(key))
            {
                keysToRemove.Add(key);
            }
        }

        // 식별된 키 제거
        foreach (string key in keysToRemove)
        {
            saveData.objectStates.Remove(key);
            removedCount++;
        }

#if UNITY_EDITOR
        if (removedCount > 0)
        {
            Debug.Log($"저장 데이터 정리: {removedCount}개의 고아 객체 상태가 제거되었습니다.");
        }
#endif

        return removedCount;
    }

    /// <summary>
    /// 게임 상태를 비동기적으로 JSON으로 직렬화하고 Rijndael 암호화를 사용하여 안전하게 저장합니다.
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
            Debug.Log("비동기 저장 시작 (JSON + Rijndael 암호화)...");
#endif

            // 캐시 초기화 확인 (중복 검사 대신 유틸리티 함수 사용)
            EnsureCacheInitialized();

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

            // 2. (백그라운드 스레드) JSON 직렬화, 암호화 및 파일 쓰기
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
                    await Task.Run(() =>
                    {
                        // 임시 파일에 먼저 쓰기 (파일 손상 방지)
                        string tempPath = path + ".tmp";
                        
                        // Newtonsoft.Json으로 직렬화
                        string jsonString = JsonConvert.SerializeObject(saveData, jsonSettings);

                        // LocalDataManager의 암호화 메서드 활용
                        byte[] encryptedBytes = Rijndael.EncryptString(jsonString, EncryptionKey);

                        // 암호화된 데이터를 임시 파일에 쓰기
                        File.WriteAllBytes(tempPath, encryptedBytes);
                        
                        // 기존 파일이 있으면 백업 만들기
                        if (File.Exists(path))
                        {
                            string backupPath = GetBackupPath();
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
                    Debug.Log($"게임이 {path}에 암호화되어 저장되었습니다.");
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
    /// 주어진 경로에서 암호화된 파일을 로드하는 시도를 합니다.
    /// </summary>
    /// <param name="path">로드할 파일 경로</param>
    /// <returns>로드된 SaveData 객체 또는 실패 시 null</returns>
    private async Task<SaveData> TryLoadFromPath(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            // 파일 읽기, 복호화, JSON 역직렬화
            return await Task.Run(() =>
            {
                // 암호화된 바이트 읽기
                byte[] encryptedBytes = File.ReadAllBytes(path);
                
                // LocalDataManager의 복호화 메서드 활용
                byte[] decryptedBytes = Rijndael.Decrypt(encryptedBytes, EncryptionKey);
                
                if (decryptedBytes == null)
                {
                    throw new Exception($"파일 복호화 실패: {path}");
                }
                
                // 복호화된 바이트를 JSON 문자열로 변환
                string jsonString = Encoding.UTF8.GetString(decryptedBytes);

                // Newtonsoft.Json으로 역직렬화
                return JsonConvert.DeserializeObject<SaveData>(jsonString, jsonSettings);
            });
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"경로 {path}에서 로드 실패: {e.Message}");
#endif
            return null;
        }
    }

    /// <summary>
    /// Rijndael 암호화된 파일을 비동기적으로 로드하고 역직렬화하여 SaveData 객체를 반환합니다.
    /// 최대 MaxRetryCount번 재시도하며, 각 시도마다 백업 파일도 확인합니다.
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
            Debug.Log("비동기 로드 시작 (Rijndael 암호화 + JSON)...");
#endif

            string mainPath = GetSavePath();
            string backupPath = GetBackupPath();
            
            // 메인 파일이 없고, 백업이 있는 경우 백업에서 복원
            if (!File.Exists(mainPath) && File.Exists(backupPath))
            {
#if UNITY_EDITOR
                Debug.Log("주 저장 파일을 찾을 수 없습니다. 백업에서 복원 중...");
#endif
                File.Copy(backupPath, mainPath, true);
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
                    // 먼저 메인 파일에서 시도
                    saveData = await TryLoadFromPath(mainPath);
                    
                    // 메인 파일 로드 실패 시 백업 파일에서 시도
                    if (saveData == null && File.Exists(backupPath))
                    {
#if UNITY_EDITOR
                        Debug.Log("메인 파일에서 로드 실패, 백업에서 시도 중...");
#endif
                        saveData = await TryLoadFromPath(backupPath);
                        
                        // 백업에서 성공적으로 로드되면 메인 파일 복원
                        if (saveData != null)
                        {
#if UNITY_EDITOR
                            Debug.Log("백업에서 성공적으로 로드됨. 메인 파일 복원 중...");
#endif
                            File.Copy(backupPath, mainPath, true);
                        }
                    }
                    
                    if (saveData != null)
                    {
#if UNITY_EDITOR
                        Debug.Log("게임 데이터가 성공적으로 로드되었습니다.");
#endif
                    }
                }
                catch (Exception e)
                {
                    lastException = e;
#if UNITY_EDITOR
                    Debug.LogWarning($"로드 시도 {attempt + 1}/{MaxRetryCount} 실패: {e.Message}");
#endif
                }
            }
            
            if (saveData == null)
            {
                if (lastException != null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"모든 로드 시도 실패 ({MaxRetryCount}회): {lastException.Message}\n{lastException.StackTrace}");
#endif
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("저장 파일을 찾을 수 없습니다.");
#endif
                }
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
    /// <param name="cleanupOrphanedData">제거된 객체의 데이터를 정리할지 여부 (false 시 다음 저장 시 정리)</param>
    public void RestoreLoadedData(SaveData saveData, RestoreStateResultCallback callback = null, bool cleanupOrphanedData = false)
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

        // 캐시 초기화 확인 (중복 검사 대신 유틸리티 함수 사용)
        EnsureCacheInitialized();

        // 불필요한 데이터 정리 옵션이 활성화되어 있으면 로드 직후에 정리
        int orphanedDataCount = 0;
        if (cleanupOrphanedData)
        {
            orphanedDataCount = CleanupOrphanedData(saveData);
        }

        List<string> failedObjects = new List<string>();
        int restoredCount = 0;
        
        foreach (var kvp in saveData.objectStates)
        {
            string id = kvp.Key;
            // 내부 관리 데이터는 건너뛰기
            if (id.StartsWith(InternalKeyPrefix))
            {
                continue;
            }

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
        string cleanupMsg = cleanupOrphanedData ? $", 정리된 고아 데이터: {orphanedDataCount}" : "";
        Debug.Log($"게임 상태가 복원되었습니다. 성공: {restoredCount}, 실패: {failedObjects.Count}{cleanupMsg}");
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

    /// <summary>
    /// LocalDataManager에 저장된 플레이어 데이터를 저장 시스템에 추가합니다.
    /// 이 메서드는 메인 무기, 스킨 등 중요한 플레이어 데이터를 안전하게 저장합니다.
    /// </summary>
    /// <param name="saveData">저장 데이터 객체</param>
    public void AddPlayerDataFromLocalDataManager(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("SaveData가 null입니다");
            return;
        }

        try
        {
            // LocalDataManager에서 유저 메인 무기 데이터 가져오기
            JsonData mainWeaponData = LocalDataManager.Instance.GetLocalUserMainWeaponData();
            if (mainWeaponData != null)
            {
                // 메인 무기 데이터를 저장 시스템에 추가
                saveData.objectStates["LocalPlayerMainWeapon"] = mainWeaponData.ToJson();
            }

            // LocalDataManager에서 게임 에셋 데이터 가져오기 (스킨 등)
            JsonData gameAssetData = LocalDataManager.Instance.GetLocalUserGameAssetData();
            if (gameAssetData != null)
            {
                // 게임 에셋 데이터를 저장 시스템에 추가
                saveData.objectStates["LocalPlayerGameAssets"] = gameAssetData.ToJson();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"LocalDataManager에서 플레이어 데이터를 가져오는 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// 저장된 데이터에서 LocalDataManager로 플레이어 데이터를 복원합니다.
    /// </summary>
    /// <param name="saveData">복원할 저장 데이터</param>
    /// <returns>복원 성공 여부</returns>
    public bool RestorePlayerDataToLocalDataManager(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("SaveData가 null입니다");
            return false;
        }

        bool success = true;

        try
        {
            // 메인 무기 데이터 복원
            if (saveData.objectStates.TryGetValue("LocalPlayerMainWeapon", out object mainWeaponObj) && mainWeaponObj is string mainWeaponJson)
            {
                JsonData mainWeaponData = JsonMapper.ToObject(mainWeaponJson);
                if (!LocalDataManager.Instance.UpdateLocalUserMainWeaponData(mainWeaponData))
                {
                    Debug.LogError("메인 무기 데이터를 LocalDataManager에 복원하지 못했습니다");
                    success = false;
                }
            }

            // 게임 에셋 데이터 복원 (스킨 등)
            if (saveData.objectStates.TryGetValue("LocalPlayerGameAssets", out object gameAssetObj) && gameAssetObj is string gameAssetJson)
            {
                JsonData gameAssetData = JsonMapper.ToObject(gameAssetJson);
                if (!LocalDataManager.Instance.UpdateLocalUserGameAssetData(gameAssetData))
                {
                    Debug.LogError("게임 에셋 데이터를 LocalDataManager에 복원하지 못했습니다");
                    success = false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"LocalDataManager에 플레이어 데이터를 복원하는 중 오류 발생: {ex.Message}");
            success = false;
        }

        return success;
    }

    /// <summary>
    /// 게임을 저장하기 전에 추가로 LocalDataManager의 데이터도 포함합니다.
    /// 저장 전에 불필요한 데이터를 정리하는 옵션이 포함되어 있습니다.
    /// </summary>
    /// <param name="cleanupOrphanedData">저장 전에 제거된 객체의 데이터를 정리할지 여부 (기본값: 설정값 사용)</param>
    /// <returns>성공 여부를 나타내는 Task<bool></returns>
    public async Task<bool> SaveGameWithPlayerDataAsync(bool? cleanupOrphanedData = null)
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
            Debug.Log("플레이어 데이터를 포함한 비동기 저장 시작...");
#endif

            // 캐시 초기화 확인 (중복 검사 대신 유틸리티 함수 사용)
            EnsureCacheInitialized();

            // 1. 저장할 데이터 캡처
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
            
            // 2. LocalDataManager의 플레이어 데이터 추가
            AddPlayerDataFromLocalDataManager(saveData);

            // 3. 불필요한 데이터 정리 (파라미터 값이나 설정값에 따라)
            bool shouldCleanup = cleanupOrphanedData ?? cleanupSaveDataOnSave;
            int removedCount = 0;
            
            if (shouldCleanup)
            {
                removedCount = CleanupOrphanedData(saveData);
            }

            // 4. 저장 진행
            bool success = false;
            Exception lastException = null;
            
            for (int attempt = 0; attempt < MaxRetryCount && !success; attempt++)
            {
                if (attempt > 0)
                {
#if UNITY_EDITOR
                    Debug.Log($"저장 재시도 중... (시도 {attempt + 1}/{MaxRetryCount})");
#endif
                    await Task.Delay(RetryDelayMs);
                }
                
                try
                {
                    string path = GetSavePath();
                    await Task.Run(() =>
                    {
                        string tempPath = path + ".tmp";
                        string jsonString = JsonConvert.SerializeObject(saveData, jsonSettings);
                        byte[] encryptedBytes = Rijndael.EncryptString(jsonString, EncryptionKey);
                        File.WriteAllBytes(tempPath, encryptedBytes);
                        
                        if (File.Exists(path))
                        {
                            string backupPath = GetBackupPath();
                            if (File.Exists(backupPath))
                            {
                                File.Delete(backupPath);
                            }
                            File.Move(path, backupPath);
                        }
                        
                        File.Move(tempPath, path);
                    });
                    
                    success = true;
#if UNITY_EDITOR
                    string cleanupMsg = shouldCleanup ? $" (정리된 고아 데이터: {removedCount}개)" : "";
                    Debug.Log($"플레이어 데이터를 포함한 게임이 {path}에 암호화되어 저장되었습니다.{cleanupMsg}");
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
    /// 게임 데이터를 로드하고 LocalDataManager의 플레이어 데이터도 함께 복원합니다.
    /// </summary>
    /// <param name="cleanupOrphanedData">로드 후 제거된 객체의 데이터를 정리할지 여부</param>
    /// <returns>로드된 SaveData 객체 또는 로드 실패 시 null</returns>
    public async Task<SaveData> LoadGameWithPlayerDataAsync(bool cleanupOrphanedData = false)
    {
        SaveData loadedData = await LoadGameDataAsync();
        
        if (loadedData != null)
        {
            // 불필요한 데이터 정리 옵션이 활성화된 경우 정리
            if (cleanupOrphanedData)
            {
                EnsureCacheInitialized(); // 캐시가 최신 상태인지 확인
                int removedCount = CleanupOrphanedData(loadedData);
                
#if UNITY_EDITOR
                if (removedCount > 0)
                {
                    Debug.Log($"로드 중 {removedCount}개의 고아 데이터가 제거되었습니다.");
                }
#endif
            }
            
            // LocalDataManager의 플레이어 데이터도 함께 복원
            bool playerDataRestored = RestorePlayerDataToLocalDataManager(loadedData);
            
#if UNITY_EDITOR
            if (playerDataRestored)
            {
                Debug.Log("플레이어 데이터가 LocalDataManager에 성공적으로 복원되었습니다.");
            }
            else
            {
                Debug.LogWarning("LocalDataManager에 플레이어 데이터 복원에 일부 실패했습니다.");
            }
#endif
        }
        
        return loadedData;
    }
}
