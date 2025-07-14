using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    public Image itemImage;
    public TextMeshProUGUI rarityText;
    public EquipmentItem item;

    public void SetItem(EquipmentItem newItem, Sprite itemSprite = null)
    {
        item = newItem;

        if (itemImage == null)
        {
            Debug.LogError("ItemSlot의 itemImage가 null입니다. Prefab을 확인하세요.");
            return;
        }

        if (rarityText == null)
        {
            Debug.LogError("ItemSlot의 rarityText가 null입니다. Prefab을 확인하세요.");
            return;
        }

        if (item != null)
        {
            if (itemSprite != null)
            {
                itemImage.sprite = itemSprite;
            }
            else
            {
                itemImage.sprite = Resources.Load<Sprite>("graybox"); // 기본 스프라이트 설정
                if (itemImage.sprite == null)
                {
                    Debug.LogError("graybox 스프라이트를 로드할 수 없습니다. 경로를 확인하세요.");
                }
            }
            itemImage.enabled = true;
            rarityText.text = item.rarity.ToString();
        }
        else
        {
            itemImage.enabled = false;
            rarityText.text = "";
        }
    }

    public void ClearSlot()
    {
        item = null;
        itemImage.enabled = false;
        rarityText.text = "";
    }
}