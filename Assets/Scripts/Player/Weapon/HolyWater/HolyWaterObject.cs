using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class HolyWaterObject : Projectile
{
    [SerializeField] private Sprite holyWaterSplashSprite;
    [SerializeField] private float projectileInitialScale;
    [SerializeField] private float projectileScale;
    private PolygonCollider2D polygonCollider;
    private SpriteRenderer spriteRenderer;
    private bool isInSplashState = false;
    private readonly float attackIntervalInSeconds = 0.5f;
    private int attackIntervalInTicks;
    private List<Collider2D> hits = new List<Collider2D>();

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        attackIntervalInTicks = (int)(attackIntervalInSeconds / Time.fixedDeltaTime);
    }

    private void FixedUpdate()
    {
        if (isInSplashState)
        {
            hits.Clear();
            ContactFilter2D contactFilter = new();
            contactFilter.useLayerMask = true;
            contactFilter.layerMask = LayerMask.GetMask("Monster");
            Physics2D.OverlapCollider(polygonCollider, contactFilter, hits);
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
    }

    public void ChangeSprite()
    {
        isInSplashState = true;
        spriteRenderer.sprite = holyWaterSplashSprite;
        // update polygon collider
        polygonCollider.TryUpdateShapeToAttachedSprite();
        float scale = 1 / transform.localScale.x * attackRange;
        transform.localScale = new Vector3(scale, scale, 1);
        StartCoroutine(Despawn());
    }

    // This method handles the despawning of the holy water object after a delay
    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(3f);
        transform.localScale = new Vector3(projectileInitialScale, projectileInitialScale, 1);
        // update polygon collider
        polygonCollider.TryUpdateShapeToAttachedSprite();
        isInSplashState = false;
        transform.localScale = new Vector3(projectileScale, projectileScale, 1);
        if (TryGetComponent<PoolAble>(out var poolAble))
        {
            poolAble.ReleaseObject();
        }
    }


}
