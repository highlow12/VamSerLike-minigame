using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class Cross : Weapon
{
    [SerializeField] private float callibrationMultiplier = 0.4f;

    private void Awake()
    {
        attackObject = Resources.Load<GameObject>("Prefabs/Player/Weapon/Cross");
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
        attackObject.GetComponent<CrossObject>().Init(
            attackDamage,
            attackRange
        );
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        attackObject.SetActive(true);
        attackObjectAnimator.SetFloat("AttackSpeed", attackSpeed);
        isAttackCooldown = true;
        Vector3 normalizedDirection = attackDirection.normalized;
        float distanceMultiplier = (1 + (callibrationMultiplier / attackRange)) * Mathf.Sqrt(attackRange);
        attackObject.transform.position = transform.position + (distanceMultiplier * attackForwardDistance * normalizedDirection);
        attackObject.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg - 90);
        yield return new WaitForSeconds(1f / attackSpeed);
        attackObject.SetActive(false);
        isAttackCooldown = false;
        yield return null;
    }


}


