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

    public void SetItem(EquipmentItem newItem, Sprite itemSprite)
    {
        item = newItem;
        if (item != null)
        {
            itemImage.sprite = itemSprite;
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