using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Cross : Weapon.MainWeapon
{
    [SerializeField] private float callibrationMultiplier = 0.4f;

    protected override void Awake()
    {
        weaponType = WeaponType.Cross;
        weaponAttackDirectionType = Weapon.WeaponAttackDirectionType.Nearest;
        weaponPositionXOffset = 0.2f;
        base.Awake();
    }

    public override void InitStat()
    {
        base.InitStat();
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        weaponScript.Init(attackDamage, attackRange, 0, attackTarget);
        attackObjectAnimator.SetFloat("AttackSpeed", attackSpeed);
        Vector3 normalizedDirection = attackDirection.normalized;
        float distanceMultiplier = (1 + (callibrationMultiplier / attackRange)) * Mathf.Sqrt(attackRange);
        float degree = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        weaponScript.colliderObject.transform.position = transform.position + (distanceMultiplier * attackForwardDistance * normalizedDirection);
        weaponScript.colliderObject.transform.rotation = Quaternion.Euler(0, 0, degree - 90);
        Flip(degree);
        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
        yield return null;
    }

    protected override void Flip(float degree)
    {
        base.Flip(degree);
    }

}


