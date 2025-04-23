using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI baseStatsText;
    public TextMeshProUGUI bonusStatsText;
    public Button actionButton; // 장착 또는 해제 버튼
    public TextMeshProUGUI actionButtonText;
    
    private EquipmentItem currentItem;
    private bool isEquipped;
    private Action<EquipmentItem> onEquip;
    private Action<EquipmentItem> onUnequip;
    
    public void Initialize(EquipmentItem item, bool equipped, Action<EquipmentItem> equipAction, Action<EquipmentItem> unequipAction)
    {
        currentItem = item;
        isEquipped = equipped;
        onEquip = equipAction;
        onUnequip = unequipAction;
        
        // 아이템 정보 표시
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        
        // 기본 능력치 표시
        baseStatsText.text = "[기본 능력치]\n";
        if (item.attackBonus > 0)
            baseStatsText.text += $"공격력: {item.attackBonus}\n";
        
        // 추가 능력치 표시
        bonusStatsText.text = "[부가 능력치]\n";
        if (item.healthBonus > 0)
            bonusStatsText.text += $"추가체력: {item.healthBonus}\n";
        if (item.defenseBonus > 0)
            bonusStatsText.text += $"방어력: {item.defenseBonus}\n";
        if (item.attackSpeedBonus > 0)
            bonusStatsText.text += $"공격속도: {item.attackSpeedBonus}%\n";
        if (item.moveSpeedBonus > 0)
            bonusStatsText.text += $"이동속도: {item.moveSpeedBonus}%\n";
        if (item.visionRangeBonus > 0)
            bonusStatsText.text += $"시야범위: {item.visionRangeBonus}%\n";
        
        // 버튼 설정
        if (isEquipped)
        {
            actionButtonText.text = "장착 해제";
            actionButton.onClick.AddListener(() => onUnequip(currentItem));
        }
        else
        {
            actionButtonText.text = "장착";
            actionButton.onClick.AddListener(() => onEquip(currentItem));
        }
    }
    
    // 패널 닫기
    public void Close()
    {
        Destroy(gameObject);
    }
    
    // 클릭 이벤트가 패널 외부에서 발생할 경우 패널 닫기
    public void OnClickOutside()
    {
        Close();
    }
}