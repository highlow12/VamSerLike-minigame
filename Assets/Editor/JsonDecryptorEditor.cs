using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using CustomEncryption;
using System.Text;
using System.Collections.Generic;

public class JsonDecryptorEditor : EditorWindow
{
    private TextAsset jsonFile;
    private string decryptionKey = "EnRBcwL791f3oEf/AH2D0D2EhbajQ0yBimSUbLHDTA8=";
    private string outputFileName = "DecryptedJson";
    private string status = "";
    
    private bool isBatchMode = false;
    private DefaultAsset sourceFolder;
    private string batchFileExtension = "json";
    private int successCount = 0;
    private int failCount = 0;
    private Vector2 scrollPosition;
    private List<string> recentFiles = new List<string>();

    [MenuItem("Tools/JSON 복호화 도구")]
    public static void ShowWindow()
    {
        GetWindow<JsonDecryptorEditor>("JSON 복호화 도구");
    }

    private void OnGUI()
    {
        GUILayout.Label("암호화된 JSON 파일 복호화 도구", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space();

        // 모드 선택: 단일 파일 또는 배치 처리
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("모드 선택", GUILayout.Width(130));
        isBatchMode = EditorGUILayout.Toggle("배치 처리 모드", isBatchMode);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 복호화 키 입력
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("복호화 키", GUILayout.Width(130));
        decryptionKey = EditorGUILayout.TextField(decryptionKey);
        EditorGUILayout.EndHorizontal();

        if (isBatchMode)
        {
            DrawBatchModeGUI();
        }
        else
        {
            DrawSingleFileGUI();
        }

        EditorGUILayout.Space();

        // 복호화 버튼
        if (GUILayout.Button("JSON 복호화하기", GUILayout.Height(30)))
        {
            if (isBatchMode)
            {
                BatchDecryptJson();
            }
            else
            {
                DecryptSingleJson();
            }
        }
        
        // StreamingAssets 바로가기 버튼
        EditorGUILayout.Space();
        GUILayout.Label("바로가기", EditorStyles.boldLabel);
        
        if (GUILayout.Button("StreamingAssets/LocalData 폴더 선택하기"))
        {
            string streamingAssetsPath = Application.streamingAssetsPath;
            string localDataPath = Path.Combine(streamingAssetsPath, "LocalData");
            
            if (Directory.Exists(localDataPath))
            {
                string relativePath = "Assets" + localDataPath.Substring(Application.dataPath.Length);
                DefaultAsset folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(relativePath);
                if (folder != null)
                {
                    sourceFolder = folder;
                    Selection.activeObject = folder;
                    EditorGUIUtility.PingObject(folder);
                }
                else
                {
                    status = "StreamingAssets/LocalData 폴더를 찾을 수 없습니다.";
                }
            }
            else
            {
                status = "StreamingAssets/LocalData 폴더가 존재하지 않습니다.";
            }
        }
        
        // 최근 복호화된 파일 목록
        if (recentFiles.Count > 0)
        {
            EditorGUILayout.Space();
            GUILayout.Label("최근 복호화된 파일", EditorStyles.boldLabel);
            
            foreach (string filePath in recentFiles)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(Path.GetFileName(filePath), EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("열기", GUILayout.Width(60)))
                {
                    TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        // 상태 메시지 표시
        if (!string.IsNullOrEmpty(status))
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(status, MessageType.Info);
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawSingleFileGUI()
    {
        // 암호화된 JSON 파일 선택
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("암호화된 JSON 파일", GUILayout.Width(130));
        jsonFile = (TextAsset)EditorGUILayout.ObjectField(jsonFile, typeof(TextAsset), false);
        EditorGUILayout.EndHorizontal();

        // 출력 파일 이름 설정
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("출력 파일 이름", GUILayout.Width(130));
        outputFileName = EditorGUILayout.TextField(outputFileName);
        EditorGUILayout.EndHorizontal();
        
        if (jsonFile != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("선택된 파일:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(jsonFile));
            
            if (outputFileName == "DecryptedJson" || string.IsNullOrEmpty(outputFileName))
            {
                outputFileName = Path.GetFileNameWithoutExtension(jsonFile.name);
            }
        }
    }

    private void DrawBatchModeGUI()
    {
        // 소스 폴더 선택
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("소스 폴더", GUILayout.Width(130));
        sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField(sourceFolder, typeof(DefaultAsset), false);
        EditorGUILayout.EndHorizontal();

        // 파일 확장자 필터
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("파일 확장자", GUILayout.Width(130));
        batchFileExtension = EditorGUILayout.TextField(batchFileExtension);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("선택한 폴더 내의 모든 파일을 복호화하여 Assets/DecryptedJson 폴더에 저장합니다.", MessageType.Info);
        
        if (sourceFolder != null)
        {
            string folderPath = AssetDatabase.GetAssetPath(sourceFolder);
            EditorGUILayout.LabelField("선택된 폴더:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(folderPath);
            
            // 폴더 내 파일 개수 표시
            string[] filePaths = Directory.GetFiles(folderPath, $"*.{batchFileExtension}", SearchOption.AllDirectories);
            EditorGUILayout.LabelField($"찾은 파일: {filePaths.Length}개");
        }
    }

    private void DecryptSingleJson()
    {
        if (jsonFile == null)
        {
            status = "오류: 복호화할 JSON 파일을 선택해주세요.";
            return;
        }

        try
        {
            byte[] encryptedBytes = jsonFile.bytes;
            byte[] decryptedBytes = Rijndael.Decrypt(encryptedBytes, decryptionKey);
            
            if (decryptedBytes == null)
            {
                status = "오류: 복호화에 실패했습니다. 키가 올바른지 확인해주세요.";
                return;
            }

            string decryptedJson = Encoding.UTF8.GetString(decryptedBytes);
            string filePath = SaveDecryptedJson(decryptedJson, outputFileName);
            
            status = $"성공: {outputFileName}.json 파일이 복호화되었습니다.";
            
            // 최근 파일 목록에 추가
            AddToRecentFiles(filePath);
        }
        catch (Exception ex)
        {
            status = $"오류: {ex.Message}";
            Debug.LogError($"JSON 복호화 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void BatchDecryptJson()
    {
        if (sourceFolder == null)
        {
            status = "오류: 소스 폴더를 선택해주세요.";
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(sourceFolder);
        if (!Directory.Exists(folderPath))
        {
            status = "오류: 선택한 소스 폴더가 존재하지 않습니다.";
            return;
        }

        // 폴더 내 모든 파일 가져오기
        string[] filePaths = Directory.GetFiles(folderPath, $"*.{batchFileExtension}", SearchOption.AllDirectories);
        
        successCount = 0;
        failCount = 0;
        recentFiles.Clear(); // 최근 파일 목록 초기화

        try
        {
            foreach (string filePath in filePaths)
            {
                try
                {
                    // 파일 읽기
                    byte[] encryptedBytes = File.ReadAllBytes(filePath);
                    byte[] decryptedBytes = Rijndael.Decrypt(encryptedBytes, decryptionKey);
                    
                    if (decryptedBytes == null)
                    {
                        Debug.LogWarning($"파일 복호화 실패: {filePath}");
                        failCount++;
                        continue;
                    }

                    string decryptedJson = Encoding.UTF8.GetString(decryptedBytes);
                    
                    // 파일명 추출
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string outputPath = SaveDecryptedJson(decryptedJson, fileName);
                    
                    // 최근 파일 목록에 추가
                    AddToRecentFiles(outputPath);
                    
                    successCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"파일 처리 중 오류: {filePath}\n{ex.Message}");
                    failCount++;
                }
            }

            status = $"배치 처리 완료: {successCount}개 성공, {failCount}개 실패";
            if (successCount > 0)
            {
                status += "\n복호화된 파일은 Assets/DecryptedJson 폴더에 있습니다.";
            }
            
            // 최근 파일 목록이 너무 길면 마지막 10개만 유지
            if (recentFiles.Count > 10)
            {
                recentFiles = recentFiles.GetRange(recentFiles.Count - 10, 10);
            }
        }
        catch (Exception ex)
        {
            status = $"오류: {ex.Message}";
            Debug.LogError($"배치 복호화 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private string SaveDecryptedJson(string json, string fileName)
    {
        // Assets/DecryptedJson 폴더 생성
        string folderPath = "Assets/DecryptedJson";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "DecryptedJson");
        }

        // 복호화된 JSON 파일 저장
        string outputPath = Path.Combine(folderPath, $"{fileName}.json");
        File.WriteAllText(outputPath, json);
        
        // 에셋 데이터베이스 갱신
        AssetDatabase.Refresh();
        
        return outputPath;
    }
    
    private void AddToRecentFiles(string filePath)
    {
        if (!recentFiles.Contains(filePath))
        {
            recentFiles.Add(filePath);
        }
        
        // 최근 파일 목록이 너무 길면 첫 번째 항목 제거
        if (recentFiles.Count > 10)
        {
            recentFiles.RemoveAt(0);
        }
    }
}