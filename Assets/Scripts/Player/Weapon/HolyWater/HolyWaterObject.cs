using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class HolyWaterObject : Projectile
{
    [SerializeField] private Sprite holyWaterSplashSprite;
    [SerializeField] private Sprite holyWaterProjectileSprite;
    [SerializeField] private float projectileInitialScale;
    private PolygonCollider2D polygonCollider;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private bool isInSplashState = false;
    private readonly float attackIntervalInSeconds = 0.5f;
    private int attackIntervalInTicks;
    [SerializeField] private List<Collider2D> hits = new List<Collider2D>();
    public LayerMask monsterLayer;
    private ContactFilter2D contactFilter;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        attackIntervalInTicks = (int)(attackIntervalInSeconds / Time.fixedDeltaTime);
        contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(monsterLayer);
        Debug.Log($"Monster Layer: {monsterLayer.value}");

    }

    private void FixedUpdate()
    {
        if (isInSplashState)
        {
            // contatctFilter is not working properly
            Physics2D.OverlapCollider(polygonCollider, hits);
            foreach (Collider2D hit in hits)
            {
                if (hit == null)
                {
                    break;
                }
                Monster monster = hit.GetComponent<Monster>();
                if (monster == null)
                {
                    continue;
                }
                // Monsters already in monstersHit will be attacked again after attackIntervalInSeconds
                if (monstersHit.Any(x => x.monster == monster && x.hitTime + attackIntervalInTicks >= GameManager.Instance.gameTimer))
                {
                    continue;
                }
                Debug.Log($"[{GameManager.Instance.gameTimer}] HolyWaterObject hit {monster.name}");
                Weapon.MonsterHit appendedMonsterHit = monstersHit.FirstOrDefault(x => x.monster == monster);
                if (default(Weapon.MonsterHit).Equals(appendedMonsterHit))
                {
                    Weapon.MonsterHit monsterHit = new()
                    {
                        monster = monster,
                        hitTime = GameManager.Instance.gameTimer
                    };
                    monstersHit.Add(monsterHit);
                }
                int index = monstersHit.FindIndex(x => x.monster == monster);
                if (index != -1)
                {
                    monstersHit[index] = new Weapon.MonsterHit
                    {
                        monster = monster,
                        hitTime = GameManager.Instance.gameTimer
                    };
                }
                monster.TakeDamage(attackDamage);
            }
        }
        else
        {
            hits.Clear();
        }
    }

    public void ChangeSprite()
    {
        isInSplashState = true;
        spriteRenderer.sprite = holyWaterSplashSprite;
        spriteRenderer.sortingLayerName = "Splash";
        // update polygon collider
        polygonCollider.TryUpdateShapeToAttachedSprite();
        float scale = attackRange;
        Debug.Log($"localScale: {transform.localScale.x}, attackRange: {attackRange}, scale: {scale}");
        transform.localScale = new Vector3(scale, scale, 1);
        StartCoroutine(Despawn());
    }

    // This method handles the despawning of the holy water object after a delay
    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(3f);
        transform.localScale = new Vector3(projectileInitialScale, projectileInitialScale, 1);
        spriteRenderer.sprite = holyWaterProjectileSprite;
        spriteRenderer.sortingLayerName = "Projectile";
        // update polygon collider
        polygonCollider.TryUpdateShapeToAttachedSprite();
        isInSplashState = false;
        if (TryGetComponent<PoolAble>(out var poolAble))
        {
            poolAble.ReleaseObject();
        }
    }


}
