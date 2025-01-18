using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PaperPlane : Weapon.SubWeapon
{
    public List<GameObject> attackObjects = new();

    protected override void Awake()
    {
        weaponType = WeaponType.PaperPlane;
        // test value
        attackSpeed = 1f;
        attackDamage = 20f;
        attackRange = 1f;
        attackTarget = 10;
        weaponAttackDirectionType = Weapon.WeaponAttackDirectionType.Nearest;
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        for (int i = 0; i < attackObjects.Count; i++)
        {
            if (attackObjects[i] == null || !attackObjects[i].activeSelf)
            {
                attackObjects.RemoveAt(i);
            }
        }
        if (attackObjects.Count > 2) // 3개까지만 생성
        {
            isAttackCooldown = false;
            yield break;
        }
        attackObject = ObjectPoolManager.instance.GetGo("PaperPlane");
        attackObject.transform.position = transform.position + (Vector3)attackDirection.normalized;
        if (attackObjects.Find(x => x == attackObject) == null)
        {
            attackObjects.Add(attackObject);
        }
        weaponScript = attackObject.GetComponent<AttackObject>();
        weaponScript.Init(attackDamage, attackRange, 0, attackTarget);
        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
    }
}
