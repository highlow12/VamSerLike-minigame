using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Axe : Weapon
{
    // 테스트용 변수
    Vector2 ad;

    private void Awake()
    {
        weaponType = WeaponType.Axe;
        weaponAttackRange = WeaponAttackRange.Close;
        weaponAttackTarget = WeaponAttackTarget.Single;
        weaponAttackDirectionType = WeaponAttackDirectionType.Aim;
    }

    public override void InitStat(WeaponRare weaponRare)
    {
        base.InitStat(weaponRare);
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        ad = attackDirection;
        isAttackCooldown = true;
        Vector2 newOrigin = (Vector2)transform.position + attackDirection.normalized * attackRange;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            newOrigin,
            new Vector2(attackRange, attackRange),
            0f, // 회전 각도
            Vector2.zero, // 방향
            0f,
            1 << LayerMask.NameToLayer("Monster")
        );
        foreach (RaycastHit2D hit in hits)
        {
            Monster monster = hit.collider.GetComponent<Monster>();
            monster.TakeDamage(attackDamage);
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
