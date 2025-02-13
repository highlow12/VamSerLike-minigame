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
        InitPool("HolyWater");
    }


    public override void InitStat()
    {
        base.InitStat();
    }

    public override IEnumerator Attack(Vector2 attackDirection)
    {
        isAttackCooldown = true;
        GameObject holyWaterObject = ObjectPoolManager.Instance.GetGo("HolyWater");
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

        Vector2 baseEndPos = (Vector2)transform.position + attackDirection * projectileSpeed;
        Vector2 endPos = baseEndPos;
        float baseEndPosMagnitude = baseEndPos.magnitude;
        float positionVectorMagnitude = transform.position.magnitude;

        Vector3 startPos = holyWaterObject.transform.position;
        bool isDestinationUpperThanPlayer = baseEndPos.y > transform.position.y;

        float distance = 0;
        float duration = 0;
        Vector3 midPoint = Vector3.zero;

        if (angleDifference > 180)
        {
            angleDifference = 360 - angleDifference;
        }

        if (angleDifference < nearestMonsterTargetingDegreeThreshold && nearestMonsterDirection != Vector2.zero)
        {
            endPos = (Vector2)transform.position + nearestMonsterDirection;
            // 조준 보정 동작 조건을 만족하였지만, 몬스터가 플레이어와 너무 멀리 떨어져 있어 공격이 불가능한 경우 조준 보정을 취소
            if (endPos.magnitude > baseEndPosMagnitude)
            {
                endPos = baseEndPos;
            }
            isDestinationUpperThanPlayer = endPos.y > transform.position.y;
            Flip(nearestMosterAngle);
        }
        else
        {
            Flip(attackAngle);
            distance = Vector2.Distance(startPos, endPos);
        }

        yield return new WaitForSeconds(holdTime / attackSpeed);
        holyWaterObject.transform.parent = null;
        startPos = holyWaterObject.transform.position;
        distance = Vector2.Distance(startPos, endPos);
        duration = distance / (projectileSpeed * 0.5f);
        midPoint = (startPos + (Vector3)endPos) * 0.5f;
        if (!isDestinationUpperThanPlayer)
        {
            midPoint += Vector3.down * (distance / 3f);
        }
        else
        {
            midPoint += Vector3.up * (distance / 3f);
        }
        if (!weaponSpriteRenderer.flipX)
        {
            midPoint += Vector3.left * (distance / 5f);
        }
        else
        {
            midPoint += Vector3.right * (distance / 5f);
        }

        DebugConsole.Line line = new()
        {
            text = $"[HolyWater] StartPos: {startPos}, MidPoint: {midPoint}, EndPos: {endPos}",
            messageType = DebugConsole.MessageType.Local,
            tick = GameManager.Instance.gameTimer
        };
        DebugConsole.Instance.MergeLine(line);

        holyWaterObject.transform.DOPath(new Vector3[] { startPos, midPoint, endPos },
             duration,
                 PathType.CatmullRom)
                    .SetEase(Ease.InOutQuad)
                        .SetSpeedBased(true)
                            .OnComplete(holyWaterComponent.ChangeSprite);

        yield return new WaitForSeconds(1f / attackSpeed);
        isAttackCooldown = false;
    }

    private Vector2 CalculateVectorDistanceBetweenPlayerAndNearestMonster()
    {
        GameObject[] mosnters = GameObject.FindGameObjectsWithTag("Monster");
        GameObject nearestMonster = mosnters.OrderBy(x => Math.Abs(Vector2.Distance(x.transform.position, transform.position))).FirstOrDefault();
        if (Equals(nearestMonster, default))
        {
            return Vector2.zero;
        }
        return nearestMonster.transform.position - transform.position;
    }

    protected override void Flip(float degree)
    {
        base.Flip(degree);
    }

}
