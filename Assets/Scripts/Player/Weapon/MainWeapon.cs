using UnityEngine;
using System;
using System.Collections;

namespace Weapon
{
    public abstract class MainWeapon : MonoBehaviour
    {
        public enum WeaponType
        {
            Hammer,
            Axe,
            Cross,
            Machete,
            Shotgun,
            HolyWater,
            Glass
        }
        [Serializable]
        public struct MonsterHit
        {
            public Monster monster;
            public long hitTime;
        }
        protected bool isUsingPool = false;
        protected string prefabName;
        public GameObject attackObject;
        protected AttackObject weaponScript;
        public string displayName;
        public WeaponType weaponType;
        protected WeaponStatProvider.WeaponStat weaponStat;
        public string displayWeaponAttackRange;
        protected WeaponAttackRange weaponAttackRange;
        public string displayWeaponAttackTarget;
        protected WeaponAttackTarget weaponAttackTarget;
        public WeaponAttackDirectionType weaponAttackDirectionType;
        public string displayWeaponRare;
        private WeaponRare _weaponRare;
        public WeaponRare weaponRare
        {
            get
            {
                return _weaponRare;
            }
            set
            {
                _weaponRare = value;
                InitStat();
            }
        }
        protected Animator attackObjectAnimator;
        protected SpriteRenderer weaponSpriteRenderer;
        protected float weaponPositionXOffset;
        public float baseAttackDamage;
        public float baseAttackSpeed;
        public float baseAttackRange;
        public int baseAttackTarget;
        public int baseProjectileCount;
        public float baseProjectileSpeed;
        public float attackDamage;
        public float attackSpeed;
        private float _attackRange;
        public float attackRange
        {
            get
            {
                return _attackRange;
            }
            set
            {
                _attackRange = value;
                attackForwardDistance = Mathf.Sqrt(_attackRange);
                if (weaponScript != null && weaponScript.colliderObject != null)
                {
                    weaponScript.colliderObject.transform.localScale = new Vector3(_attackRange, _attackRange, 1);
                }
            }
        }
        protected float attackForwardDistance;
        public int attackTarget;
        public int projectileCount;
        public float projectileSpeed;
        public bool isAttackCooldown;

        protected virtual void Awake()
        {
            attackObject = Resources.Load<GameObject>("Prefabs/Player/Weapon/MainWeapon/" + weaponType.ToString());
            attackObject = Instantiate(attackObject, transform);
            weaponSpriteRenderer = attackObject.GetComponent<SpriteRenderer>();
            weaponScript = attackObject.GetComponent<AttackObject>();
            attackObject.transform.localPosition = new Vector3(weaponPositionXOffset, 0, 0);
        }

        protected void InitPool(string name, int poolSize = 10)
        {
            string path = $"Prefabs/Player/Weapon/MainWeapon/{name}";
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

        public virtual void InitStat()
        {
            weaponStat = WeaponStatProvider.Instance.weaponStats.Find(x => x.weaponType == weaponType && x.weaponRare == weaponRare);
            displayWeaponAttackRange = weaponStat.displayWeaponAttackRange;
            displayWeaponAttackTarget = weaponStat.displayWeaponAttackTarget;
            displayWeaponRare = weaponStat.displayWeaponRare;
            displayName = weaponStat.displayName;
            weaponAttackRange = weaponStat.weaponAttackRange;
            weaponAttackTarget = weaponStat.weaponAttackTarget;
            baseAttackDamage = weaponStat.attackDamage;
            baseAttackSpeed = weaponStat.attackSpeed;
            baseAttackRange = weaponStat.attackRange;
            attackForwardDistance = Mathf.Sqrt(baseAttackRange);
            baseAttackTarget = weaponStat.attackTarget;
            baseProjectileCount = weaponStat.projectileCount;
            baseProjectileSpeed = weaponStat.projectileSpeed;
            if (attackObject != null)
            {
                weaponScript.colliderObject.transform.localScale = new Vector3(baseAttackRange, baseAttackRange, 1);
                attackObjectAnimator = attackObject.GetComponent<Animator>();
            }
        }

        protected virtual void Flip(float degree)
        {
            if (degree > 90 || degree < -90)
            {
                weaponSpriteRenderer.flipX = false;
                attackObject.transform.localPosition = new Vector3(-weaponPositionXOffset, 0, 0);
            }
            else
            {
                weaponSpriteRenderer.flipX = true;
                attackObject.transform.localPosition = new Vector3(weaponPositionXOffset, 0, 0);
            }
        }

        public virtual IEnumerator Attack(Vector2 attackDirection)
        {
            Debug.Log("Attack");
            yield return null;
        }

    }
}