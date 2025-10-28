using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// MirrorUI의 아이템 관련 기능 구현
public partial class MirrorUI
{
    // 아이템 정보 표시
    private void ShowItemInfo(ItemSlot itemSlot, Vector3 position)
    {
        EquipmentItem item = itemSlot.item;
        if (item == null) return;
        // 기존 정보 패널 닫기
        Item_CloseItemInfoPanel();

        // 아이템이 장착되어 있는지 확인
        bool isEquipped = item.isEquipped;

        /*
        if (item.category == EquipmentCategory.Weapon && equippedWeapon != null && item.itemName == equippedWeapon.itemName)
        {
            isEquipped = true;
        }
        else if (item.category == EquipmentCategory.Cloak && equippedCloak != null && item.itemName == equippedCloak.itemName)
        {
            isEquipped = true;
        }*/

        // 아이템 정보 패널 생성 (로그 출력)
        Debug.Log($"아이템 정보: {item.itemName}, 희귀도: {item.rarity}, 장착 여부: {isEquipped}");

        /*// 실제 UI 패널 생성
        GameObject panel = new GameObject("ItemInfoPanel");
        panel.transform.SetParent(transform);
        currentInfoPanel = panel;
        
        // 여기에 실제 패널 내용 구현*/

        ItemInfoPanel.Instance.gameObject.SetActive(true);
        ItemInfoPanel.Instance.Initialize(item, itemSlot, EquipItem, UnequipItem, position);
    }

    // 장착된 아이템 정보 표시 (MirrorUI_ItemFunctions)
    public void Item_ShowEquippedItemInfo(ItemSlot item, Vector3 position)
    {
        if (item == null) return;

        ShowItemInfo(item, position);
    }
    
    // 아이템 정보 패널 닫기
    protected void Item_CloseItemInfoPanel()
    {
        /*if (currentInfoPanel != null)
        {
            Destroy(currentInfoPanel);
            currentInfoPanel = null;
        }*/

        ItemInfoPanel.Instance?.gameObject.SetActive(false);
        Debug.Log("아이템 정보 패널 닫기");
    }

    // 아이템 장착
    public void EquipItem(ItemSlot itemSlot, EquipmentItem item)
    {
        //현재 itemSlot은 사용 안함;; 나중에 필요할 수도 있어서 남겨둠
        if (item == null) return;

        /*if (item.category == EquipmentCategory.Weapon)
        {
            equippedWeapon.isEquipped = false;
            equippedWeapon = item;
        }
        else if (item.category == EquipmentCategory.Cloak)
        {
            equippedCloak.isEquipped = false;
            equippedCloak = item;
        }

        item.isEquipped = true;*/

        if(item.category == EquipmentCategory.Weapon)
        {
            /*if(weaponSlot.GetComponent<ItemSlot>().SwitchItem(itemSlot))
            {
                equippedWeapon.isEquipped = false;
                equippedWeapon = item;
                equippedWeapon.isEquipped = true;
            }*/
            weaponSlot.GetComponent<ItemSlot>().SetItem(item, weaponDataLoader.GetItemSprite(item.itemName));
            if (equippedWeapon != null)
                equippedWeapon.isEquipped = false;
            equippedWeapon = item;
            equippedWeapon.isEquipped = true;

            HomeInventoryManager.Instance.SaveItemCode("EquippedWeapon", HomeInventoryManager.Instance.GetItemCode(equippedWeapon));
        }
        else if(item.category == EquipmentCategory.Cloak)
        {
            /*if(cloakSlot.GetComponent<ItemSlot>().SwitchItem(itemSlot))
            {
                equippedCloak.isEquipped = false;
                equippedCloak = item;
                equippedCloak.isEquipped = true;
            }*/
            cloakSlot.GetComponent<ItemSlot>().SetItem(item, weaponDataLoader.GetItemSprite(item.itemName));
            if (equippedCloak != null)
                equippedCloak.isEquipped = false;
            equippedCloak = item;
            equippedCloak.isEquipped = true;
            HomeInventoryManager.Instance.SaveItemCode("EquippedCloak", HomeInventoryManager.Instance.GetItemCode(equippedCloak));
        }

        // 스탯 및 시각적 요소 업데이트
        Core_UpdateEquippedStats();
        //AdjustItemGrid();// 아이템 그리드 다시 출력. 장착된 아이템이 포함되지 않도록

        // 생성되어있는 아이템 슬롯에다가 다시 재배치 해주는 함수임. equipped된 item은 건너뜀
        Grid_PopulateItemGrid();
        // 정보 패널 닫기
        Item_CloseItemInfoPanel();
    }

    // 아이템 해제
    public void UnequipItem(EquipmentItem item)
    {
        if (item == null) return;

        if (item.category == EquipmentCategory.Weapon && equippedWeapon != null && item == equippedWeapon)
        {
            equippedWeapon.isEquipped = false;
            weaponSlot.GetComponent<ItemSlot>().ClearSlot();
            equippedWeapon = null;
            HomeInventoryManager.Instance.SaveItemCode("EquippedWeapon", "");
        }
        else if (item.category == EquipmentCategory.Cloak && equippedCloak != null && item == equippedCloak)
        {
            equippedCloak.isEquipped = false;
            cloakSlot.GetComponent<ItemSlot>().ClearSlot();
            equippedCloak = null;
            HomeInventoryManager.Instance.SaveItemCode("EquippedCloak", "");
        }

        // 스탯 및 시각적 요소 업데이트
        Core_UpdateEquippedStats();
        //AdjustItemGrid();// 아이템 그리드 다시 출력. 장착 해제된 아이템도 포함되도록

        // 생성되어있는 아이템 슬롯에다가 다시 재배치 해주는 함수임. equipped된 item은 건너뜀
        Grid_PopulateItemGrid();
        // 정보 패널 닫기
        Item_CloseItemInfoPanel();
    }
    
    public void UnequipAllItems()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.isEquipped = false;
            weaponSlot.GetComponent<ItemSlot>().ClearSlot();
            equippedWeapon = null;
        }

        if (equippedCloak != null)
        {
            equippedCloak.isEquipped = false;
            cloakSlot.GetComponent<ItemSlot>().ClearSlot();
            equippedCloak = null;
        }

        // 스탯 및 시각적 요소 업데이트
        Core_UpdateEquippedStats();
        //AdjustItemGrid();// 아이템 그리드 다시 출력. 장착 해제된 아이템도 포함되도록

        // 생성되어있는 아이템 슬롯에다가 다시 재배치 해주는 함수임. equipped된 item은 건너뜀
        Grid_PopulateItemGrid();
        // 정보 패널 닫기
        Item_CloseItemInfoPanel();
    }
}