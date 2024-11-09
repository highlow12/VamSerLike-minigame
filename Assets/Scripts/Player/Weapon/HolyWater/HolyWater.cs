using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class HolyWater : Weapon
{
    [SerializeField] private float nearestMonsterTargetingDegreeThreshold = 45f;

    private void Awake()
    {
        weaponType = WeaponType.HolyWater;
        weaponAttackRange = WeaponAttackRange.Medium;
        weaponAttackTarget = WeaponAttackTarget.Multiple;
        weaponAttackDirectionType = WeaponAttackDirectionType.Aim;
    }

    public override void InitStat(WeaponRare weaponRare)
    {
        base.InitStat(weaponRare);
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        GameObject holyWaterObject = ObjectPoolManager.instance.GetGo("HolyWater");
        HolyWaterObject holyWaterComponent = holyWaterObject.GetComponent<HolyWaterObject>();
        holyWaterObject.transform.position = transform.position;
        holyWaterComponent.attackRange = attackRange;
        holyWaterComponent.attackDamage = attackDamage;

        attackDirection = attackDirection.normalized;
        Vector2 nearestMonsterDirection = CalculateVectorDistanceBetweenPlayerAndNearestMonster().normalized;

        float nearestMosterAngle = Mathf.Atan2(nearestMonsterDirection.y, nearestMonsterDirection.x) * Mathf.Rad2Deg;
        float attackAngle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        float angleDifference = Mathf.Abs(nearestMosterAngle - attackAngle);

        if (angleDifference > 180)
        {
            angleDifference = 360 - angleDifference;
        }

        if (angleDifference < nearestMonsterTargetingDegreeThreshold)
        {
            Debug.Log($"[{Time.time}] Shoot to nearestMonsterDirection - angleDifference: {angleDifference}");
            Vector2 endValue = (Vector2)transform.position + nearestMonsterDirection;
            float newSpeed = attackSpeed * (nearestMonsterDirection.magnitude / (attackDirection * projectileSpeed).magnitude);
            holyWaterObject.transform.DOMove(endValue, newSpeed).SetEase(Ease.Linear).OnComplete(holyWaterComponent.DisableProjectileSprite);
        }
        else
        {
            Debug.Log($"[{Time.time}] Shoot to attackDirection - angleDifference: {angleDifference}");
            Vector2 endValue = (Vector2)transform.position + attackDirection * projectileSpeed;
            holyWaterObject.transform.DOMove(endValue, attackSpeed).SetEase(Ease.Linear).OnComplete(holyWaterComponent.DisableProjectileSprite);
        }

        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
    }

    private Vector2 CalculateVectorDistanceBetweenPlayerAndNearestMonster()
    {
        GameObject[] mosnters = GameObject.FindGameObjectsWithTag("Monster");
        GameObject nearestMonster = mosnters.OrderBy(x => Math.Abs(Vector2.Distance(x.transform.position, transform.position))).FirstOrDefault();
        return nearestMonster.transform.position - transform.position;
    }

}
