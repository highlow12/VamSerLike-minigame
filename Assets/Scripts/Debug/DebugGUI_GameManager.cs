using UnityEngine;

public class DebugGUI_GameManager : MonoBehaviour
{
    public int defaultStage = 1;
    private int selectedStage = 1;
    
    private void Awake()
    {
        // Execute SetStage at Awake with default stage
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetStage(defaultStage);
        }
    }
    
    private void OnGUI()
    {
        // Only show debug GUI in editor or development builds
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Force GUI rendering regardless of stageResources
        GUI.depth = -1000; // Ensure GUI renders on top of everything
        
        GUILayout.BeginArea(new Rect(10, Screen.height - 310, 200, 300));
        GUILayout.Label("Debug Game Manager");
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Stage: ");
        string input = GUILayout.TextField(selectedStage.ToString(), 3, GUILayout.Width(50));
        if (int.TryParse(input, out int value))
        {
            selectedStage = value;
        }
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Set Stage"))
        {
            SetGameStage(selectedStage);
        }
        
        GUILayout.EndArea();
        #endif
    }
    
    // Method to set the game stage
    public void SetGameStage(int stageNumber)
    {
        Debug.Log($"Setting Stage: {stageNumber}");
        if (GameManager.Instance != null)
        {
            bool success = GameManager.Instance.UpdateStageVisuals(stageNumber);
            Debug.LogWarning($"Set Stage {stageNumber}: {(success ? "Success" : "Failed")}");
        }
        else
        {
            Debug.LogWarning("GameManager instance not found!");
        }
    }
}
