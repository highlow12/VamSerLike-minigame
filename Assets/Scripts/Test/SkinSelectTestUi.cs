using UnityEngine;

public class SkinSelectTestUi : MonoBehaviour
{
    public void SkinSelectGUI()
    {
        // Load all skin data
        PlayerSkinSO[] skinData = Resources.LoadAll<PlayerSkinSO>("ScriptableObjects/Player/Skin");

        // Create a button for each skin
        foreach (PlayerSkinSO skin in skinData)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            if (GUILayout.Button(skin.playerSkin.ToString(), buttonStyle, GUILayout.Width(100), GUILayout.Height(30)))
            {
                SelectSkin(skin.playerSkin);
            }
        }
    }

    public void SelectSkin(Player.PlayerSkin skinEnum)
    {
        GameManager.Instance.playerScript.currentPlayerSkin = skinEnum;
    }
}
