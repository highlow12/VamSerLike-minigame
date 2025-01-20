using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PaperPlane : Weapon.SubWeapon
{
    public List<GameObject> attackObjects = new();

    protected override void Awake()
    {
        weaponType = WeaponType.PaperPlane;
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
        weaponScript.subWeaponSO = weaponData;
        weaponScript.Init(attackDamage, attackRange, 0, attackTarget);
        weaponScript.SubWeaponInit();
        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
    }
}
