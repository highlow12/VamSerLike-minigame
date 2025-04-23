using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// MirrorUI의 그리드 및 UI 관련 기능 구현
public partial class MirrorUI
{
    // 아이템 그리드 채우기
    private void PopulateItemGrid()
    {
        // 기존 아이템 슬롯 제거
        foreach (Transform child in itemGridContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        // 현재 카테고리로 아이템 필터링
        List<EquipmentItem> filteredItems = equipmentItems.FindAll(item => item.category == currentCategory);
        
        // 정렬 적용
        SortItemsList(filteredItems);
        
        // 아이템 슬롯 생성
        for (int i = 0; i < filteredItems.Count; i++)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemGridContainer.transform);
            ItemSlot slot = slotObj.GetComponent<ItemSlot>();
            
            // 더미 스프라이트 생성 (실제 구현에서는 아이템 스프라이트 로드)
            Sprite dummySprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 100, 100), Vector2.zero);
            
            slot.SetItem(filteredItems[i], dummySprite);
            
            // 아이템 클릭 이벤트 설정
            int index = i; // 람다에서 사용하기 위한 인덱스 캡처
            slotObj.GetComponent<Button>().onClick.AddListener(() => ShowItemInfo(filteredItems[index]));
        }
        
        // 필요시 그리드 크기 조정
        AdjustGridSize(filteredItems.Count);
    }
    
    // 그리드 크기 조정
    private void AdjustGridSize(int itemCount)
    {
        // 아이템 수에 따라 그리드 크기 조정 로직
        int requiredRows = Mathf.CeilToInt((float)itemCount / gridColumns);
        Debug.Log($"필요한 행 수: {requiredRows}, 아이템 수: {itemCount}");
        
        // 그리드 레이아웃 조정 (실제 구현에서는 더 복잡할 수 있음)
        GridLayoutGroup grid = itemGridContainer.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            // 필요에 따라 그리드 설정 조정
        }
    }
    
    // 아이템 목록 정렬
    private void SortItemsList(List<EquipmentItem> items)
    {
        if (currentSortType == SortType.ByRarity)
        {
            // 희귀도 기준 정렬 (S, A, B, C, D)
            items.Sort((a, b) => a.rarity.CompareTo(b.rarity));
        }
        else if (currentSortType == SortType.ByAcquired)
        {
            // 획득 순서 기준 정렬 (이 예제에서는 원래 순서 유지)
            // 실제 구현에서는 획득 시간 데이터 사용
        }
    }
    
    // 카테고리 변경
    private void SwitchCategory(EquipmentCategory newCategory)
    {
        currentCategory = newCategory;
        PopulateItemGrid();
        
        // 카테고리 버튼 시각적 업데이트 (활성 버튼 강조)
        weaponCategory.GetComponent<Image>().color = (currentCategory == EquipmentCategory.Weapon) ? 
            new Color(1f, 0.8f, 0.8f) : Color.white;
        cloakCategory.GetComponent<Image>().color = (currentCategory == EquipmentCategory.Cloak) ? 
            new Color(1f, 0.8f, 0.8f) : Color.white;
    }
    
    // 정렬 방식 변경
    private void SortItems(SortType sortType)
    {
        currentSortType = sortType;
        PopulateItemGrid();
        
        // 정렬 버튼 시각적 업데이트 (활성 버튼 강조)
        raritySort.GetComponent<Image>().color = (currentSortType == SortType.ByRarity) ? 
            new Color(0.8f, 1f, 0.8f) : Color.white;
        acquiredSort.GetComponent<Image>().color = (currentSortType == SortType.ByAcquired) ? 
            new Color(0.8f, 1f, 0.8f) : Color.white;
    }
    
    // 강화 모드 열기
    private void OpenEnhancementMode()
    {
        Debug.Log("강화 모드로 전환");
        // 강화 모드 구현
        // 실제 구현에서는 별도의 UI 전환 또는 새 패널 오픈
    }
    
    // 거울 UI 닫기
    private void CloseMirror()
    {
        // 변경사항 저장 및 거울 UI 닫기
        CloseItemInfoPanel();
        mirrorPanel.SetActive(false);
    }
}