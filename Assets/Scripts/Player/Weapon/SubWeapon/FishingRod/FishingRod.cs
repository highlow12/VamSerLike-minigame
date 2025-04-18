using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishingRod : Weapon.SubWeapon
{
    public List<GameObject> attackObjects = new();
    protected override void Awake()
    {
        weaponType = WeaponType.FishingRod;
        base.Awake();
        attackObject.transform.parent = transform;
        attackObject.transform.localPosition = new Vector3(0, 0, 0);
    }

    protected override void InitStat()
    {
        base.InitStat();
        weaponScript.subWeaponSO = weaponData;
        weaponScript.SubWeaponInit();
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        weaponScript.Init(attackDamage, attackRange, 0, attackTarget);
        weaponScript.tweeningDuration = 1f / (attackSpeed * 5);
        weaponScript.Attack();
        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
    }
}
