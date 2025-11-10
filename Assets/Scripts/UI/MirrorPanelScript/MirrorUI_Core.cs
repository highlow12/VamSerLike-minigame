using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// MirrorUI의 핵심 기능들을 담당하는 클래스
public partial class MirrorUI
{
    // 장착된 장비 기반으로 스탯 업데이트 (MirrorUI_Core)
    protected void Core_UpdateEquippedStats()
    {
        Debug.Log("Core_UpdateEquippedStats 호출됨");
        int baseHealth = 500;
        int baseDefense = 5;
        int baseAttack = 5;
        int baseAttackSpeed = 10;
        int baseMoveSpeed = 10;
        int baseVisionRange = 10;
        
        int totalHealth = baseHealth;
        int totalDefense = baseDefense;
        int totalAttack = baseAttack;
        int totalAttackSpeed = baseAttackSpeed;
        int totalMoveSpeed = baseMoveSpeed;
        int totalVisionRange = baseVisionRange;
        
        if (equippedWeapon != null)
        {
            totalHealth += equippedWeapon.healthBonus;
            totalDefense += equippedWeapon.defenseBonus;
            totalAttack += equippedWeapon.attackDamage;
            totalAttackSpeed += equippedWeapon.attackSpeed;
            totalMoveSpeed += equippedWeapon.moveSpeedBonus;
            totalVisionRange += equippedWeapon.visionRangeBonus;
        }
        
        if (equippedCloak != null)
        {
            totalHealth += equippedCloak.healthBonus;
            totalDefense += equippedCloak.defenseBonus;
            totalAttack += equippedCloak.attackDamage;
            totalAttackSpeed += equippedCloak.attackSpeed;
            totalMoveSpeed += equippedCloak.moveSpeedBonus;
            totalVisionRange += equippedCloak.visionRangeBonus;
        }
        
        // 스탯 텍스트 업데이트
        healthText.text = "체력: " + totalHealth;
        defenseText.text = "방어력: " + totalDefense;
        attackText.text = "공격력: " + totalAttack;
        attackSpeedText.text = "공격속도: " + totalAttackSpeed + "%";
        moveSpeedText.text = "이동속도: " + totalMoveSpeed + "%";
        visionRangeText.text = "시야범위: " + totalVisionRange + "%";
        
        // 장착된 아이템 시각적 업데이트
        UpdateEquippedItemVisuals();
    }
    
    // 장착된 아이템의 시각적 표현 업데이트
    private void UpdateEquippedItemVisuals()
    {
        // 무기 슬롯 업데이트
        UpdateEquipmentSlot(weaponSlot, equippedWeapon);
        
        // 망토 슬롯 업데이트
        UpdateEquipmentSlot(cloakSlot, equippedCloak);
        
        // 플레이어 미리보기 업데이트
        UpdatePlayerPreview();
    }
    
    // 장비 슬롯 업데이트
    private void UpdateEquipmentSlot(GameObject slot, EquipmentItem item)
    {
        if (item != null)
        {
            slot.GetComponent<ItemSlot>().SetItem(item, weaponDataLoader.GetItemSprite(item.itemName));
            // 아이템 이미지 및 등급 표시
            Image itemImage = slot.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI rarityText = slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            
            // 실제 구현에서는 Resources.Load 또는 에셋 관리 시스템 사용
            //itemImage.enabled = true;
            slot.GetComponent<ItemSlot>().SetItem(item, weaponDataLoader.GetItemSprite(item.itemName));
            rarityText.text = item.rarity.ToString();
        }
        else
        {
            // 빈 슬롯 표시
            Image itemImage = slot.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI rarityText = slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            
            //itemImage.enabled = false;
            slot.GetComponent<ItemSlot>().ClearSlot();
            rarityText.text = "";
        }
    }

    // 플레이어 미리보기 업데이트
    private void UpdatePlayerPreview()
    {
        // 실제 구현에서는 플레이어 모델/애니메이션 업데이트
        Debug.Log("플레이어 미리보기 업데이트됨");
        ChangeEquippedWeaponPreview();

        // 대기 애니메이션 효과 (간단한 예시)
        StartCoroutine(PlayerIdleAnimation());
    }
    
    public void ChangeEquippedWeaponPreview()
    {
        if (weaponPreviewImage == null) return;
        
        if (equippedWeapon != null)
        {
            Debug.Log("장착된 무기 미리보기 업데이트: " + equippedWeapon.itemName);
            weaponPreviewImage.sprite = weaponDataLoader.GetItemSprite(equippedWeapon.itemName);
            weaponPreviewImage.enabled = true;
        }
        else
        {
            Debug.Log("장착된 무기 없음 - 미리보기 비활성화");
            weaponPreviewImage.enabled = false;
            weaponPreviewImage.sprite = null;
        }
    }
    
    // 대기 애니메이션 (간단한 예시)
    private IEnumerator PlayerIdleAnimation()
    {
        Vector3 originalScale = playerPreviewImage.transform.localScale;
        Vector3 targetScale = originalScale * idleMultiplier;
        Vector3 imagePos = playerPreviewImage.transform.localPosition;
        
        while (true)
        {
            float t = 0;
            while (t < idleInterval)
            {
                t += Time.deltaTime;
                float normalizedTime = t / idleInterval;
                float scaleValue = Mathf.Sin(normalizedTime * Mathf.PI);
                playerPreviewImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, scaleValue);
                playerPreviewImage.transform.localPosition = imagePos + new Vector3(0, Mathf.Sin(normalizedTime * Mathf.PI), 0) * idleMovingMultiplier;
                //Debug.Log($"플레이어 미리보기 애니메이션 스케일: {playerPreviewImage.transform.localScale}");
                yield return null;
            }
            
            yield return null;
        }
    }
}