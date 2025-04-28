using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// MirrorUI의 아이템 관련 기능 구현
public partial class MirrorUI
{
    // 아이템 정보 표시
    private void ShowItemInfo(EquipmentItem item)
    {
        // 기존 정보 패널 닫기
        Item_CloseItemInfoPanel();
        
        // 아이템이 장착되어 있는지 확인
        bool isEquipped = false;
        if (item.category == EquipmentCategory.Weapon && equippedWeapon != null && item.itemName == equippedWeapon.itemName)
        {
            isEquipped = true;
        }
        else if (item.category == EquipmentCategory.Cloak && equippedCloak != null && item.itemName == equippedCloak.itemName)
        {
            isEquipped = true;
        }
        
        // 아이템 정보 패널 생성 (로그 출력)
        Debug.Log($"아이템 정보: {item.itemName}, 희귀도: {item.rarity}, 장착 여부: {isEquipped}");
        
        // 실제 UI 패널 생성
        GameObject panel = new GameObject("ItemInfoPanel");
        panel.transform.SetParent(transform);
        currentInfoPanel = panel;
        
        // 여기에 실제 패널 내용 구현
    }
    
    // 장착된 아이템 정보 표시 (MirrorUI_ItemFunctions)
    protected void Item_ShowEquippedItemInfo(EquipmentItem item, EquipmentCategory category)
    {
        if (item == null) return;
        
        ShowItemInfo(item);
    }
    
    // 아이템 정보 패널 닫기
    protected void Item_CloseItemInfoPanel()
    {
        if (currentInfoPanel != null)
        {
            Destroy(currentInfoPanel);
            currentInfoPanel = null;
        }
    }
    
    // 아이템 장착
    public void EquipItem(EquipmentItem item)
    {
        if (item.category == EquipmentCategory.Weapon)
        {
            equippedWeapon = item;
        }
        else if (item.category == EquipmentCategory.Cloak)
        {
            equippedCloak = item;
        }
        
        // 스탯 및 시각적 요소 업데이트
        Core_UpdateEquippedStats();
        
        // 정보 패널 닫기
        Item_CloseItemInfoPanel();
    }
    
    // 아이템 해제
    public void UnequipItem(EquipmentItem item)
    {
        if (item.category == EquipmentCategory.Weapon && equippedWeapon != null && item.itemName == equippedWeapon.itemName)
        {
            equippedWeapon = null;
        }
        else if (item.category == EquipmentCategory.Cloak && equippedCloak != null && item.itemName == equippedCloak.itemName)
        {
            equippedCloak = null;
        }
        
        // 스탯 및 시각적 요소 업데이트
        Core_UpdateEquippedStats();
        
        // 정보 패널 닫기
        Item_CloseItemInfoPanel();
    }
}