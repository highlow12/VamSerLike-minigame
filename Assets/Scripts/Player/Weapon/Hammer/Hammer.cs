using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Hammer : Weapon
{
    // 테스트용 변수
    Vector2 ad;

    private void Awake()
    {
        // attackObject = Resources.Load<GameObject>("Prefabs/Player/Weapon/Hammer");
        weaponType = WeaponType.Hammer;
        weaponAttackDirectionType = WeaponAttackDirectionType.Aim;
    }

    public override void InitStat()
    {
        base.InitStat();
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        ad = attackDirection;
        isAttackCooldown = true;
        Vector2 newOrigin = (Vector2)transform.position + attackDirection.normalized * attackForwardDistance;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            newOrigin,
            new Vector2(attackRange, attackRange),
            0f,
            Vector2.zero,
            0f,
            1 << LayerMask.NameToLayer("Monster")
        );
        hits = hits.OrderBy(x => Vector2.Distance(x.transform.position, transform.position)).ToArray();
        if (hits.Length > 0)
        {
            for (int i = 0; i < attackTarget; i++)
            {
                if (i >= hits.Length)
                {
                    break;
                }
                Monster monster = hits[i].collider.GetComponent<Monster>();
                monster.TakeDamage(attackDamage);
            }
        }
        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
        yield return null;
    }

    void OnDrawGizmos()
    {
        Vector2 newOrigin = (Vector2)transform.position + ad.normalized * attackRange;
        Gizmos.color = Color.red;
        Gizmos.DrawCube(
            newOrigin,
            new Vector3(attackRange, attackRange, 0)
        );
    }
}
