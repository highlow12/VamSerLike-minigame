using System;
using UnityEngine;

public class DemoUI : MonoBehaviour
{
    private bool showWeaponUI = true;
    private Vector2 scrollPosition;
    private float startTime;
    
    // Version display string
    [SerializeField] private string versionText = "Version 1.0.0";
    
    private void Start()
    {
        startTime = Time.time;
    }
    
    private void Update()
    {
        // Press Tab key to toggle weapon selection UI
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //showWeaponUI = !showWeaponUI;
        }
    }
    
    private void OnGUI()
    {
        // Display timer at the top center of the screen
        DisplayTimer();
        
        // Display version at bottom left
        DisplayVersion();
        
        // Display close button in top right
        DisplayCloseButton();
        
        if (!showWeaponUI) 
            return;
            
        // Create a window in the left side (moved from right)
        int windowWidth = 200;
        int windowHeight = 300;
        int padding = 10;
        
        // Changed position to left side
        GUI.Box(new Rect(padding, padding, windowWidth, windowHeight), "무기 선택");
        
        // Create a scrollable area for weapon options - updated for left positioning
        Rect scrollViewRect = new Rect(padding + 10, padding + 30, windowWidth - 20, windowHeight - 40);
        Rect contentRect = new Rect(0, 0, windowWidth - 40, 50 * Enum.GetValues(typeof(Weapon.MainWeapon.WeaponType)).Length);
        
        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, contentRect);
        
        int buttonHeight = 40;
        int buttonY = 0;
        
        // Get all weapon types from the enum
        foreach (Weapon.MainWeapon.WeaponType weaponType in Enum.GetValues(typeof(Weapon.MainWeapon.WeaponType)))
        {
            string weaponName = weaponType.ToString();
            
            if (GUI.Button(new Rect(10, buttonY, windowWidth - 60, buttonHeight), weaponName))
            {
                if (ChangeWeaponCommand(weaponName))
                {
                    Debug.Log($"Changed weapon to: {weaponName}");
                }
                else
                {
                    Debug.LogError($"Failed to change weapon to: {weaponName}");
                }
            }
            
            buttonY += buttonHeight + 5;
        }
        
        GUI.EndScrollView();
    }
    
    // New method to display close button
    private void DisplayCloseButton()
    {
        int buttonWidth = 100;
        int buttonHeight = 40;
        int padding = 10;
        
        // Style for close button
        GUIStyle closeButtonStyle = new GUIStyle(GUI.skin.button);
        closeButtonStyle.normal.textColor = Color.white;
        closeButtonStyle.fontSize = 16;
        
        // Draw close button in top right corner
        if (GUI.Button(new Rect(Screen.width - buttonWidth - padding, padding, buttonWidth, buttonHeight), "게임 종료", closeButtonStyle))
        {
            // Close the application
            Debug.Log("Game closing...");
            Application.Quit();
            
            // This helps when testing in the Unity editor
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
    
    private void DisplayVersion()
    {
        // Create style for the version text
        GUIStyle versionStyle = new GUIStyle(GUI.skin.label);
        versionStyle.fontSize = 14;
        versionStyle.normal.textColor = Color.white;
        
        // Calculate the size of the text
        Vector2 textSize = versionStyle.CalcSize(new GUIContent(versionText));
        
        // Position at bottom left with padding
        float padding = 10f;
        
        // Draw background for better visibility
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.Box(new Rect(padding, Screen.height - textSize.y - padding - 5, textSize.x + 10, textSize.y + 5), "");
        GUI.color = Color.white;
        
        // Display the version text
        GUI.Label(new Rect(padding + 5, Screen.height - textSize.y - padding - 2, textSize.x, textSize.y), versionText, versionStyle);
    }
    
    private void DisplayTimer()
    {
        float elapsedTime = Time.time - startTime;
        
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 1000) % 1000);
        
        string timeText = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        
        // Create style for the timer text
        GUIStyle timerStyle = new GUIStyle(GUI.skin.label);
        timerStyle.fontSize = 24;
        timerStyle.fontStyle = FontStyle.Bold;
        timerStyle.normal.textColor = Color.white;
        timerStyle.alignment = TextAnchor.UpperCenter;
        
        // Calculate the size of the text
        Vector2 textSize = timerStyle.CalcSize(new GUIContent(timeText));
        
        // Draw background for better visibility
        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.Box(new Rect((Screen.width - textSize.x - 20) / 2, 10, textSize.x + 20, textSize.y + 10), "");
        GUI.color = Color.white;
        
        // Display the timer
        GUI.Label(new Rect((Screen.width - textSize.x) / 2, 15, textSize.x, textSize.y), timeText, timerStyle);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    bool ChangeWeaponCommand(string parameters)
    {
        var localPlayer = GameManager.Instance.player.gameObject;
        try
        {
            // parse string to enum
            Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(typeof(Weapon.MainWeapon.WeaponType), parameters);
            PlayerAttack playerAttack = localPlayer.GetComponent<PlayerAttack>();
            // remove component
            try
            {
                Destroy(playerAttack.mainWeapon.attackObject);
                Destroy(playerAttack.mainWeapon);
            }
            catch
            {
                //AddLine("No weapon to remove", LineType.Warning);
                Debug.LogWarning("No weapon to remove");
            }
            playerAttack.mainWeapon = null;
            // Find weapon class that named same as weaponType
            Type weaponClassType = Type.GetType(weaponType.ToString());
            Weapon.MainWeapon initWeapon;
            if (weaponClassType != null)
            {
                initWeapon = (Weapon.MainWeapon)localPlayer.AddComponent(weaponClassType);
            }
            else
            {
                //AddLine($"Weapon type {weaponType} not found", LineType.Error);
                Debug.LogError($"Weapon type {weaponType} not found");
                return false;
            }
            initWeapon.weaponRare = playerAttack.GetWeaponRare(weaponType);
            playerAttack.mainWeapon = initWeapon;
            return true;
        }
        catch
        {
            return false;
        }
    }

}
