using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FamillyPicture : Weapon.SubWeapon
{
    // Barrier health = attackDamage

    protected override void Awake()
    {
        weaponType = WeaponType.FamillyPicture;
        base.Awake();
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        weaponScript.subWeaponSO = weaponData;
        weaponScript.Init(attackDamage, attackRange, 0, attackTarget);
        // Cooldown
        yield return new WaitForSeconds(attackSpeed);
        isAttackCooldown = false;
    }
}
