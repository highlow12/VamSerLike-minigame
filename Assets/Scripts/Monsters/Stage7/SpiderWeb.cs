using System.Collections;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    private float slowAmount = 0.5f; // How much to slow player (0.5 means 50% slower)
    private float duration = 5f;     // How long web stays on ground
    private float size = 1f;         // Size of web collider/visual
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D webCollider;
    private bool isInitialized = false;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        webCollider = GetComponent<CircleCollider2D>();
        
        // If no collider exists, add one
        if (webCollider == null)
        {
            webCollider = gameObject.AddComponent<CircleCollider2D>();
            webCollider.isTrigger = true;
        }
    }
    
    public void Initialize(float slowAmount, float duration, float size)
    {
        this.slowAmount = slowAmount;
        this.duration = duration;
        this.size = size;
        
        // Apply size to the transform and collider
        transform.localScale = new Vector3(size, size, 1f);
        
        if (webCollider != null)
        {
            webCollider.radius = 0.5f; // Half the sprite size typically
        }
        
        isInitialized = true;
        StartCoroutine(WebLifetime());
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered web trigger");
            // Apply slow effect to the player
            GameManager.Instance.player.SpeedDebuff = true;
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Keep applying slow effect while player is in web
            GameManager.Instance.player.SpeedDebuff = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Remove slow effect when player leaves web
            GameManager.Instance.player.SpeedDebuff = false;
        }
    }
    
    IEnumerator WebLifetime()
    {
        // Fade in the web
        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            startColor.a = 0;
            spriteRenderer.color = startColor;
            
            // Fade in
            float fadeInTime = 0.5f;
            float timer = 0;
            while (timer < fadeInTime)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(0, 1, timer / fadeInTime);
                spriteRenderer.color = color;
                timer += Time.deltaTime;
                yield return null;
            }
        }
        
        // Wait for the duration
        yield return new WaitForSeconds(duration - 1f); // Subtract fade time
        
        // Fade out before destroying
        if (spriteRenderer != null)
        {
            float fadeOutTime = 1f;
            float timer = 0;
            while (timer < fadeOutTime)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1, 0, timer / fadeOutTime);
                spriteRenderer.color = color;
                timer += Time.deltaTime;
                yield return null;
            }
        }
        
        Destroy(gameObject);
    }
}