using UnityEngine;
using Item;
using System.Collections;

public class Magnet : DropItem
{
    public override void UseItem()
    {
        base.UseItem();
        // Drag items towards player
        GameManager.Instance.ChangeValueForDuration(
            (value) => GameManager.Instance.dragDistanceMultiplier = value,
            () => GameManager.Instance.dragDistanceMultiplier,
            effectValue,
            duration
        );
        GameManager.Instance.ChangeValueForDuration(
            (value) => GameManager.Instance.dragSpeedMultiplier = value,
            () => GameManager.Instance.dragSpeedMultiplier,
            dragSpeed,
            duration
        );
        Destroy(gameObject);
    }

}
