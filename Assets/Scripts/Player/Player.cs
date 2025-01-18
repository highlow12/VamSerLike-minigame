using UnityEngine;
using Item;

public class Player : MonoBehaviour
{
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

    public void Heal(float amount)
    {
        health = Mathf.Clamp(health + amount, 0f, maxHealth);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<DropItem>(out var dropItem))
        {
            dropItem.UseItem();
        }
    }
}
