using UnityEngine;

public class Snowman : NormalMonster
{
    [SerializeField] private float projectileInterval = 3f; // n초마다 발사 (기본값: 3초)
    [SerializeField] private float projectileSpeed = 5f; // 투사체 속도
    [SerializeField] private float projectileLifetime = 3f; // 투사체 수명
    [SerializeField] private GameObject projectilePrefab; // 투사체 프리팹

    private float lastProjectileTime;

    protected override void Start()
    {
        movement = new StraightMovement(moveSpeed);
        base.Start();
        lastProjectileTime = Time.time;
    }

    protected override void Update()
    {
        base.Update();
        
        if (!isDead && playerTransform != null)
        {
            // n초마다 투사체 발사
            if (Time.time >= lastProjectileTime + projectileInterval)
            {
                ShootProjectile();
                lastProjectileTime = Time.time;
            }
        }
    }

    private void ShootProjectile()
    {
        // 프리팹이 없으면 오브젝트 생성
        if (projectilePrefab == null)
        {
            GameObject snowball = new GameObject("Snowball");
            snowball.transform.position = transform.position;
            
            // 컴포넌트 추가
            SpriteRenderer renderer = snowball.AddComponent<SpriteRenderer>();
            renderer.sprite = Resources.Load<Sprite>("Sprites/Snowball"); // 적절한 스프라이트 경로로 수정 필요
            
            CircleCollider2D collider = snowball.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            
            Rigidbody2D rb = snowball.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            SnowballProjectile projectileScript = snowball.AddComponent<SnowballProjectile>();
            projectileScript.damage = damage;
            projectileScript.speed = projectileSpeed;
            projectileScript.lifetime = projectileLifetime;
            
            // 플레이어 방향으로 발사
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            projectileScript.direction = direction;
        }
        else
        {
            // 프리팹이 있으면 인스턴스화
            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            SnowballProjectile projectileScript = projectile.GetComponent<SnowballProjectile>();
            if (projectileScript != null)
            {
                projectileScript.damage = damage;
                projectileScript.speed = projectileSpeed;
                projectileScript.lifetime = projectileLifetime;
                
                // 플레이어 방향으로 발사
                Vector2 direction = (playerTransform.position - transform.position).normalized;
                projectileScript.direction = direction;
            }
        }
    }

    // NormalMonster의 Attack 메서드 오버라이드
    protected override void Attack()
    {
        // 기본 공격은 투사체 발사로 대체
        ShootProjectile();
    }

    protected override void CheckAttackRange()
    {
        // 공격 범위 내에 있으면 공격
        Attack();
    }
}
