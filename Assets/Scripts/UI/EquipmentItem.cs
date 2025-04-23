using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentItem
{
    public string itemName;
    public EquipmentCategory category;
    public ItemRarity rarity;
    public string description;
    
    // 아이템이 제공하는 능력치 보너스
    public int healthBonus;
    public int defenseBonus;
    public int attackBonus;
    public int attackSpeedBonus;
    public int moveSpeedBonus;
    public int visionRangeBonus;
    
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
        this.attackBonus = attackBonus;
        this.attackSpeedBonus = attackSpeedBonus;
        this.moveSpeedBonus = moveSpeedBonus;
        this.visionRangeBonus = visionRangeBonus;
    }
}