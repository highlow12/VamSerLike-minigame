using System.Collections;
using UnityEngine;

namespace Weapon
{
    public abstract class SubWeapon : MonoBehaviour
    {
        public SubWeaponSO weaponData;
        protected WeaponStatProvider.SubWeaponStat weaponStat;
        public enum WeaponType
        {
            PaperPlane,
            TeddyBear,
            FamillyPicture,
            ClayKnife,
        }
        protected bool isUsingPool = false;
        protected string prefabName;
        public WeaponType weaponType;
        public string displayName;
        public string displayWeaponAttackDirectionType;
        public WeaponAttackDirectionType weaponAttackDirectionType;
        public GameObject attackObject;
        public float baseAttackDamage;
        public float baseAttackRange;
        public float baseAttackSpeed;
        public int baseAttackTarget;
        public int baseProjectileCount;
        public float baseProjectileSpeed;
        public float attackDamage;
        public float attackRange;
        public float attackSpeed;
        protected float attackIntervalInSeconds;
        public int attackTarget;
        public int projectileCount;
        public float projectileSpeed;
        public int maxWeaponGrade;
        private int _weaponGrade = 0;
        public int weaponGrade
        {
            get
            {
                return _weaponGrade;
            }
            set
            {
                _weaponGrade = value;
                // Set weaponData
                string soPath = $"ScriptableObjects/SubWeapon/{weaponType}/{weaponType}_{weaponGrade}";
                weaponData = Resources.Load<SubWeaponSO>(soPath);

                if (weaponData == null)
                {
                    Debug.LogError($"Failed to load {soPath}");
#if UNITY_EDITOR
                    DebugConsole.Line errorLine = new()
                    {
                        text = $"Failed to load {soPath}",
                        messageType = DebugConsole.MessageType.Local,
                        tick = GameManager.Instance.gameTimer
                    };
                    DebugConsole.Instance.MergeLine(errorLine, "#FF0000");
#endif
                }

                InitStat();
            }
        }
        protected AttackObject weaponScript;
        public bool isAttackCooldown = false;
        protected virtual void Awake()
        {
            attackObject = Resources.Load<GameObject>("Prefabs/Player/Weapon/SubWeapon/" + weaponType.ToString());
            attackObject = Instantiate(attackObject, transform);
            weaponScript = attackObject.GetComponent<AttackObject>();
        }
        protected void InitPool(string name, int poolSize = 10)
        {
            string path = $"Prefabs/Player/Weapon/SubWeapon/{name}";
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

        protected void OnDestroy()
        {
            if (isUsingPool)
            {
                ObjectPoolManager.Instance.UnregisterObjectPool(prefabName);
            }
        }

        protected virtual void InitStat()
        {
            weaponStat = WeaponStatProvider.Instance.subWeaponStats.Find(x => x.weaponType == weaponType && x.weaponGrade == weaponGrade);
            displayName = weaponStat.displayName;
            displayWeaponAttackDirectionType = weaponStat.displayWeaponAttackDirectionType;
            weaponAttackDirectionType = weaponStat.weaponAttackDirectionType;
            baseAttackDamage = weaponStat.attackDamage;
            baseAttackRange = weaponStat.attackRange;
            baseAttackSpeed = weaponStat.attackSpeed;
            attackIntervalInSeconds = weaponStat.attackIntervalInSeconds;
            baseAttackTarget = weaponStat.attackTarget;
            baseProjectileCount = weaponStat.projectileCount;
            baseProjectileSpeed = weaponStat.projectileSpeed;
            maxWeaponGrade = weaponStat.maxWeaponGrade;
            if (attackObject != null)
            {
                weaponScript.colliderObject.transform.localScale = new Vector3(attackRange, attackRange, 1);
            }
        }


        public virtual IEnumerator Attack(Vector2 attackDirection)
        {
            yield return null;
        }
    }
}
