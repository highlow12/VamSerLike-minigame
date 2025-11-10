using System;
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


    // 강화 기능을 만들어봅시다..!
    public int enhancementValue = 0;        // 강화 수치

    public bool isEquipped = false;       // 장착 여부

    public List<string> enhancedByRarityDescriptions = new List<string>(); // 희귀도에 따른 강화 설명 리스트
    public bool isEnhancedByRarity = false; //희귀도에 따른 강화 적용 여부

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

    public EquipmentItem Clone()
    {
        EquipmentItem copy = (EquipmentItem)this.MemberwiseClone();// 얕은 복사. 참조 타입 필드는 주의. enum, int, float 등은 괜찮음.
        //copy.optionIds = new List<int>(this.optionIds); // 참조 타입 필드가 있다면 이렇게 새로 복사
        return copy;
    }

    public void SetValuesWithEnhancement()
    {
        if (enhancementValue > 0)
        {
            float enhancementMultiplier = 1 + (0.1f * enhancementValue); // 예:  강화 수치당 10% 증가. 일단 임시 코드임
            attackDamage = Mathf.RoundToInt(attackDamage * enhancementMultiplier);
            attackSpeed = Mathf.RoundToInt(attackSpeed * enhancementMultiplier);
            attackRange = attackRange * enhancementMultiplier;

            healthBonus = Mathf.RoundToInt(healthBonus * enhancementMultiplier);
            defenseBonus = Mathf.RoundToInt(defenseBonus * enhancementMultiplier);
            moveSpeedBonus = Mathf.RoundToInt(moveSpeedBonus * enhancementMultiplier);
            visionRangeBonus = Mathf.RoundToInt(visionRangeBonus * enhancementMultiplier);
        }
    }

    public void SetValuesWithRarity()
    {
        int rarityEnhancement;
        switch (rarity)
        {
            case ItemRarity.S: rarityEnhancement = 3; break;
            case ItemRarity.A: rarityEnhancement = 2; break;
            case ItemRarity.B: rarityEnhancement = 1; break;
            default: rarityEnhancement = 0; break;
        }
        float rarityMultiplier = 1 + (0.2f * rarityEnhancement); // 예:  희귀도 수치당 20% 증가. 일단 임시 코드임

        attackDamage = Mathf.RoundToInt(attackDamage * rarityMultiplier);
        attackSpeed = Mathf.RoundToInt(attackSpeed * rarityMultiplier);
        attackRange = attackRange * rarityMultiplier;

        healthBonus = Mathf.RoundToInt(healthBonus * rarityMultiplier);
        defenseBonus = Mathf.RoundToInt(defenseBonus * rarityMultiplier);
        moveSpeedBonus = Mathf.RoundToInt(moveSpeedBonus * rarityMultiplier);
        visionRangeBonus = Mathf.RoundToInt(visionRangeBonus * rarityMultiplier);
    }

    public List<AddedStatType> GetRandomEnhanceByRarity()
    {
        int rarityEnhancement;
        switch (rarity)
        {
            case ItemRarity.S: rarityEnhancement = 3; break;
            case ItemRarity.A: rarityEnhancement = 2; break;
            case ItemRarity.B: rarityEnhancement = 1; break;
            default: rarityEnhancement = 0; break;
        }


        // 강화할 변수들을 배열로 묶음 (참조를 위해 람다 사용)
        /* List<System.Action> statUpdaters = new List<System.Action>
         {
             () => { attackDamage += Mathf.RoundToInt(attackDamage * rarityMultiplier);
                     enhancedByRarityDescriptions.Add($"공격력: +{Mathf.RoundToInt(attackDamage * rarityMultiplier)}"); },
             () => { attackSpeed += Mathf.RoundToInt(attackSpeed * rarityMultiplier);
                     enhancedByRarityDescriptions.Add($"공격속도: +{Mathf.RoundToInt(attackSpeed * rarityMultiplier)}"); },
             () => { attackRange += Mathf.RoundToInt(attackRange * rarityMultiplier);
                     enhancedByRarityDescriptions.Add($"공격범위: +{Mathf.RoundToInt(attackRange * rarityMultiplier)}"); }
         };*/

        List<AddedStatType> statUpdaters = new List<AddedStatType>
        {
            AddedStatType.AttackDamage,
            AddedStatType.AttackSpeed,
            AddedStatType.AttackRange
        };

        if (category == EquipmentCategory.Cloak)
        {
            // 망토는 공격 외의 스탯도 강화
            //statUpdaters.Add(() => healthBonus += Mathf.RoundToInt(healthBonus * rarityMultiplier));
            /*statUpdaters.Add(
                () =>
                {
                    defenseBonus += Mathf.RoundToInt(defenseBonus * rarityMultiplier);
                    enhancedByRarityDescriptions.Add($"방어력: +{Mathf.RoundToInt(defenseBonus * rarityMultiplier)}");
                });
            statUpdaters.Add(
                () =>
                {
                    moveSpeedBonus += Mathf.RoundToInt(moveSpeedBonus * rarityMultiplier);
                    enhancedByRarityDescriptions.Add($"이동속도: +{Mathf.RoundToInt(moveSpeedBonus * rarityMultiplier)}");
                });
            statUpdaters.Add(
                () =>
                {
                    visionRangeBonus += Mathf.RoundToInt(visionRangeBonus * rarityMultiplier);
                    enhancedByRarityDescriptions.Add($"시야범위: +{Mathf.RoundToInt(visionRangeBonus * rarityMultiplier)}");
                });*/
            statUpdaters.Add(AddedStatType.DefenseBonus);
            statUpdaters.Add(AddedStatType.MoveSpeedBonus);
            statUpdaters.Add(AddedStatType.VisionRangeBonus);
        }

        var result = new List<AddedStatType>();
        for (int i = 0; i < rarityEnhancement; i++)
        {
            int idx = UnityEngine.Random.Range(0, statUpdaters.Count);
            result.Add(statUpdaters[idx]); // 무작위로 선택해서 추가
        }

        Debug.Log($"GetRandomEnhanceByRarity: rarity={rarity}, enhancement count={result.Count}");
        return result;
    }

    public void AddStatByType(List<AddedStatType> statTypes)
    {
        if(!isEnhancedByRarity)
        {
            isEnhancedByRarity = true;
        }
        else
        {
            Debug.LogWarning("AddStatByType: 이미 희귀도 강화가 적용된 아이템에 다시 적용 시도됨. 중복 적용 방지.");
            return;
        }

        int rarityEnhancement;
        switch (rarity)
        {
            case ItemRarity.S: rarityEnhancement = 3; break;
            case ItemRarity.A: rarityEnhancement = 2; break;
            case ItemRarity.B: rarityEnhancement = 1; break;
            default: rarityEnhancement = 0; break;
        }
        float rarityMultiplier = 0.2f * rarityEnhancement; // 예:  희귀도 수치당 20% 증가. 일단 임시 코드임

        int attackDamageDelta = Mathf.RoundToInt(attackDamage * rarityMultiplier);
        int attackSpeedDelta = Mathf.RoundToInt(attackSpeed * rarityMultiplier);
        int attackRangeDelta = Mathf.RoundToInt(attackRange * rarityMultiplier);
        int defenseBonusDelta = Mathf.RoundToInt(defenseBonus * rarityMultiplier);
        int moveSpeedBonusDelta = Mathf.RoundToInt(moveSpeedBonus * rarityMultiplier);
        int visionRangeBonusDelta = Mathf.RoundToInt(visionRangeBonus * rarityMultiplier);

        foreach (var statType in statTypes)
        {
            switch (statType)
            {
                case AddedStatType.AttackDamage:
                    {
                        attackDamage += attackDamageDelta;
                        enhancedByRarityDescriptions.Add($"공격력: +{attackDamageDelta}");
                        break;
                    }
                case AddedStatType.AttackSpeed:
                    {
                        attackSpeed += attackSpeedDelta;
                        enhancedByRarityDescriptions.Add($"공격속도: +{attackSpeedDelta}");
                        break;
                    }
                case AddedStatType.AttackRange:
                    {
                        attackRange += attackRangeDelta;
                        enhancedByRarityDescriptions.Add($"공격범위: +{attackRangeDelta}");
                        break;
                    }
                case AddedStatType.DefenseBonus:
                    {
                        defenseBonus += defenseBonusDelta;
                        enhancedByRarityDescriptions.Add($"방어력: +{defenseBonusDelta}");
                        break;
                    }
                case AddedStatType.MoveSpeedBonus:
                    {
                        moveSpeedBonus += moveSpeedBonusDelta;
                        enhancedByRarityDescriptions.Add($"이동속도: +{moveSpeedBonusDelta}");
                        break;
                    }
                case AddedStatType.VisionRangeBonus:
                    {
                        visionRangeBonus += visionRangeBonusDelta;
                        enhancedByRarityDescriptions.Add($"시야범위: +{visionRangeBonusDelta}");
                        break;
                    }
            }
        }
    }


}

public enum AddedStatType
{
    AttackDamage,
    AttackSpeed,
    AttackRange,
    DefenseBonus,
    MoveSpeedBonus,
    VisionRangeBonus
}