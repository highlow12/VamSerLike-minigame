using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StageSpawnPattern))]
public class StageSpawnPatternDataEditor : Editor
{
    private StageSpawnPattern m_Target;

    private void OnEnable()
    {
        m_Target = (StageSpawnPattern)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (m_Target != null)
        {
            EditorGUILayout.BeginHorizontal();

            // Add buttons that allow you to load and save the data from a JSON file
            if (GUILayout.Button("LoadFromJSON"))
            {
                Load(m_Target.jsonFile.text);
            }
            if (GUILayout.Button("SaveToJSON"))
            {
                Save();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void Load(string data)
    {
        if (string.IsNullOrEmpty(data))
            return;

        m_Target.patterns.Clear();

        JArray jArray = JArray.Parse(data);

        foreach (var json in jArray)
        {
            SpawnPatternData patternData = json.ToObject<SpawnPatternData>();
            m_Target.patterns.Add(patternData);
        }
    }

    private void Save()
    {
        if (m_Target.patterns == null || m_Target.patterns.Count == 0)
            return;

        string path = AssetDatabase.GetAssetPath(m_Target);

        var jArray = new JArray();

        foreach (var d in m_Target.patterns)
        {
            JObject jObject = JObject.FromObject(d);
            jArray.Add(jObject);
        }

        string directory = Path.GetDirectoryName(path);
        string assetName = Path.GetFileNameWithoutExtension(path);
        string jsonFilePath = Path.Combine(directory, $"{assetName}.json");

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(jsonFilePath, jArray.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"Saved JSON file at: {jsonFilePath}");
    }

}