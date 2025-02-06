using UnityEngine;
using Item;

public class Medikit : DropItem
{
    public override void UseItem()
    {
        base.UseItem();
        // Heal player
        GameManager.Instance.playerScript.Heal(effectValue, Player.HealType.PercentageByLostHealth);
        Destroy(gameObject);
    }
}
