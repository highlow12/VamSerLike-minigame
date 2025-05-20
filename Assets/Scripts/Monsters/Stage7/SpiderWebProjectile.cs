using System.Collections;
using UnityEngine;

public class SpiderWebProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed = 8f;
    private float damage = 1f;
    private float poisonDamage = 0.5f;
    private float poisonDuration = 3f;
    private float lifetime = 3f;
    
    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }
    
    public void Initialize(Vector2 direction, float damage, float poisonDamage, float poisonDuration)
    {
        this.direction = direction;
        this.damage = damage;
        this.poisonDamage = poisonDamage;
        this.poisonDuration = poisonDuration;
        
        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    private void Update()
    {
        // Move in the direction
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Apply damage to player
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                // Apply immediate damage
                //player.TakeDamage(damage);
                
                // Apply poison effect
                StartCoroutine(ApplyPoisonEffect(player));
            }
            
            // Destroy the projectile
            Destroy(gameObject);
        }
        else if (!collision.CompareTag("Monster") && !collision.CompareTag("Item") && !collision.isTrigger)
        {
            // Hit a wall or other non-monster, non-item object
            Destroy(gameObject);
        }
    }
    
    private IEnumerator ApplyPoisonEffect(Player player)
    {
        float elapsedTime = 0f;
        float tickRate = 1f; // Apply poison damage every second
        float nextTick = tickRate;
        
        while (elapsedTime < poisonDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Apply poison damage at regular intervals
            if (elapsedTime >= nextTick)
            {
                //player.TakeDamage(poisonDamage);
                nextTick += tickRate;
                
                // Visual feedback on poison tick
                GameObject poisonEffect = new GameObject("PoisonEffect");
                poisonEffect.transform.position = player.transform.position;
                
                // Add visual indicator
                SpriteRenderer indicator = poisonEffect.AddComponent<SpriteRenderer>();
                if (indicator != null)
                {
                    // Set up a simple poison indicator
                    indicator.color = new Color(0.5f, 0f, 0.5f, 0.5f); // Purple
                    indicator.sortingOrder = 5;
                    
                    // Destroy after small animation
                    Destroy(poisonEffect, 0.5f);
                }
            }
            
            yield return null;
        }
    }
}