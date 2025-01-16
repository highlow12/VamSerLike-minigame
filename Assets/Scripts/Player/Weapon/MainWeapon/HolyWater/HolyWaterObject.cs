using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class HolyWaterObject : Projectile
{
    [SerializeField] private float projectileInitialScale;
    private PolygonCollider2D polygonCollider;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private bool isInSplashState = false;
    private Animator animator;

    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks = 0, int attackTarget = 0)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
        this.attackRange = attackRange;
    }

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (isInSplashState)
        {
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
                Debug.Log($"[{GameManager.Instance.gameTimer}] HolyWaterObject hit {monster.name}");
                Weapon.MainWeapon.MonsterHit appendedMonsterHit = monstersHit.FirstOrDefault(x => x.monster == monster);
                if (default(Weapon.MainWeapon.MonsterHit).Equals(appendedMonsterHit))
                {
                    Weapon.MainWeapon.MonsterHit monsterHit = new()
                    {
                        monster = monster,
                        hitTime = GameManager.Instance.gameTimer
                    };
                    monstersHit.Add(monsterHit);
                }
                int index = monstersHit.FindIndex(x => x.monster == monster);
                if (index != -1)
                {
                    monstersHit[index] = new Weapon.MainWeapon.MonsterHit
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
            monstersHit.Clear();
        }
    }

    public void ChangeSprite()
    {
        isInSplashState = true;
        animator.SetBool("Splash", true);
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
        animator.SetBool("Splash", false);
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
