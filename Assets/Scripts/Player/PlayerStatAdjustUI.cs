using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;

public class PlayerStatAdjustUI : MonoBehaviour
{
    private bool showUI = false;
    private int tabIndex = 0;
    private readonly string[] tabNames = { "Player", "PlayerMove", "PlayerAttack" };
    private Component player;
    private Component playerMove;
    private Component playerAttack;
    private FieldInfo[][] tabFields;
    private object[] tabTargets;
    private Dictionary<string, string> inputStrings = new();

    void Awake()
    {
        player = FindFirstObjectByType<Player>();
        playerMove = FindFirstObjectByType<PlayerMove>();
        playerAttack = FindFirstObjectByType<PlayerAttack>();
        tabTargets = new object[] { player, playerMove, playerAttack };
        tabFields = new FieldInfo[3][];
        tabFields[0] = player.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Where(f => (f.FieldType == typeof(int) || f.FieldType == typeof(float)))
            .ToArray();
        tabFields[1] = playerMove.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Where(f => (f.FieldType == typeof(int) || f.FieldType == typeof(float)))
            .ToArray();
        tabFields[2] = playerAttack.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Where(f => (f.FieldType == typeof(int) || f.FieldType == typeof(float)))
            .ToArray();
        LoadAllStats();
    }

    void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            showUI = !showUI;
        }
    }

    void OnGUI()
    {
        if (!showUI) return;
        GUI.Window(0, new Rect(20, 20, 400, 500), DrawWindow, "Player Stat Adjuster");
    }

    void DrawWindow(int id)
    {
        GUILayout.BeginHorizontal();
        for (int i = 0; i < tabNames.Length; i++)
        {
            if (GUILayout.Toggle(tabIndex == i, tabNames[i], "Button"))
                tabIndex = i;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        DrawFields(tabFields[tabIndex], tabTargets[tabIndex]);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save"))
        {
            SaveAllStats();
        }
        if (GUILayout.Button("Load"))
        {
            LoadAllStats();
        }
    }

    void DrawFields(FieldInfo[] fields, object target)
    {
        foreach (var field in fields)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(field.Name, GUILayout.Width(180));
            string key = $"{tabNames[tabIndex]}_{field.Name}";
            if (!inputStrings.ContainsKey(key))
            {
                if (field.FieldType == typeof(int))
                    inputStrings[key] = field.GetValue(target).ToString();
                else if (field.FieldType == typeof(float))
                    inputStrings[key] = ((float)field.GetValue(target)).ToString("F2");
            }
            string prevStr = inputStrings[key];
            string newStr = GUILayout.TextField(prevStr, GUILayout.Width(80));
            if (newStr != prevStr)
            {
                inputStrings[key] = newStr;
                // 값이 비어있으면 0으로 처리
                if (string.IsNullOrEmpty(newStr) || newStr == "-")
                {
                    // 입력 중이므로 값 반영하지 않음
                }
                else if (field.FieldType == typeof(int) && int.TryParse(newStr, out int newVal))
                {
                    field.SetValue(target, newVal);
                }
                else if (field.FieldType == typeof(float) && float.TryParse(newStr, out float newValF))
                {
                    field.SetValue(target, newValF);
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    void SaveAllStats()
    {
        for (int t = 0; t < tabFields.Length; t++)
        {
            foreach (var field in tabFields[t])
            {
                string key = $"StatAdjust_{tabNames[t]}_{field.Name}";
                string inputKey = $"{tabNames[t]}_{field.Name}";
                object val = field.GetValue(tabTargets[t]);
                // 저장 시 입력 문자열이 유효하면 값 반영
                if (inputStrings.ContainsKey(inputKey))
                {
                    string str = inputStrings[inputKey];
                    if (field.FieldType == typeof(int) && int.TryParse(str, out int newVal))
                        val = newVal;
                    else if (field.FieldType == typeof(float) && float.TryParse(str, out float newValF))
                        val = newValF;
                    field.SetValue(tabTargets[t], val);
                }
                if (field.FieldType == typeof(int))
                    PlayerPrefs.SetInt(key, (int)val);
                else if (field.FieldType == typeof(float))
                    PlayerPrefs.SetFloat(key, (float)val);
            }
        }
        PlayerPrefs.Save();
    }

    void LoadAllStats()
    {
        for (int t = 0; t < tabFields.Length; t++)
        {
            foreach (var field in tabFields[t])
            {
                string key = $"StatAdjust_{tabNames[t]}_{field.Name}";
                string inputKey = $"{tabNames[t]}_{field.Name}";
                if (field.FieldType == typeof(int) && PlayerPrefs.HasKey(key))
                {
                    int v = PlayerPrefs.GetInt(key);
                    field.SetValue(tabTargets[t], v);
                    inputStrings[inputKey] = v.ToString();
                }
                else if (field.FieldType == typeof(float) && PlayerPrefs.HasKey(key))
                {
                    float v = PlayerPrefs.GetFloat(key);
                    field.SetValue(tabTargets[t], v);
                    inputStrings[inputKey] = v.ToString("F2");
                }
            }
        }
    }
}
