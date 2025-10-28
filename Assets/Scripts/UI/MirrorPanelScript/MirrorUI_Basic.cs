using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class MirrorUI
{
    // Awake 메서드 제거 (다른 클래스에 이미 있음)

    void OnEnable()//게임 시작할때와, 미러 패널 열릴때마다 호출됨
    {
        Debug.Log("MirrorUI: OnEnable called");
        Basic_InitializeUI();

        // weaponDataLoader 사용
        if (weaponDataLoader != null)
        {
            Main_LoadEquipmentItems();
        }
        else
        {
            //Basic_LoadMockItems();
            Debug.LogError("MirrorUI: weaponDataLoader is not assigned!");
        }

        Core_UpdateEquippedStats();
        Grid_PopulateItemGrid();

        Grid_SwitchCategory(currentCategory);//초기 카테고리 설정
        Grid_SortItems(currentSortType);//초기 정렬 기준 설정

        if (enhancementPanel != null)
        {
            enhancementPanel.gameObject.SetActive(false); // Ensure the panel is disabled on enable
        }
    }

    private void Basic_InitializeUI()
    {

    }

    
}