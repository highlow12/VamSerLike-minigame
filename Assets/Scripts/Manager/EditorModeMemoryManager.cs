#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
[ExecuteInEditMode]
public class EditorModeMemoryManager
{
    static EditorModeMemoryManager()
    {
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            ClearMemory();
        }
    }

    private static void ClearMemory()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Debug.Log("[Editor] Memory cleared after exiting play mode.");
    }
}
#endif