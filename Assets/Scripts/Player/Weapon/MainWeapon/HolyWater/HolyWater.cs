using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class HolyWater : Weapon.MainWeapon
{
    [SerializeField] private float nearestMonsterTargetingDegreeThreshold = 45f;
    private readonly float attackIntervalInSeconds = 0.5f;
    private readonly float holdTime = 0.25f;

    protected override void Awake()
    {
        weaponType = WeaponType.HolyWater;
        weaponAttackDirectionType = Weapon.WeaponAttackDirectionType.Aim;
        weaponPositionXOffset = 0.2f;
    }

    public override void InitStat()
    {
        base.InitStat();
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        GameObject holyWaterObject = ObjectPoolManager.instance.GetGo("HolyWater");
        holyWaterObject.transform.parent = transform;
        weaponSpriteRenderer = holyWaterObject.GetComponent<SpriteRenderer>();
        attackObject = holyWaterObject;
        HolyWaterObject holyWaterComponent = holyWaterObject.GetComponent<HolyWaterObject>();
        Animator holyWaterAnimator = holyWaterObject.GetComponent<Animator>();
        holyWaterAnimator.SetFloat("AttackSpeed", attackSpeed);
        holyWaterComponent.Init(attackDamage, attackRange, (int)(attackIntervalInSeconds / Time.fixedDeltaTime), attackTarget);
        attackDirection = attackDirection.normalized;
        Vector2 nearestMonsterDirection = CalculateVectorDistanceBetweenPlayerAndNearestMonster();

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
            Flip(nearestMosterAngle);
            yield return new WaitForSeconds(holdTime / attackSpeed);
            holyWaterObject.transform.parent = null;
            holyWaterObject.transform.DOMove(endValue, newSpeed).SetEase(Ease.Linear).OnComplete(holyWaterComponent.ChangeSprite);
        }
        else
        {
            Debug.Log($"[{Time.time}] Shoot to attackDirection - angleDifference: {angleDifference}");
            Vector2 endValue = (Vector2)transform.position + attackDirection * projectileSpeed;
            Flip(attackAngle);
            yield return new WaitForSeconds(holdTime / attackSpeed);
            holyWaterObject.transform.parent = null;
            holyWaterObject.transform.DOMove(endValue, attackSpeed).SetEase(Ease.Linear).OnComplete(holyWaterComponent.ChangeSprite);
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

    protected override void Flip(float degree)
    {
        base.Flip(degree);
    }

}
