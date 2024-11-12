using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Shotgun : Weapon
{
    private ObjectPoolManager objectPoolManager;
    [SerializeField] private float spreadDegree = 45f;

    private void Awake()
    {
        objectPoolManager = ObjectPoolManager.instance;
        weaponType = WeaponType.Shotgun;
        weaponAttackRange = WeaponAttackRange.Medium;
        weaponAttackTarget = WeaponAttackTarget.Multiple;
        weaponAttackDirectionType = WeaponAttackDirectionType.Aim;
    }

    public override void InitStat()
    {
        base.InitStat();
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        float spreadDegree = this.spreadDegree / projectileCount; // 퍼짐 정도를 조절
        for (int i = 0; i < projectileCount; i++)
        {
            GameObject bullet = objectPoolManager.GetGo("ShotgunBullet");
            float baseAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            float spreadAngle = (i - (projectileCount - 1) / 2f) * spreadDegree;
            Quaternion rotation = Quaternion.Euler(0, 0, baseAngle + spreadAngle);
            bullet.transform.SetPositionAndRotation(transform.position, rotation);
            Projectile bulletComponent = bullet.GetComponent<Projectile>();
            bulletComponent.speed = projectileSpeed;
            bulletComponent.attackRange = attackRange;
            bulletComponent.initialPosition = transform.position;
            bulletComponent.attackDamage = attackDamage;
        }

        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
        yield return null;

    }
}
