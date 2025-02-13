using UnityEngine;
using Item;

public class Sight : DropItem
{
    public override void UseItem()
    {
        base.UseItem();
        // Give player sight
        GameManager.Instance.ChangeValueForDuration(
            (value) => GameManager.Instance.playerLight.pointLightOuterRadius = value,
            () => GameManager.Instance.playerLight.pointLightOuterRadius,
            effectValue,
            duration
        );
        Destroy(gameObject);
    }
}
