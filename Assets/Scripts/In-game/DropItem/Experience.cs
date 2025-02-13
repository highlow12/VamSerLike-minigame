using UnityEngine;
using Item;

public class Experience : DropItem
{
    public void SetExperienceItemLevel(int level)
    {
        if (level < 1)
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
        // set scriptable object
        string path = $"ScriptableObjects/DropItem/Experience/Experience_{level}";
        DropItemSO loadedSO = Resources.Load<DropItemSO>(path);
        if (loadedSO == null)
        {
#if UNITY_EDITOR
            DebugConsole.Line errorLog = new()
            {
                text = $"[{GameManager.Instance.gameTimer}] Failed to load ScriptableObject for {path}",
                messageType = DebugConsole.MessageType.Local,
                tick = GameManager.Instance.gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLog, "#FF0000");
#endif
            return;
        }
        dropItemSO = loadedSO;
    }

    protected override void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"[{gameObject.name}] SpriteRenderer component is missing!");
            return;
        }
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
