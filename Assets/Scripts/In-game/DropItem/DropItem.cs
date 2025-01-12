using System.Collections;
using UnityEngine;

namespace Item
{
    // Drop Item Enums
    public enum DropItemType
    {
        Medikit,
        Magnet,
        Sight,
        Expereince
    }
    public enum DropItemEffectType
    {
        Growth,
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

        // item drag
        public Vector3 GetPlayerPosition()
        {
            return GameManager.Instance.playerScript.transform.position;
        }
        public void DragItem(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);
            float newDragDistance = dropItemType == DropItemType.Expereince ? dragDistance * GameManager.Instance.dragDistanceMultiplier : dragDistance;
            float newDragSpeed = dropItemType == DropItemType.Expereince ? dragSpeed * GameManager.Instance.dragSpeedMultiplier : dragSpeed;
            if (distance < dragDistance * newDragDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, dragSpeed * newDragSpeed * Time.deltaTime);
            }
        }

        // unity event
        void Update()
        {
            DragItem(GetPlayerPosition());
        }
    }
}
