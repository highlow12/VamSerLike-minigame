using System.Collections;
using UnityEngine;

public class Spider : NormalMonster
{
    [SerializeField] private float webCooldown = 2f;
    [SerializeField] private float webSlowAmount = 0.5f;
    [SerializeField] private float webDuration = 5f;
    [SerializeField] private float webSize = 1f;
    [SerializeField] private float poisonDamage = 0.5f;
    [SerializeField] private float poisonDuration = 3f;
    [SerializeField] private float attackCooldown = 2f;
    
    [SerializeField] private GameObject webPrefab;
    [SerializeField] private GameObject webProjectilePrefab;
    
    private float lastWebTime = 0f;
    private float lastAttackTime = 0f;
    private Animator animator;

    protected override void Start()
    {
        // Using StraightMovement instead of SinMovement
        movement = new StraightMovement(moveSpeed);
        animator = GetComponent<Animator>(); // Get the animator component
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        
        // Create web on floor periodically
        if (Time.time > lastWebTime + webCooldown && !isDead)
        {
            CreateWebOnFloor();
            lastWebTime = Time.time;
        }
        
        // Animate the spider legs movement
        if (animator != null)
        {
            //animator.SetFloat("MoveSpeed", moveSpeed);
        }
    }

    private void CreateWebOnFloor()
    {
        // Create a web on the floor at the spider's position
        if (webPrefab != null)
        {
            GameObject web = Instantiate(webPrefab, transform.position, Quaternion.identity);
            SpiderWeb webComponent = web.GetComponent<SpiderWeb>();
            
            if (webComponent != null)
            {
                webComponent.Initialize(webSlowAmount, webDuration, webSize);
            }
        }
    }

    protected override void CheckAttackRange()
    {
        // If player is in attack range, shoot web
        if (Vector2.Distance(transform.position, playerTransform.position) < attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    protected override void Attack()
    {
        // Shoot web projectile at player
        ShootWebProjectile();
    }

    private void ShootWebProjectile()
    {
        if (webProjectilePrefab != null)
        {
            // Calculate direction to player
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            
            // Instantiate and setup projectile
            GameObject projectile = Instantiate(webProjectilePrefab, transform.position, Quaternion.identity);
            SpiderWebProjectile webProjectile = projectile.GetComponent<SpiderWebProjectile>();
            
            if (webProjectile != null)
            {
                webProjectile.Initialize(direction, damage, poisonDamage, poisonDuration);
            }
        }
    }
    
    private void OnEnable()
    {
        lastWebTime = 0f; // Reset web timer
        lastAttackTime = 0f; // Reset attack timer
    }
}
