using UnityEngine;
using Item;

public class Expereince : DropItem
{
    // Set item values manually for testing
    void Start()
    {
        dropItemType = DropItemType.Expereince;
        dropItemEffectType = DropItemEffectType.Growth;
        dropItemName = "Expereince";
        effectValue = 50;
        dragDistance = 1;
        dragSpeed = 3;
        duration = -1;
    }

    public void SetExpereinceItemLevel(int level)
    {
        if (level < 1 || level > GameManager.Instance.experienceValues.Count)
        {
            DebugConsole.Line errorLog = new()
            {
                text = $"[{GameManager.Instance.gameTimer}] Invalid level for experience item",
                messageType = DebugConsole.MessageType.Local,
                tick = GameManager.Instance.gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLog, "#FF0000");
            return;
        }
        effectValue = (float)GameManager.Instance.experienceValues[level - 1];
        // Other logics for changing item level
        // changing sprite, color, etc.
    }

    public override void UseItem()
    {
        // Gain expereince
        GameManager.Instance.AddExperience(effectValue);
        DebugConsole.Line itemLog = new()
        {
            text = $"[{GameManager.Instance.gameTimer}] Player gained {effectValue} experience",
            messageType = DebugConsole.MessageType.Local,
            tick = GameManager.Instance.gameTimer
        };
        DebugConsole.Instance.MergeLine(itemLog, "#00FF00");
        try
        {
            GetComponent<PoolAble>().ReleaseObject();
        }
        catch
        {
            Destroy(gameObject);
        }
    }
}
