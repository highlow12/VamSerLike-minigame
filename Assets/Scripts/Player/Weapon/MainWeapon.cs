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
            HolyWater
        }
        [Serializable]
        public struct MonsterHit
        {
            public Monster monster;
            public long hitTime;
        }

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
        protected float attackDamage;
        protected float attackSpeed;
        protected float attackRange;
        protected float attackForwardDistance;
        protected int attackTarget;
        protected int projectileCount;
        protected float projectileSpeed;
        public bool isAttackCooldown;

        protected virtual void Awake()
        {
            attackObject = Resources.Load<GameObject>("Prefabs/Player/Weapon/MainWeapon/" + weaponType.ToString());
            attackObject = Instantiate(attackObject, transform);
            weaponSpriteRenderer = attackObject.GetComponent<SpriteRenderer>();
            weaponScript = attackObject.GetComponent<AttackObject>();
            attackObject.transform.localPosition = new Vector3(weaponPositionXOffset, 0, 0);
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
            attackDamage = weaponStat.attackDamage;
            attackSpeed = weaponStat.attackSpeed;
            attackRange = weaponStat.attackRange;
            attackForwardDistance = Mathf.Sqrt(attackRange);
            attackTarget = weaponStat.attackTarget;
            projectileCount = weaponStat.projectileCount;
            projectileSpeed = weaponStat.projectileSpeed;
            if (attackObject != null)
            {
                weaponScript.colliderObject.transform.localScale = new Vector3(attackRange, attackRange, 1);
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