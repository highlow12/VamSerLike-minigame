using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ObjectPoolManager.ObjectInfo))]
public class ObjectPoolManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUIStyle style = new(EditorStyles.helpBox)
        {
            fontSize = 14,
            wordWrap = true,
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(5, 5, 5, 5),
            richText = true
        };

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField(PlayerSkinSO.toolTipText, style);
        EditorGUILayout.Space(5);

        DrawDefaultInspector();
    }
}
