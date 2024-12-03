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
        attackObject.SetActive(false);
        weaponType = WeaponType.Axe;
        weaponAttackDirectionType = WeaponAttackDirectionType.Nearest;
    }

    public override void InitStat()
    {
        base.InitStat();
        attackObject.transform.localScale = new Vector3(attackRange, attackRange, 1);
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
        attackObject.SetActive(true);
        attackObjectAnimator.SetFloat("AttackSpeed", attackSpeed);
        isAttackCooldown = true;
        Vector2 pos = (Vector2)transform.position + attackDirection.normalized * attackForwardDistance;
        attackObject.transform.position = pos;
        attackObject.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg);
        yield return new WaitForSeconds(1f / attackSpeed);
        attackObject.SetActive(false);
        isAttackCooldown = false;
        yield return null;
    }

}
