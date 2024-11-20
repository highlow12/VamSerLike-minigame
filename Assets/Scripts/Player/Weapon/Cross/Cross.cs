using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Cross : Weapon
{
    private Animator attackObjectAnimator;

    private void Awake()
    {
        attackObject = Resources.Load<GameObject>("Prefabs/Player/AttackObject/Cross");
        attackObject = Instantiate(attackObject, transform);
        attackObject.SetActive(false);
        weaponType = WeaponType.Cross;
        weaponAttackDirectionType = WeaponAttackDirectionType.Nearest;
    }

    public override void InitStat()
    {
        base.InitStat();
        attackObject.transform.localScale = new Vector3(attackRange, attackRange, 1);
        attackObjectAnimator = attackObject.GetComponent<Animator>();
        attackObject.GetComponent<CrossObject>().attackDamage = attackDamage;
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        attackObject.SetActive(true);
        attackObjectAnimator.SetFloat("FadeMultiplier", attackSpeed);
        isAttackCooldown = true;
        attackObject.transform.position = transform.position + ((Vector3)attackDirection.normalized * attackForwardDistance);
        attackObject.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg);
        yield return new WaitForSeconds(1f / attackSpeed);
        attackObject.SetActive(false);
        isAttackCooldown = false;
        yield return null;
    }


}


