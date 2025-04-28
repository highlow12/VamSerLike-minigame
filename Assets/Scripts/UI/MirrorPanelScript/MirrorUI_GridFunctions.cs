using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

// MirrorUI의 그리드 및 UI 관련 기능 구현
public partial class MirrorUI
{
    // 아이템 그리드 채우기 (MirrorUI_GridFunctions)
    protected void Grid_PopulateItemGrid()
    {
        // 기존 자식 오브젝트 제거 방지
        foreach (Transform child in itemGridContainer.transform)
        {
            // 프리팹 파괴하지 않고 유지
            child.gameObject.SetActive(true);
        }

        // 현재 카테고리에 맞는 아이템 필터링
        List<EquipmentItem> filteredItems = equipmentItems
            .Where(item => item.category == currentCategory)
            .ToList();

        // 정렬 로직 추가
        if (currentSortType == SortType.ByRarity)
        {
            filteredItems = filteredItems.OrderBy(item => item.rarity).ToList();
        }
        else if (currentSortType == SortType.ByAcquired)
        {
            filteredItems = filteredItems.OrderBy(item => acquireOrderMap[item]).ToList();
        }

        // 아이템 슬롯 채우기
        for (int i = 0; i < itemGridContainer.transform.childCount; i++)
        {
            ItemSlot itemSlot = itemGridContainer.transform.GetChild(i).GetComponent<ItemSlot>();

            if (i < filteredItems.Count)
            {
                // 스프라이트 로드 로직 (임시)
                Sprite itemSprite = Resources.Load<Sprite>($"Sprites/Weapons/{filteredItems[i].itemName}");

                itemSlot.SetItem(filteredItems[i], itemSprite);
                itemSlot.gameObject.SetActive(true);
            }
            else
            {
                // 남은 슬롯 비우기
                itemSlot.ClearSlot();
            }
        }
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
            items.Sort((a, b) => b.rarity.CompareTo(a.rarity));
            Debug.Log("희귀도 순으로 정렬했습니다.");
        }
        else if (currentSortType == SortType.ByAcquired)
        {
            // 획득 순서 기준 정렬 (acquireOrderMap 사용)
            if (acquireOrderMap != null && acquireOrderMap.Count > 0)
            {
                items.Sort((a, b) => {
                    int orderA = acquireOrderMap.ContainsKey(a) ? acquireOrderMap[a] : 9999;
                    int orderB = acquireOrderMap.ContainsKey(b) ? acquireOrderMap[b] : 9999;
                    return orderA.CompareTo(orderB);
                });
                Debug.Log("획득 순서로 정렬했습니다.");
            }
            else
            {
                // 획득 순서 맵이 없으면 원래 순서 유지
                Debug.Log("획득 순서 데이터가 없어 원래 순서를 유지합니다.");
            }
        }
    }

    // 카테고리 변경 (MirrorUI_GridFunctions)
    protected void Grid_SwitchCategory(EquipmentCategory newCategory)
    {
        currentCategory = newCategory;
        Grid_PopulateItemGrid();

        // 카테고리 버튼 시각적 업데이트 (활성 버튼 강조)
        weaponCategory.GetComponent<Image>().color = (currentCategory == EquipmentCategory.Weapon) ?
            new Color(1f, 0.8f, 0.8f) : Color.white;
        cloakCategory.GetComponent<Image>().color = (currentCategory == EquipmentCategory.Cloak) ?
            new Color(1f, 0.8f, 0.8f) : Color.white;
    }

    // 정렬 방식 변경 (MirrorUI_GridFunctions)
    protected void Grid_SortItems(SortType sortType)
    {
        currentSortType = sortType;
        Grid_PopulateItemGrid();

        // 정렬 버튼 시각적 업데이트 (활성 버튼 강조)
        raritySort.GetComponent<Image>().color = (currentSortType == SortType.ByRarity) ?
            new Color(0.8f, 1f, 0.8f) : Color.white;
        acquiredSort.GetComponent<Image>().color = (currentSortType == SortType.ByAcquired) ?
            new Color(0.8f, 1f, 0.8f) : Color.white;
    }

    // 강화 모드 열기 (MirrorUI_GridFunctions)
    protected void Grid_OpenEnhancementMode()
    {
        Debug.Log("강화 모드로 전환");
        // 강화 모드 구현
        // 실제 구현에서는 별도의 UI 전환 또는 새 패널 오픈
    }

    // 거울 UI 닫기 (MirrorUI_GridFunctions)
    protected void Grid_CloseMirror()
    {
        // 변경사항 저장 및 거울 UI 닫기
        Item_CloseItemInfoPanel();
        mirrorPanel.SetActive(false);
    }
}