using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class TestUi : MonoBehaviour
{
    [Serializable]
    public struct TestUI
    {
        public string name;
        public Action guiAction;
        public bool show;
    }

    public List<TestUI> testUIs = new();

    void Start()
    {
        SkinSelectTestUi skinSelectTestUi = gameObject.AddComponent<SkinSelectTestUi>();
        MonsterPatternTestUi monsterPatternTestUi = gameObject.AddComponent<MonsterPatternTestUi>();
        MonsterSpawnTestUi monsterSpawnTestUi = gameObject.AddComponent<MonsterSpawnTestUi>();
        PlayerSpecialEffectTestUi playerSpecialEffectTestUi = gameObject.AddComponent<PlayerSpecialEffectTestUi>();

        testUIs.Add(new TestUI
        {
            name = "Skin Select",
            guiAction = skinSelectTestUi.SkinSelectGUI,
            show = false
        });

        testUIs.Add(new TestUI
        {
            name = "Monster Pattern",
            guiAction = monsterPatternTestUi.MonsterPatternGUI,
            show = false
        });

        testUIs.Add(new TestUI
        {
            name = "Monster Spawn",
            guiAction = monsterSpawnTestUi.MonsterSpawnGUI,
            show = false
        });

        testUIs.Add(new TestUI
        {
            name = "Special Effects",
            guiAction = playerSpecialEffectTestUi.PlayerSpecialEffectGUI,
            show = false
        });
    }

    void OnGUI()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter
        };
        GUILayout.BeginHorizontal();
        for (int i = 0; i < testUIs.Count(); i++)
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button($"Toggle {testUIs[i].name}", buttonStyle, GUILayout.Width(120), GUILayout.Height(30)))
            {
                TestUI temp = testUIs[i];
                temp.show = !temp.show;
                testUIs[i] = temp;
            }

            if (testUIs[i].show)
            {
                testUIs[i].guiAction?.Invoke();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
    }
}
#endif
