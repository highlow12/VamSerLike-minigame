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
        }
        public WeaponType weaponType;
        public string displayName;
        public string displayWeaponAttackDirectionType;
        public WeaponAttackDirectionType weaponAttackDirectionType;
        public GameObject attackObject;
        protected float attackDamage;
        protected float attackRange;
        protected float attackSpeed;
        protected float attackIntervalInSeconds;
        protected int attackTarget;
        protected int maxWeaponGrade;
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
                    DebugConsole.Line errorLine = new()
                    {
                        text = $"Failed to load {soPath}",
                        messageType = DebugConsole.MessageType.Local,
                        tick = GameManager.Instance.gameTimer
                    };
                    DebugConsole.Instance.MergeLine(errorLine, "#FF0000");
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
        protected virtual void InitStat()
        {
            weaponStat = WeaponStatProvider.Instance.subWeaponStats.Find(x => x.weaponType == weaponType && x.weaponGrade == weaponGrade);
            displayName = weaponStat.displayName;
            displayWeaponAttackDirectionType = weaponStat.displayWeaponAttackDirectionType;
            weaponAttackDirectionType = weaponStat.weaponAttackDirectionType;
            attackDamage = weaponStat.attackDamage;
            attackRange = weaponStat.attackRange;
            attackSpeed = weaponStat.attackSpeed;
            attackIntervalInSeconds = weaponStat.attackIntervalInSeconds;
            attackTarget = weaponStat.attackTarget;
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
