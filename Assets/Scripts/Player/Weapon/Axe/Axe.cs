using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Axe : Weapon
{
    private void Awake()
    {
        attackObject = Resources.Load<GameObject>("Prefabs/Player/Weapon/Axe");
        attackObject = Instantiate(attackObject, transform);
        weaponPositionXOffset = 0.3f;
        weaponSpriteRenderer = attackObject.GetComponent<SpriteRenderer>();
        attackObject.transform.localPosition = new Vector3(weaponPositionXOffset, 0, 0);
        weaponScript = attackObject.GetComponent<AttackObject>();
        weaponType = WeaponType.Axe;
        weaponAttackDirectionType = WeaponAttackDirectionType.Nearest;
    }

    public override void InitStat()
    {
        base.InitStat();
        weaponScript.colliderObject.transform.localScale = new Vector3(attackRange, attackRange, 1);
        attackObjectAnimator = attackObject.GetComponent<Animator>();
        attackObject.GetComponent<AxeObject>().Init(
            attackDamage,
            attackRange,
            0,
            attackTarget
        );

    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        attackObjectAnimator.SetFloat("AttackSpeed", attackSpeed);
        isAttackCooldown = true;
        Vector2 pos = (Vector2)transform.position + attackDirection.normalized * attackForwardDistance;
        float degree = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        weaponScript.colliderObject.transform.position = pos;
        weaponScript.colliderObject.transform.rotation = Quaternion.Euler(0, 0, degree);
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
