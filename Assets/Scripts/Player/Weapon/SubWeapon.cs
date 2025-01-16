using System.Collections;
using UnityEngine;

namespace Weapon
{
    public abstract class SubWeapon : MonoBehaviour
    {
        public enum WeaponType
        {
            PaperPlane
        }
        public WeaponType weaponType;
        public WeaponAttackDirectionType weaponAttackDirectionType;
        public GameObject attackObject;
        protected float attackSpeed;
        protected float attackDamage;
        protected float attackRange;
        protected float attackIntervalInSeconds;
        protected int attackTarget;
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
                // InitStat();
            }
        }
        protected AttackObject weaponScript;
        public bool isAttackCooldown = false;
        protected virtual void Awake()
        {
            attackObject = Resources.Load<GameObject>("Prefabs/Player/Weapon/" + weaponType.ToString());
            attackObject = Instantiate(attackObject, transform);
            weaponScript = attackObject.GetComponent<AttackObject>();
        }

        public virtual IEnumerator Attack(Vector2 attackDirection)
        {
            yield return null;
        }
    }
}
