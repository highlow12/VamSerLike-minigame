using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// MirrorUI의 부가 기능들을 담당하는 클래스
public class MirrorUIFunctions : MonoBehaviour
{
    private MirrorUI mirrorUI;
    
    void Start()
    {
        mirrorUI = GetComponent<MirrorUI>();
    }
    
    // 장비 슬롯 업데이트
    public void UpdateEquipmentSlot(GameObject slot, EquipmentItem item)
    {
        if (item != null)
        {
            // 아이템 이미지 및 등급 표시
            Image itemImage = slot.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI rarityText = slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            
            // 실제 구현에서는 Resources.Load 또는 에셋 관리 시스템 사용
            itemImage.enabled = true;
            rarityText.text = item.rarity.ToString();
        }
        else
        {
            // 빈 슬롯 표시
            Image itemImage = slot.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI rarityText = slot.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            
            itemImage.enabled = false;
            rarityText.text = "";
        }
    }
    
    // 아이템 목록 정렬
    public void SortItemsList(List<EquipmentItem> items, SortType sortType)
    {
        if (sortType == SortType.ByRarity)
        {
            // 희귀도 기준 정렬 (S, A, B, C, D)
            items.Sort((a, b) => a.rarity.CompareTo(b.rarity));
        }
        else if (sortType == SortType.ByAcquired)
        {
            // 획득 순서 기준 정렬 (이 예제에서는 원래 순서 유지)
            // 실제 구현에서는 획득 시간 데이터 사용
        }
    }
    
    // 아이템 정보 패널 생성
    public GameObject CreateItemInfoPanel(EquipmentItem item, bool isEquipped, GameObject prefab, Transform parent)
    {
        GameObject panel = Instantiate(prefab, parent);
        ItemInfoPanel infoPanelScript = panel.GetComponent<ItemInfoPanel>();
        
        if (infoPanelScript != null)
        {
            infoPanelScript.Initialize(item, isEquipped, 
                (equippedItem) => mirrorUI.EquipItem(equippedItem), 
                (unequippedItem) => mirrorUI.UnequipItem(unequippedItem));
        }
        
        return panel;
    }
    
    // 그리드 크기 조정
    public void AdjustGridSize(GameObject gridContainer, int itemCount, int columns)
    {
        // 아이템 수에 따라 그리드 크기 조정 로직
        int requiredRows = Mathf.CeilToInt((float)itemCount / columns);
        
        // GridLayoutGroup이 있다면 크기 조정
        GridLayoutGroup gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            // 필요에 따라 조정
            Debug.Log($"그리드 행 수 조정: {requiredRows}");
        }
    }
    
    // 플레이어 미리보기 업데이트 (애니메이션, 장비 표시 등)
    public void UpdatePlayerPreview(Image previewImage, EquipmentItem weapon, EquipmentItem cloak)
    {
        // 실제 구현에서는 플레이어 캐릭터의 무기와 망토에 따라 이미지/애니메이션 업데이트
        Debug.Log($"플레이어 미리보기 업데이트: 무기={weapon?.itemName ?? "없음"}, 망토={cloak?.itemName ?? "없음"}");
        
        // 대기 애니메이션 적용 (실제 구현에서는 애니메이션 시스템 사용)
        StartCoroutine(PlayIdleAnimation(previewImage));
    }
    
    // 대기 애니메이션 시뮬레이션 (실제 구현에서는 애니메이션 시스템 사용)
    private IEnumerator PlayIdleAnimation(Image previewImage)
    {
        float time = 0;
        Vector3 originalScale = previewImage.transform.localScale;
        Vector3 targetScale = originalScale * 1.05f;
        
        while (true)
        {
            time += Time.deltaTime;
            float t = (Mathf.Sin(time * 2) + 1) * 0.5f; // 0~1 사이 값
            
            previewImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            
            yield return null;
        }
    }
}