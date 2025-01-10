using UnityEngine;

namespace Item
{
    // Drop Item Enums
    public enum DropItemType
    {
        Medikit,
        Magnet,
        Sight
    }
    public enum DropItemEffectType
    {
        Support,
        Attack
    }

    public abstract class DropItem : MonoBehaviour
    {
        public DropItemType dropItemType;
        public DropItemEffectType dropItemEffectType;
        public string dropItemName;
        // effectValue is the value of the effect, like heal amount, attack damage, sight radius, etc.
        public float effectValue;
        public float dragDistance;
        public float dragSpeed;
        // if duration is -1, it just affects once (like medikit)
        public float duration;

        // item effect
        public abstract void UseItem();
    }
}
