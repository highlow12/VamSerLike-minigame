using UnityEngine;
using System.Collections.Generic;
using System.IO;
// using System.Runtime.Serialization.Formatters.Binary; // 더 이상 사용 안 함
using System.Linq;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json; // Newtonsoft.Json 사용
using System.Text;     // Encoding (UTF8, Base64) 사용

public class SaveManager : Singleton<SaveManager>
{
    // 파일 확장자 변경 (선택 사항)
    private string saveFileName = "gameSave.sav";
    private bool isSaving = false;
    private bool isLoading = false;

    // Newtonsoft.Json 설정 (타입 정보 포함, 들여쓰기 없음)
    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto, // 객체 타입 정보 포함 (중요!)
        Formatting = Formatting.None // 파일 크기 줄이기 위해 들여쓰기 없음
        // 필요에 따라 다른 설정 추가 가능 (ReferenceLoopHandling 등)
    };

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }

    /// <summary>
    /// 게임 상태를 비동기적으로 JSON으로 직렬화하고 Base64 인코딩하여 저장합니다.
    /// </summary>
    public async Task SaveGameAsync()
    {
        if (isSaving)
        {
#if UNITY_EDITOR
            Debug.LogWarning("저장이 이미 진행 중입니다.");
#endif
            return;
        }
        isSaving = true;
#if UNITY_EDITOR
        Debug.Log("비동기 저장 시작 (JSON + Base64)...");
#endif

        // 1. (메인 스레드) 저장할 데이터 캡처
        SaveData saveData = new SaveData();
        saveData.lastSaved = DateTime.UtcNow;

        IEnumerable<ISaveable> saveableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();
        foreach (ISaveable saveable in saveableObjects)
        {
            string id = saveable.GetUniqueIdentifier();
            if (!string.IsNullOrEmpty(id))
            {
                 try
                 {
                    saveData.objectStates[id] = saveable.CaptureState();
                 }
                 catch (Exception e)
                 {
#if UNITY_EDITOR
                     Debug.LogError($"객체 {id} ({((MonoBehaviour)saveable).gameObject.name})의 상태 캡처 오류: {e.Message}\\n{e.StackTrace}", (MonoBehaviour)saveable);
#endif
                 }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"GameObject '{((MonoBehaviour)saveable).gameObject.name}'에 저장용 고유 식별자가 없습니다.", (MonoBehaviour)saveable);
#endif
            }
        }

        // 2. (백그라운드 스레드) JSON 직렬화, Base64 인코딩 및 파일 쓰기
        try
        {
            string path = GetSavePath();
            await Task.Run(async () =>
            {
                // Newtonsoft.Json으로 직렬화
                string jsonString = JsonConvert.SerializeObject(saveData, jsonSettings);

                // JSON 문자열을 Base64로 인코딩
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                string base64String = Convert.ToBase64String(jsonBytes);

                // Base64 문자열을 파일에 비동기적으로 쓰기
                using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8)) // UTF8 인코딩 명시
                {
                    await writer.WriteAsync(base64String);
                }
            });
#if UNITY_EDITOR
            Debug.Log($"게임이 {path}에 비동기적으로 저장되었습니다 (JSON + Base64).");
#endif
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError($"게임을 비동기적으로 저장하는 데 실패했습니다: {e.Message}\\n{e.StackTrace}");
#endif
        }
        finally
        {
            isSaving = false;
        }
    }

     /// <summary>
    /// Base64로 인코딩된 JSON 파일을 비동기적으로 로드하고 역직렬화하여 SaveData 객체를 반환합니다.
    /// </summary>
    /// <returns>로드된 SaveData 객체 또는 로드 실패 시 null.</returns>
    public async Task<SaveData> LoadGameDataAsync()
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
#if UNITY_EDITOR
            Debug.LogWarning("저장 파일을 찾을 수 없습니다.");
#endif
            isLoading = false;
            return null;
        }

        SaveData saveData = null;
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
#if UNITY_EDITOR
            Debug.Log($"게임 데이터가 {path}에서 비동기적으로 로드되었습니다 (JSON + Base64).");
#endif
        }
        catch (FormatException fe) // Base64 디코딩 실패
        {
#if UNITY_EDITOR
             Debug.LogError($"Base64 디코딩 실패: 저장 파일이 손상되었거나 잘못된 형식일 수 있습니다. Path: {path}, Error: {fe.Message}\n{fe.StackTrace}");
#endif
             saveData = null;
        }
        catch (JsonException je) // JSON 역직렬화 실패
        {
#if UNITY_EDITOR
             Debug.LogError($"JSON 역직렬화 실패: 저장 데이터 구조가 변경되었거나 파일이 손상되었을 수 있습니다. Path: {path}, Error: {je.Message}\n{je.StackTrace}");
#endif
             saveData = null;
        }
        catch (Exception e) // 기타 예외
        {
#if UNITY_EDITOR
            Debug.LogError($"게임 데이터를 비동기적으로 로드하는 데 실패했습니다: {e.Message}\n{e.StackTrace}");
#endif
            saveData = null;
        }
        finally
        {
             isLoading = false;
        }

        return saveData;
    }

    /// <summary>
    /// 로드된 SaveData를 사용하여 게임 상태를 복원합니다. (메인 스레드에서 호출해야 함)
    /// </summary>
    /// <param name="saveData">LoadGameDataAsync에서 반환된 SaveData 객체.</param>
    public void RestoreLoadedData(SaveData saveData)
    {
        if (saveData == null)
        {
#if UNITY_EDITOR
            Debug.LogError("null SaveData로부터 상태를 복원할 수 없습니다.");
#endif
            return;
        }

#if UNITY_EDITOR
        Debug.Log("로드된 데이터로부터 게임 상태 복원 중...");
#endif

        // (메인 스레드) ISaveable 객체 찾기 및 상태 복원
        // 참고: FindObjectsByType은 느릴 수 있습니다. 필요한 경우 최적화를 고려하세요.
        IEnumerable<ISaveable> saveableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();
        Dictionary<string, ISaveable> saveableDict = new Dictionary<string, ISaveable>();

        foreach(ISaveable saveable in saveableObjects)
        {
             string id = saveable.GetUniqueIdentifier();
             if (!string.IsNullOrEmpty(id))
             {
                 if (!saveableDict.ContainsKey(id))
                 {
                    saveableDict.Add(id, saveable);
                 }
                 else
                 {
#if UNITY_EDITOR
                     Debug.LogWarning($"중복된 고유 식별자 발견: {id} (GameObject: '{((MonoBehaviour)saveable).gameObject.name}'). 하나만 복원됩니다.", (MonoBehaviour)saveable);
#endif
                 }
             }
        }

        foreach (var kvp in saveData.objectStates)
        {
            string id = kvp.Key;
            if (saveableDict.TryGetValue(id, out ISaveable saveable))
            {
                 try
                 {
                    // RestoreState는 여전히 object를 받으므로, Newtonsoft가 올바른 타입으로
                    // 역직렬화했는지 확인하는 것이 중요 (TypeNameHandling 설정 덕분에 가능)
                    saveable.RestoreState(kvp.Value);
                 }
                 catch (Exception e)
                 {
#if UNITY_EDITOR
                     Debug.LogError($"객체 {id} ({((MonoBehaviour)saveable).gameObject.name})의 상태 복원 오류: {e.Message}\\n{e.StackTrace}", (MonoBehaviour)saveable);
#endif
                 }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"현재 씬에서 고유 식별자 '{id}'를 가진 객체를 찾을 수 없어 상태를 복원할 수 없습니다.");
#endif
            }
        }

#if UNITY_EDITOR
        Debug.Log("게임 상태가 복원되었습니다.");
#endif
    }

    // --- 사용 예시 (다른 스크립트에서 호출) ---
    /*
    public async void HandleSaveButtonClicked()
    {
        // UI 비활성화 또는 로딩 인디케이터 표시
#if UNITY_EDITOR
        Debug.Log("저장 버튼 클릭됨. 저장 시작...");
#endif
        await SaveManager.Instance.SaveGameAsync();
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
            SaveManager.Instance.RestoreLoadedData(loadedData);
#if UNITY_EDITOR
            Debug.Log("로드 및 복원 작업 완료.");
#endif
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
