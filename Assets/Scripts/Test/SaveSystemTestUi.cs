using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SaveSystemTestUi : MonoBehaviour
{
    // 테스트용 간단한 데이터
    private string testKey = "TestObject";
    private bool testBoolValue = false;
    private int testIntValue = 0;
    private float testFloatValue = 0f;
    private Vector3 testPosition = Vector3.zero;
    private bool isRestoring = false;
    
    // 리스트 및 딕셔너리 테스트를 위한 변수
    private List<string> testStringList = new List<string>();
    private List<int> testIntList = new List<int>();
    private Dictionary<string, int> testDictionary = new Dictionary<string, int>();
    private Dictionary<int, string> testDictionaryReversed = new Dictionary<int, string>();
    private bool showComplexDataUI = false;

    // 테스트 로그 메시지를 담을 문자열
    private string logMessages = "";
    private Vector2 scrollPosition = Vector2.zero;
    private int maxLogLines = 20;
    private string[] logLines;
    
    // 단독 실행을 위한 UI 표시 여부
    [SerializeField] private bool showUI = true;
    
    // UI 위치와 크기 설정
    private Rect windowRect = new Rect(10, 10, 320, 600);
    private int windowId = 100;

    private void Awake()
    {
        logLines = new string[maxLogLines];
        for (int i = 0; i < maxLogLines; i++)
        {
            logLines[i] = "";
        }
        
        // 첫 시작 메시지
        Log("SaveSystem 테스트 시작");
        
        // 테스트 리스트와 딕셔너리 초기화
        InitializeComplexTestData();
    }
    
    /// <summary>
    /// 테스트용 복잡한 데이터 구조 초기화
    /// </summary>
    private void InitializeComplexTestData()
    {
        // 테스트 문자열 리스트 초기화
        testStringList = new List<string>() { "첫 번째", "두 번째", "세 번째" };
        
        // 테스트 정수 리스트 초기화
        testIntList = new List<int>() { 10, 20, 30 };
        
        // 테스트 딕셔너리(문자열-정수) 초기화
        testDictionary = new Dictionary<string, int>()
        {
            { "사과", 5 },
            { "바나나", 3 },
            { "오렌지", 7 }
        };
        
        // 테스트 딕셔너리(정수-문자열) 초기화
        testDictionaryReversed = new Dictionary<int, string>()
        {
            { 1, "일" },
            { 2, "이" },
            { 3, "삼" }
        };
    }
    
    private void Start()
    {
        // Unity 에디터가 아닌 환경에서도 작동하도록 수동으로 캐싱 시도
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CacheAllSaveableObjects();
            Log("초기 SaveableObjects 캐싱 완료");
        }
        else
        {
            Log("SaveManager 인스턴스를 찾을 수 없습니다!");
        }
        
        // 현재 씬에서 플레이어의 위치 가져오기 시도
        UpdatePlayerPosition();
    }

    /// <summary>
    /// 로그 메시지를 저장하고 콘솔에도 출력합니다
    /// </summary>
    private void Log(string message)
    {
        Debug.Log($"[SaveTest] {message}");
        
        // 로그 라인 업데이트
        for (int i = maxLogLines - 1; i > 0; i--)
        {
            logLines[i] = logLines[i - 1];
        }
        logLines[0] = $"[{Time.time:F1}] {message}";
        
        // 전체 로그 문자열 업데이트
        logMessages = string.Join("\n", logLines);
    }
    
    /// <summary>
    /// 플레이어 위치를 업데이트합니다
    /// </summary>
    private void UpdatePlayerPosition()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            testPosition = GameManager.Instance.player.transform.position;
        }
        else
        {
            // 플레이어가 없으면 현재 위치로 대체
            testPosition = transform.position;
        }
    }
    
    /// <summary>
    /// 단독 실행을 위한 GUI 표시
    /// </summary>
    private void OnGUI()
    {
        if (!showUI) return;
        
        // GUI 스타일 초기화
        GUI.skin = null;
        
        // 창 렌더링
        windowRect = GUILayout.Window(windowId, windowRect, DrawWindow, "SaveSystem 테스트");
    }
    
    /// <summary>
    /// 창 내부 GUI 그리기
    /// </summary>
    private void DrawWindow(int windowID)
    {
        // 메인 컨테이너 시작
        GUILayout.BeginVertical(GUILayout.Width(300), GUILayout.Height(580));
        
        // UI 표시 토글
        GUILayout.BeginHorizontal();
        showUI = GUILayout.Toggle(showUI, "UI 표시");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("플레이어 위치 새로고침", GUILayout.Width(150)))
        {
            UpdatePlayerPosition();
        }
        GUILayout.EndHorizontal();
        
        // 메인 UI 렌더링
        SaveSystemGUI();
        
        GUILayout.EndVertical();
        
        // 창 드래그 가능하도록 설정
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    public void SaveSystemGUI()
    {
        GUILayout.BeginVertical("box", GUILayout.Width(300), GUILayout.Height(520));

        // 테스트 데이터 섹션
        GUILayout.Label("테스트 데이터", GUILayout.Width(280));
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Bool:", GUILayout.Width(80));
        testBoolValue = GUILayout.Toggle(testBoolValue, testBoolValue.ToString());
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Int:", GUILayout.Width(80));
        string intString = GUILayout.TextField(testIntValue.ToString(), GUILayout.Width(100));
        if (int.TryParse(intString, out int newIntValue))
        {
            testIntValue = newIntValue;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Float:", GUILayout.Width(80));
        string floatString = GUILayout.TextField(testFloatValue.ToString("F2"), GUILayout.Width(100));
        if (float.TryParse(floatString, out float newFloatValue))
        {
            testFloatValue = newFloatValue;
        }
        GUILayout.EndHorizontal();

        // 위치 정보 표시
        GUILayout.Label($"Position: {testPosition}", GUILayout.Width(280));

        // 복잡한 데이터 구조 테스트 UI 토글
        GUILayout.Space(5);
        showComplexDataUI = GUILayout.Toggle(showComplexDataUI, "리스트/딕셔너리 테스트 설정 표시");
        
        if (showComplexDataUI)
        {
            DrawComplexDataTestUI();
        }

        GUILayout.Space(10);

        // 캐시 테스트
        if (GUILayout.Button("Cache Saveable Objects", GUILayout.Width(280), GUILayout.Height(30)))
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.CacheAllSaveableObjects();
                Log("캐싱 시도 완료");
            }
            else
            {
                Log("SaveManager 인스턴스를 찾을 수 없습니다!");
            }
        }

        // 테스트 저장 섹션
        GUILayout.Space(5);
        GUILayout.Label("저장 테스트", GUILayout.Width(280));
        
        // 기본 저장 테스트
        if (GUILayout.Button("Save Game", GUILayout.Width(280), GUILayout.Height(30)))
        {
            if (SaveManager.Instance != null)
            {
                StartCoroutine(SaveGameTest());
            }
            else
            {
                Log("SaveManager 인스턴스를 찾을 수 없습니다!");
            }
        }

        // 플레이어 데이터 포함 저장 테스트
        if (GUILayout.Button("Save Game With Player Data", GUILayout.Width(280), GUILayout.Height(30)))
        {
            if (SaveManager.Instance != null)
            {
                StartCoroutine(SaveGameWithPlayerDataTest());
            }
            else
            {
                Log("SaveManager 인스턴스를 찾을 수 없습니다!");
            }
        }

        GUILayout.Space(5);
        GUILayout.Label("로드 테스트", GUILayout.Width(280));
        
        // 기본 로드 테스트
        if (GUILayout.Button("Load Game", GUILayout.Width(280), GUILayout.Height(30)))
        {
            if (SaveManager.Instance != null)
            {
                StartCoroutine(LoadGameTest());
            }
            else
            {
                Log("SaveManager 인스턴스를 찾을 수 없습니다!");
            }
        }

        // 플레이어 데이터 포함 로드 테스트
        if (GUILayout.Button("Load Game With Player Data", GUILayout.Width(280), GUILayout.Height(30)))
        {
            if (SaveManager.Instance != null)
            {
                StartCoroutine(LoadGameWithPlayerDataTest());
            }
            else
            {
                Log("SaveManager 인스턴스를 찾을 수 없습니다!");
            }
        }

        GUILayout.Space(10);

        // 테스트 객체 생성
        if (GUILayout.Button("Create Test SaveableObject", GUILayout.Width(280), GUILayout.Height(30)))
        {
            CreateTestSaveableObject();
        }
        
        // 복잡한 데이터 구조 테스트 객체 생성
        if (GUILayout.Button("Create Complex Data Test Object", GUILayout.Width(280), GUILayout.Height(30)))
        {
            CreateComplexDataTestObject();
        }

        // 로그 출력 영역
        GUILayout.Space(10);
        GUILayout.Label("테스트 로그:", GUILayout.Width(280));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(280), GUILayout.Height(120));
        GUILayout.TextArea(logMessages, GUILayout.Width(260));
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }
    
    /// <summary>
    /// 복잡한 데이터 구조 테스트 UI 그리기
    /// </summary>
    private void DrawComplexDataTestUI()
    {
        GUILayout.BeginVertical("box");
        
        // 문자열 리스트 UI
        GUILayout.Label("문자열 리스트:");
        string stringListContent = string.Join(", ", testStringList);
        GUILayout.Label(stringListContent);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("항목 추가", GUILayout.Width(100)))
        {
            testStringList.Add($"항목{testStringList.Count + 1}");
            Log($"문자열 리스트에 항목 추가: 항목{testStringList.Count}");
        }
        
        if (testStringList.Count > 0 && GUILayout.Button("항목 제거", GUILayout.Width(100)))
        {
            testStringList.RemoveAt(testStringList.Count - 1);
            Log("문자열 리스트에서 마지막 항목 제거");
        }
        GUILayout.EndHorizontal();
        
        // 정수 리스트 UI
        GUILayout.Space(5);
        GUILayout.Label("정수 리스트:");
        string intListContent = string.Join(", ", testIntList);
        GUILayout.Label(intListContent);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("항목 추가", GUILayout.Width(100)))
        {
            testIntList.Add(10 * (testIntList.Count + 1));
            Log($"정수 리스트에 항목 추가: {10 * (testIntList.Count)}");
        }
        
        if (testIntList.Count > 0 && GUILayout.Button("항목 제거", GUILayout.Width(100)))
        {
            testIntList.RemoveAt(testIntList.Count - 1);
            Log("정수 리스트에서 마지막 항목 제거");
        }
        GUILayout.EndHorizontal();
        
        // 딕셔너리 UI
        GUILayout.Space(5);
        GUILayout.Label("딕셔너리 (string -> int):");
        foreach (var pair in testDictionary)
        {
            GUILayout.Label($"  {pair.Key}: {pair.Value}");
        }
        
        // 딕셔너리 추가/제거 UI
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("항목 추가", GUILayout.Width(100)))
        {
            string newKey = $"과일{testDictionary.Count + 1}";
            testDictionary[newKey] = testDictionary.Count * 2;
            Log($"딕셔너리에 항목 추가: {newKey} -> {testDictionary.Count * 2}");
        }
        
        if (testDictionary.Count > 0 && GUILayout.Button("항목 제거", GUILayout.Width(100)))
        {
            string keyToRemove = $"과일{testDictionary.Count}";
            if (testDictionary.ContainsKey(keyToRemove))
            {
                testDictionary.Remove(keyToRemove);
                Log($"딕셔너리에서 항목 제거: {keyToRemove}");
            }
            else
            {
                // 기본 항목 중 하나 제거
                string firstKey = new List<string>(testDictionary.Keys)[0];
                testDictionary.Remove(firstKey);
                Log($"딕셔너리에서 항목 제거: {firstKey}");
            }
        }
        GUILayout.EndHorizontal();
        
        // 역방향 딕셔너리 UI
        GUILayout.Space(5);
        GUILayout.Label("딕셔너리 (int -> string):");
        foreach (var pair in testDictionaryReversed)
        {
            GUILayout.Label($"  {pair.Key}: {pair.Value}");
        }
        
        // 역방향 딕셔너리 추가/제거 UI
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("항목 추가", GUILayout.Width(100)))
        {
            int newKey = testDictionaryReversed.Count + 4;
            testDictionaryReversed[newKey] = $"숫자{newKey}";
            Log($"역방향 딕셔너리에 항목 추가: {newKey} -> 숫자{newKey}");
        }
        
        if (testDictionaryReversed.Count > 0 && GUILayout.Button("항목 제거", GUILayout.Width(100)))
        {
            int keyToRemove = new List<int>(testDictionaryReversed.Keys)[0];
            testDictionaryReversed.Remove(keyToRemove);
            Log($"역방향 딕셔너리에서 항목 제거: {keyToRemove}");
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 기본 저장 테스트
    /// </summary>
    private IEnumerator SaveGameTest()
    {
        Log("기본 저장 테스트 시작...");
        
        // 테스트용 데이터 생성하기 전에 저장 상태 확인
        Task<bool> saveTask = SaveManager.Instance.SaveGameAsync();
        
        // 작업이 완료될 때까지 대기
        while (!saveTask.IsCompleted)
        {
            Log("저장 중...");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 결과 확인
        bool result = saveTask.Result;
        if (result)
        {
            Log("저장 성공!");
        }
        else
        {
            Log("저장 실패!");
        }
    }

    /// <summary>
    /// 플레이어 데이터 포함 저장 테스트
    /// </summary>
    private IEnumerator SaveGameWithPlayerDataTest()
    {
        Log("플레이어 데이터 포함 저장 테스트 시작...");
        
        Task<bool> saveTask = SaveManager.Instance.SaveGameWithPlayerDataAsync();
        
        // 작업이 완료될 때까지 대기
        while (!saveTask.IsCompleted)
        {
            Log("플레이어 데이터 저장 중...");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 결과 확인
        bool result = saveTask.Result;
        if (result)
        {
            Log("플레이어 데이터 포함 저장 성공!");
        }
        else
        {
            Log("플레이어 데이터 포함 저장 실패!");
        }
    }

    /// <summary>
    /// 기본 로드 테스트
    /// </summary>
    private IEnumerator LoadGameTest()
    {
        if (isRestoring)
        {
            Log("이미 복원 중입니다. 완료될 때까지 기다려주세요.");
            yield break;
        }
        
        isRestoring = true;
        Log("기본 로드 테스트 시작...");
        
        Task<SaveData> loadTask = SaveManager.Instance.LoadGameDataAsync();
        
        // 작업이 완료될 때까지 대기
        while (!loadTask.IsCompleted)
        {
            Log("로드 중...");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 결과 확인
        SaveData saveData = loadTask.Result;
        if (saveData != null)
        {
            Log($"로드 성공! 저장된 시간: {saveData.lastSaved}");
            Log($"저장된 객체 수: {saveData.objectStates.Count}");
            
            // 게임 상태 복원
            SaveManager.Instance.RestoreLoadedData(saveData, OnRestoreComplete);
            
            // 복원 중 대기
            yield return new WaitForSeconds(1);
        }
        else
        {
            Log("로드 실패! 저장 파일이 없거나 손상되었습니다.");
            isRestoring = false;
        }
    }

    /// <summary>
    /// 플레이어 데이터 포함 로드 테스트
    /// </summary>
    private IEnumerator LoadGameWithPlayerDataTest()
    {
        if (isRestoring)
        {
            Log("이미 복원 중입니다. 완료될 때까지 기다려주세요.");
            yield break;
        }
        
        isRestoring = true;
        Log("플레이어 데이터 포함 로드 테스트 시작...");
        
        Task<SaveData> loadTask = SaveManager.Instance.LoadGameWithPlayerDataAsync();
        
        // 작업이 완료될 때까지 대기
        while (!loadTask.IsCompleted)
        {
            Log("플레이어 데이터 로드 중...");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 결과 확인
        SaveData saveData = loadTask.Result;
        if (saveData != null)
        {
            Log($"플레이어 데이터 로드 성공! 저장된 시간: {saveData.lastSaved}");
            
            // 게임 상태 복원
            SaveManager.Instance.RestoreLoadedData(saveData, OnRestoreComplete);
            
            // 복원 중 대기
            yield return new WaitForSeconds(1);
        }
        else
        {
            Log("플레이어 데이터 로드 실패!");
            isRestoring = false;
        }
    }

    /// <summary>
    /// 저장 가능한 테스트 객체 생성
    /// </summary>
    private void CreateTestSaveableObject()
    {
        GameObject testObject = new GameObject("TestSaveableObject");
        TestSaveable testComponent = testObject.AddComponent<TestSaveable>();
        testComponent.boolValue = testBoolValue;
        testComponent.intValue = testIntValue;
        testComponent.floatValue = testFloatValue;
        testObject.transform.position = testPosition;
        
        // UniqueID 컴포넌트 추가
        UniqueID uniqueID = testObject.AddComponent<UniqueID>();
        
        Log($"테스트 객체 생성 완료: {testObject.name}, ID: {uniqueID.ID}");
        
        // 새로 생성된 객체를 캐싱
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CacheAllSaveableObjects();
        }
    }
    
    /// <summary>
    /// 복잡한 데이터 구조를 테스트하는 객체 생성
    /// </summary>
    private void CreateComplexDataTestObject()
    {
        GameObject testObject = new GameObject("ComplexDataTestObject");
        ComplexDataTestSaveable testComponent = testObject.AddComponent<ComplexDataTestSaveable>();
        
        // 리스트와 딕셔너리 데이터 설정
        testComponent.stringList = new List<string>(testStringList);
        testComponent.intList = new List<int>(testIntList);
        testComponent.stringToIntDict = new Dictionary<string, int>(testDictionary);
        testComponent.intToStringDict = new Dictionary<int, string>(testDictionaryReversed);
        
        // 위치 설정
        testObject.transform.position = testPosition;
        
        // UniqueID 컴포넌트 추가
        UniqueID uniqueID = testObject.AddComponent<UniqueID>();
        
        Log($"복잡한 데이터 테스트 객체 생성 완료: {testObject.name}, ID: {uniqueID.ID}");
        Log($"- 문자열 리스트 항목 수: {testComponent.stringList.Count}");
        Log($"- 정수 리스트 항목 수: {testComponent.intList.Count}");
        Log($"- 문자열->정수 딕셔너리 항목 수: {testComponent.stringToIntDict.Count}");
        Log($"- 정수->문자열 딕셔너리 항목 수: {testComponent.intToStringDict.Count}");
        
        // 새로 생성된 객체를 캐싱
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CacheAllSaveableObjects();
        }
    }

    /// <summary>
    /// 상태 복원 완료 콜백
    /// </summary>
    private void OnRestoreComplete(bool success, string errorMessage)
    {
        if (success)
        {
            Log("게임 상태 복원 완료!");
        }
        else
        {
            Log($"게임 상태 복원 중 오류 발생: {errorMessage}");
        }
        
        isRestoring = false;
    }
}

/// <summary>
/// 테스트용 저장 가능 컴포넌트
/// </summary>
public class TestSaveable : MonoBehaviour, ISaveable
{
    public bool boolValue = false;
    public int intValue = 0;
    public float floatValue = 0f;
    private UniqueID uniqueID;
    
    private void Awake()
    {
        uniqueID = GetComponent<UniqueID>();
        if (uniqueID == null)
        {
            uniqueID = gameObject.AddComponent<UniqueID>();
            Debug.LogWarning("TestSaveable에 UniqueID가 없어 자동 추가됨", this);
        }
    }
    
    public string GetUniqueIdentifier()
    {
        return uniqueID?.ID ?? "";
    }
    
    public object CaptureState()
    {
        return new TestSaveableData
        {
            boolValue = this.boolValue,
            intValue = this.intValue,
            floatValue = this.floatValue,
            position = transform.position
        };
    }
    
    public void RestoreState(object state)
    {
        if (state is TestSaveableData data)
        {
            this.boolValue = data.boolValue;
            this.intValue = data.intValue;
            this.floatValue = data.floatValue;
            transform.position = data.position;
            
            Debug.Log($"TestSaveable 상태 복원: bool={boolValue}, int={intValue}, float={floatValue}, pos={transform.position}", this);
        }
    }
    
    [System.Serializable]
    private struct TestSaveableData
    {
        public bool boolValue;
        public int intValue;
        public float floatValue;
        public Vector3 position;
    }
}

/// <summary>
/// 복잡한 데이터 구조를 테스트하기 위한 저장 가능 컴포넌트
/// </summary>
public class ComplexDataTestSaveable : MonoBehaviour, ISaveable
{
    // 리스트 및 딕셔너리 테스트용 데이터
    public List<string> stringList = new List<string>();
    public List<int> intList = new List<int>();
    public Dictionary<string, int> stringToIntDict = new Dictionary<string, int>();
    public Dictionary<int, string> intToStringDict = new Dictionary<int, string>();
    
    private UniqueID uniqueID;
    
    private void Awake()
    {
        uniqueID = GetComponent<UniqueID>();
        if (uniqueID == null)
        {
            uniqueID = gameObject.AddComponent<UniqueID>();
            Debug.LogWarning("ComplexDataTestSaveable에 UniqueID가 없어 자동 추가됨", this);
        }
    }
    
    public string GetUniqueIdentifier()
    {
        return uniqueID?.ID ?? "";
    }
    
    public object CaptureState()
    {
        return new ComplexDataSaveableData
        {
            stringList = this.stringList,
            intList = this.intList,
            stringToIntDict = this.stringToIntDict,
            intToStringDict = this.intToStringDict,
            position = transform.position
        };
    }
    
    public void RestoreState(object state)
    {
        if (state is ComplexDataSaveableData data)
        {
            this.stringList = data.stringList;
            this.intList = data.intList;
            this.stringToIntDict = data.stringToIntDict;
            this.intToStringDict = data.intToStringDict;
            transform.position = data.position;
            
            Debug.Log($"ComplexDataTestSaveable 상태 복원:", this);
            Debug.Log($"- 문자열 리스트: {string.Join(", ", this.stringList)}", this);
            Debug.Log($"- 정수 리스트: {string.Join(", ", this.intList)}", this);
            Debug.Log($"- 문자열->정수 딕셔너리: {this.stringToIntDict.Count}개 항목", this);
            Debug.Log($"- 정수->문자열 딕셔너리: {this.intToStringDict.Count}개 항목", this);
            Debug.Log($"- 위치: {transform.position}", this);
        }
    }
    
    [System.Serializable]
    private struct ComplexDataSaveableData
    {
        public List<string> stringList;
        public List<int> intList;
        public Dictionary<string, int> stringToIntDict;
        public Dictionary<int, string> intToStringDict;
        public Vector3 position;
    }
}