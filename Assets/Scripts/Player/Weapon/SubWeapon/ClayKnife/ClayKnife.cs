using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class ClayKnife : Weapon.SubWeapon
{
    float spreadDegree = 45f;
    protected override void Awake()
    {
        weaponType = WeaponType.ClayKnife;
        InitPool("ClayKnife");
    }
    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        float baseAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        float spreadDegree = this.spreadDegree / projectileCount; // 퍼짐 정도를 조절
        for (int i = 0; i < projectileCount; i++)
        {
            attackObject = ObjectPoolManager.Instance.GetGo("ClayKnife");
            float spreadAngle = baseAngle + (i - (projectileCount - 1) / 2) * spreadDegree;
            Quaternion rotation = Quaternion.Euler(0, 0, spreadAngle + 90);
            Vector3 rotationVector = new(Mathf.Cos(spreadAngle * Mathf.Deg2Rad), Mathf.Sin(spreadAngle * Mathf.Deg2Rad), 0);
            attackObject.transform.SetPositionAndRotation(transform.position + (Vector3)attackDirection.normalized, rotation);
            Vector3 startPos = attackObject.transform.position;
            Vector3 endPos = startPos + rotationVector * projectileSpeed;
            float duration = Vector3.Distance(startPos, endPos) / (projectileSpeed * 1.5f);
            weaponScript = attackObject.GetComponent<AttackObject>();
            weaponScript.subWeaponSO = weaponData;
            weaponScript.Init(attackDamage, attackRange, 0, attackTarget);
            weaponScript.SubWeaponInit();
            weaponScript.Throw(endPos, duration);
        }

        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
    }
}
