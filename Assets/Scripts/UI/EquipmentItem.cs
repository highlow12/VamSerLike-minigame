using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentItem
{
    // 기본 정보
    public string itemName;              // 아이템 이름
    public EquipmentCategory category;   // 장비 카테고리 (무기, 망토)
    public string description;           // 아이템 설명
    
    // 무기 속성
    public int weaponType;               // 무기 유형 (0: 근접, 1: 원거리, 2: 마법 등)
    public string displayRarity;         // 표시용 희귀도 문자열
    public int rarityValue;              // 희귀도 수치값
    public ItemRarity rarity;            // 희귀도 열거형 (S, A, B, C, D)
    
    // 전투 스탯
    public int attackDamage;             // 공격력
    public int attackSpeed;              // 공격 속도
    public string displayAttackRange;    // 표시용 공격 범위
    public int attackRangeValue;         // 공격 범위 정수값
    public float attackRange;            // 공격 범위 실수값
    public string displayAttackTarget;   // 표시용 공격 대상
    public int attackTarget;             // 공격 대상
    public int projectileCount;          // 발사체 수
    public int projectileSpeed;          // 발사체 속도
    
    // 추가 보너스 스탯 (망토 등을 위한 속성)
    public int healthBonus;              // 추가 체력
    public int defenseBonus;             // 방어력
    public int moveSpeedBonus;           // 이동 속도
    public int visionRangeBonus;         // 시야 범위
    
    // 기본 생성자
    public EquipmentItem() { }
    
    // 무기 생성자
    public EquipmentItem(string name, string displayRarity, int rarityValue, int weaponType, string description,
                         int attackDamage, int attackSpeed, float attackRange, int attackTarget, 
                         int projectileCount, int projectileSpeed)
    {
        this.itemName = name;
        this.category = EquipmentCategory.Weapon;
        this.displayRarity = displayRarity;
        this.rarityValue = rarityValue;
        this.rarity = ConvertRarityValue(rarityValue);
        this.weaponType = weaponType;
        this.description = description;
        
        this.attackDamage = attackDamage;
        this.attackSpeed = attackSpeed;
        this.attackRange = attackRange;
        this.attackTarget = attackTarget;
        this.projectileCount = projectileCount;
        this.projectileSpeed = projectileSpeed;
    }
    
    // 망토 생성자
    public EquipmentItem(string name, EquipmentCategory category, ItemRarity rarity, string description,
                         int healthBonus = 0, int defenseBonus = 0, int attackBonus = 0,
                         int attackSpeedBonus = 0, int moveSpeedBonus = 0, int visionRangeBonus = 0)
    {
        this.itemName = name;
        this.category = category;
        this.rarity = rarity;
        this.description = description;
        
        this.healthBonus = healthBonus;
        this.defenseBonus = defenseBonus;
        this.attackDamage = attackBonus;
        this.attackSpeed = attackSpeedBonus;
        this.moveSpeedBonus = moveSpeedBonus;
        this.visionRangeBonus = visionRangeBonus;
    }
    
    // 희귀도 값을 열거형으로 변환
    private ItemRarity ConvertRarityValue(int rarityValue)
    {
        switch (rarityValue)
        {
            case 0: return ItemRarity.D;
            case 1: return ItemRarity.C;
            case 2: return ItemRarity.B;
            case 3: return ItemRarity.A;
            case 4: return ItemRarity.S;
            default: return ItemRarity.C;
        }
    }
}