using UnityEngine;
using Item;

public class Medikit : DropItem
{
    // Set item values manually for testing
    void Start()
    {
        dropItemType = DropItemType.Medikit;
        dropItemEffectType = DropItemEffectType.Support;
        dropItemName = "Medikit";
        effectValue = 50;
        dragDistance = 1;
        dragSpeed = 1;
        duration = -1;
    }

    public override void UseItem()
    {
        // Heal player
        GameManager.Instance.playerScript.Heal(effectValue);
        DebugConsole.Line itemLog = new()
        {
            text = $"[{GameManager.Instance.gameTimer}] Player healed by {effectValue} HP",
            messageType = DebugConsole.MessageType.Local,
            tick = GameManager.Instance.gameTimer
        };
        DebugConsole.Instance.MergeLine(itemLog, "#00FF00");
        Destroy(gameObject);
    }
}
