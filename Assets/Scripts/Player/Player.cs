using UnityEngine;
using Item;

public class Player : MonoBehaviour
{
    public enum HealType
    {
        Fixed,
        PercentageByMaxHealth,
        PercentageByLostHealth,
    }

    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private SpriteRenderer healthBar;
    public float maxHealth = 100f;
    public float health;
    private float normalizedHealth;

    void Start()
    {
        health = maxHealth;
    }

    void Update()
    {
        normalizedHealth = Mathf.Clamp(health, 0f, maxHealth) / maxHealth;
        healthBar.size = new Vector2(normalizedHealth, healthBar.size.y);
    }

    public void Heal(float amount, HealType healType)
    {
        float newAmount = healType switch
        {
            HealType.Fixed => amount,
            HealType.PercentageByMaxHealth => maxHealth * amount,
            HealType.PercentageByLostHealth => (maxHealth - health) * amount,
            _ => 0,
        };
        health = Mathf.Clamp(health + newAmount, 0f, maxHealth);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<DropItem>(out var dropItem))
        {
            dropItem.UseItem();
        }
    }
}
