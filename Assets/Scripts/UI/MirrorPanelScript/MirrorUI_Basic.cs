using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class MirrorUI : MonoBehaviour
{
    // Awake 메서드 제거 (다른 클래스에 이미 있음)

    void OnEnable()
    {
        Basic_InitializeUI();

        // weaponDataLoader 사용
        if (weaponDataLoader != null)
        {
            Main_LoadEquipmentItems();
        }
        else
        {
            Basic_LoadMockItems();
        }

        Core_UpdateEquippedStats();
        Grid_PopulateItemGrid();

        if (enhancementPanel != null)
        {
            enhancementPanel.gameObject.SetActive(false); // Ensure the panel is disabled on enable
        }
    }

    private void Basic_InitializeUI()
    {

    }

    private void Basic_LoadMockItems()
    {
        // 기존 코드 유지
        equipmentItems.Clear();
        if (acquireOrderMap != null)
        {
            acquireOrderMap.Clear();
        }

        equipmentItems.Add(new EquipmentItem("십자가", EquipmentCategory.Weapon, ItemRarity.A, "신성한 십자가 무기", 10, 0, 5, 0, 0, 0));
        equipmentItems.Add(new EquipmentItem("일반 검", EquipmentCategory.Weapon, ItemRarity.B, "평범한 검", 0, 0, 3, 5, 0, 0));
        equipmentItems.Add(new EquipmentItem("대검", EquipmentCategory.Weapon, ItemRarity.D, "무거운 대검", 0, 10, 8, 0, -5, 0));
        equipmentItems.Add(new EquipmentItem("단검", EquipmentCategory.Weapon, ItemRarity.B, "가벼운 단검", 0, 0, 2, 15, 5, 0));
        equipmentItems.Add(new EquipmentItem("암시장 샷건", EquipmentCategory.Weapon, ItemRarity.S, "강력한 샷건", 200, 0, 5, 20, 0, 13));

        equipmentItems.Add(new EquipmentItem("일반 망토", EquipmentCategory.Cloak, ItemRarity.B, "평범한 망토", 50, 5, 0, 0, 0, 0));
        equipmentItems.Add(new EquipmentItem("가죽 망토", EquipmentCategory.Cloak, ItemRarity.C, "가죽 재질 망토", 30, 3, 0, 0, 5, 0));
        equipmentItems.Add(new EquipmentItem("강화 망토", EquipmentCategory.Cloak, ItemRarity.A, "강화된 망토", 100, 10, 0, 0, 0, 5));

        // 획득 순서 설정
        if (acquireOrderMap != null)
        {
            for (int i = 0; i < equipmentItems.Count; i++)
            {
                acquireOrderMap[equipmentItems[i]] = i + 1;
            }
        }

        equippedWeapon = equipmentItems[4]; // 암시장 샷건
        equippedCloak = equipmentItems[5];  // 일반 망토
    }
}