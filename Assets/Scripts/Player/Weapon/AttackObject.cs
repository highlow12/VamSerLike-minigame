using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;


public abstract class AttackObject : MonoBehaviour
{
    protected bool isUsingPool = false;
    protected string prefabName;
    public float attackDamage;
    public float attackRange;
    public int attackIntervalInTicks;
    public GameObject colliderObject;
    public SubWeaponSO subWeaponSO;
    protected int attackTarget;
    protected List<Weapon.MainWeapon.MonsterHit> monstersHit;
    protected List<Collider2D> hits;
    protected Collider2D attackCollider;
    protected LayerMask monsterLayer;
    protected ContactFilter2D contactFilter;
    protected int hitCount;
    // Sub weapon properties
    protected new Animation animation;
    protected SpriteRenderer spriteRenderer;

    public virtual void Setup(float baseAngle, float spreadDegree, int projectileCount, float projectileSpeed, float attackSpeed)
    {

    }

    public virtual void Init(float attackDamage, float attackRange, int attackIntervalInTicks, int attackTarget)
    {
        this.attackDamage = attackDamage;
        this.attackIntervalInTicks = attackIntervalInTicks;
        this.attackTarget = attackTarget;
        this.attackRange = attackRange;
    }

    public virtual void SubWeaponInit()
    {
        animation = GetComponent<Animation>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = subWeaponSO.weaponSprite;
        if (subWeaponSO.attackAnimation != null)
        {
            animation.AddClip(subWeaponSO.attackAnimation, "Attack");
        }
        if (subWeaponSO.hitAnimation != null)
        {
            animation.AddClip(subWeaponSO.hitAnimation, "Hit");
        }
    }

    protected virtual void Awake()
    {
        monstersHit = new List<Weapon.MainWeapon.MonsterHit>();
        hits = new List<Collider2D>();
        monsterLayer = 1 << LayerMask.NameToLayer("Monster");
        contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = true;
        contactFilter.SetLayerMask(monsterLayer);
        attackCollider = colliderObject.GetComponent<Collider2D>() ?? new Collider2D();

    }

    protected void InitPool(string name, int poolSize = 10)
    {
        string path = $"Prefabs/Player/Weapon/{name}";
        name = name.Split('/').Last();
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
#if UNITY_EDITOR
            DebugConsole.Line errorLog = new()
            {
                text = $"[{GameManager.Instance.gameTimer}] Failed to load drop item prefab {name}",
                messageType = DebugConsole.MessageType.Local,
                tick = GameManager.Instance.gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLog, "#FF0000");
            return;
#endif
        }
        else
        {
            ObjectPoolManager.Instance.RegisterObjectPool(name, prefab, null, poolSize);
            isUsingPool = true;
        }
    }

    protected virtual void OnDestroy()
    {
        if (isUsingPool)
        {
            ObjectPoolManager.Instance.UnregisterObjectPool(prefabName);
        }
    }

    protected virtual void Start()
    {
        monstersHit.Clear();
        hits.Clear();
    }

    public virtual void Attack()
    {
        Start();
        Physics2D.OverlapCollider(attackCollider, contactFilter, hits);
        hits = hits.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToList();
        hitCount = 0;
        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                break;
            }
            if (hitCount >= attackTarget)
            {
                break;
            }
            Monster monster = hit.GetComponent<Monster>();
            if (monster == null)
            {
                continue;
            }
            Debug.Log($"[{GameManager.Instance.gameTimer}] hit {monster.name}");
            monster.TakeDamage(attackDamage);
            hitCount++;
        }
    }

    public virtual void Throw(Vector3 endPos, float duration)
    {
        transform.DOMove(endPos, duration).SetEase(Ease.Linear).OnComplete(() => Despawn());
    }

    public virtual void Despawn()
    {
        if (TryGetComponent<PoolAble>(out var poolAble))
        {
            poolAble.ReleaseObject();
        }
    }

}
