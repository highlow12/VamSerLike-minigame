using UnityEngine;

[CreateAssetMenu(fileName = "DropItemSO", menuName = "Scriptable Objects/DropItemSO")]
public class DropItemSO : ScriptableObject
{
    public Item.DropItemType dropItemType;
    public Item.DropItemEffectType dropItemEffectType;
    public string dropItemName;
    public string dropItemDescription;
    public float effectValue;
    public float dragDistance;
    public float dragSpeed;
    public float duration;
    public Sprite dropItemSprite;
}
