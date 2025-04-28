using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// 아이템 정보 패널 컴포넌트
public class ItemInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI itemNameText;         // 아이템 이름
    public TextMeshProUGUI weaponTypeText;       // 무기 유형
    public TextMeshProUGUI descriptionText;      // 설명
    
    [Header("Stats")]
    public TextMeshProUGUI attackDamageText;     // 공격력
    public TextMeshProUGUI attackSpeedText;      // 공격 속도
    public TextMeshProUGUI attackRangeText;      // 공격 범위
    public TextMeshProUGUI attackTargetText;     // 공격 대상
    public TextMeshProUGUI projectileCountText;  // 발사체 수
    public TextMeshProUGUI projectileSpeedText;  // 발사체 속도
    
    [Header("UI Controls")]
    public Button actionButton;                  // 장착/해제 버튼
    public TextMeshProUGUI actionButtonText;
    public Button closeButton;
    
    private EquipmentItem currentItem;
    private bool isEquipped;
    private Action<EquipmentItem> onEquip;
    private Action<EquipmentItem> onUnequip;
    
    // 패널 초기화
    public void Initialize(EquipmentItem item, bool equipped, Action<EquipmentItem> equipAction, Action<EquipmentItem> unequipAction)
    {
        currentItem = item;
        isEquipped = equipped;
        onEquip = equipAction;
        onUnequip = unequipAction;
        
        // 기본 정보 설정
        itemNameText.text = item.itemName;
        descriptionText.text = item.description;
        
        // 무기 유형 설정 (실제 구현에서는 weaponType에 따라 다른 텍스트 표시)
        weaponTypeText.text = GetWeaponTypeText(item.weaponType);
        
        // 스탯 설정
        attackDamageText.text = $"공격력: {item.attackDamage}";
        attackSpeedText.text = $"공격속도: {item.attackSpeed}";
        attackRangeText.text = $"공격범위: {item.attackRange}";
        
        // 추가 스탯 설정 (있는 경우에만)
        if (item.attackTarget > 0)
            attackTargetText.text = $"공격대상: {GetAttackTargetText(item.attackTarget)}";
        else
            attackTargetText.gameObject.SetActive(false);
            
        if (item.projectileCount > 0)
            projectileCountText.text = $"발사체 수: {item.projectileCount}";
        else
            projectileCountText.gameObject.SetActive(false);
            
        if (item.projectileSpeed > 0)
            projectileSpeedText.text = $"발사체 속도: {item.projectileSpeed}";
        else
            projectileSpeedText.gameObject.SetActive(false);
        
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
        
        // 닫기 버튼 설정
        closeButton.onClick.AddListener(Close);
    }
    
    // 무기 유형 텍스트 반환
    private string GetWeaponTypeText(int weaponType)
    {
        switch (weaponType)
        {
            case 0: return "근접 무기";
            case 1: return "원거리 무기";
            case 2: return "마법 무기";
            case 3: return "특수 무기";
            default: return "기타 무기";
        }
    }
    
    // 공격 대상 텍스트 반환
    private string GetAttackTargetText(int attackTarget)
    {
        switch (attackTarget)
        {
            case 0: return "단일 대상";
            case 1: return "다중 대상";
            case 2: return "광역 효과";
            default: return "미지정";
        }
    }
    
    // 패널 닫기
    public void Close()
    {
        Destroy(gameObject);
    }
}