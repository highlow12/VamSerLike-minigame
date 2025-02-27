using System;
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
        testUIs.Add(new TestUI
        {
            name = "Skin Select",
            guiAction = skinSelectTestUi.SkinSelectGUI,
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

        foreach (var testUI in testUIs)
        {
            if (GUILayout.Button($"Toggle {testUI.name}", buttonStyle, GUILayout.Width(120), GUILayout.Height(30)))
            {
                TestUI temp = testUI;
                temp.show = !temp.show;
                int index = testUIs.IndexOf(testUI);
                testUIs[index] = temp;
            }

            if (testUI.show)
            {
                testUI.guiAction?.Invoke();
            }
        }
    }
}
#endif
