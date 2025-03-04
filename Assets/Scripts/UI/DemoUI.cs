using System;
using UnityEngine;

public class DemoUI : MonoBehaviour
{
    private bool showWeaponUI = true;
    private Vector2 scrollPosition;
    
    // Add this Update method to check for key input
    private void Update()
    {
        // Press Tab key to toggle weapon selection UI
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //showWeaponUI = !showWeaponUI;
        }
    }
    
    // Add this OnGUI method to render the weapon selection UI
    private void OnGUI()
    {
        if (!showWeaponUI) 
            return;
            
        // Create a window in the top right corner
        int windowWidth = 200;
        int windowHeight = 300;
        int padding = 10;
        
        GUI.Box(new Rect(Screen.width - windowWidth - padding, padding, windowWidth, windowHeight), "무기 선택");
        
        // Create a scrollable area for weapon options
        Rect scrollViewRect = new Rect(Screen.width - windowWidth - padding + 10, padding + 30, windowWidth - 20, windowHeight - 40);
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
