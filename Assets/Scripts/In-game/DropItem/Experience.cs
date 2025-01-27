using UnityEngine;
using Item;

public class Experience : DropItem
{
    public void SetExpereinceItemLevel(int level)
    {
        if (level < 1 || level > GameManager.Instance.experienceValues.Count)
        {
#if UNITY_EDITOR
            DebugConsole.Line errorLog = new()
            {
                text = $"[{GameManager.Instance.gameTimer}] Invalid level for experience item",
                messageType = DebugConsole.MessageType.Local,
                tick = GameManager.Instance.gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLog, "#FF0000");
#endif
            return;
        }
        effectValue = (float)GameManager.Instance.experienceValues[level - 1];
        // Other logics for changing item level
        // changing sprite, color, etc.
    }

    public override void UseItem()
    {
        base.UseItem();
        // Gain expereince
        GameManager.Instance.AddExperience(effectValue);
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
