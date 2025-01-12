using UnityEngine;
using Item;
using System.Collections;

public class Magnet : DropItem
{
    // Set item values manually for testing
    void Start()
    {
        dropItemType = DropItemType.Magnet;
        dropItemEffectType = DropItemEffectType.Support;
        dropItemName = "Magnet";
        effectValue = 10000 * 10000; // just a big number
        dragDistance = 0;
        dragSpeed = 2;
        duration = 10;
    }

    public override void UseItem()
    {
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
        DebugConsole.Line itemLog = new()
        {
            text = $"[{GameManager.Instance.gameTimer}] Magnet activated",
            messageType = DebugConsole.MessageType.Local,
            tick = GameManager.Instance.gameTimer
        };
        DebugConsole.Instance.MergeLine(itemLog, "#00FF00");
        Destroy(gameObject);
    }

}
