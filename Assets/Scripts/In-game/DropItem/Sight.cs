using UnityEngine;
using Item;

public class Sight : DropItem
{
    // Set item values manually for testing
    void Start()
    {
        dropItemType = DropItemType.Sight;
        dropItemEffectType = DropItemEffectType.Support;
        dropItemName = "Sight";
        effectValue = 20;
        dragDistance = 1;
        dragSpeed = 1;
        duration = 10;
    }

    public override void UseItem()
    {
        // Give player sight
        GameManager.Instance.ChangeValueForDuration(
            (value) => GameManager.Instance.playerLight.pointLightOuterRadius = value,
            () => GameManager.Instance.playerLight.pointLightOuterRadius,
            effectValue,
            duration
        );
        DebugConsole.Line itemLog = new()
        {
            text = $"[{GameManager.Instance.gameTimer}] Player gained {effectValue} sight for {duration} seconds",
            messageType = DebugConsole.MessageType.Local,
            tick = GameManager.Instance.gameTimer
        };
        DebugConsole.Instance.MergeLine(itemLog, "#00FF00");
        Destroy(gameObject);
    }
}
