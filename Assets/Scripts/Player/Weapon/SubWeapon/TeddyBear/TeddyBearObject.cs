using UnityEngine;
using System.Linq;
using System;
using System.Collections;
using DG.Tweening;

public class TeddyBearObject : AttackObject
{
    public GameObject targetMonster;
    public Vector3 targetPosition;
    public float targetDistance;
    private float maxTargetDistance = 5f;

    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks = 0, int attackTarget = 0)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
        colliderObject.transform.localScale = new Vector3(attackRange, attackRange, 1);
        this.attackRange = attackRange;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    void OnEnable()
    {
        base.Start();
        targetMonster = null;
        targetPosition = Vector3.zero;
        targetDistance = 0;
        transform.parent = GameManager.Instance.player.transform;
    }

    private void FixedUpdate()
    {
        if (targetMonster == null || !targetMonster.activeSelf)
        {
            // Set target monster to the nearest monster
            GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
            if (monsters.Length == 0)
            {
                return;
            }
            targetMonster = monsters.OrderBy(x => Math.Abs(Vector2.Distance(x.transform.position, transform.position))).FirstOrDefault();
            targetPosition = targetMonster.transform.position;
            targetDistance = Vector2.Distance(targetPosition, transform.position);
            if (targetDistance > maxTargetDistance)
            {
                targetMonster = null;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.05f);
            targetPosition = targetMonster.transform.position;
            targetDistance = Vector2.Distance(targetPosition, transform.position);
            if (targetDistance < 0.1f)
            {
                transform.parent = null;
                Attack();
            }
        }
    }

    public override void Attack()
    {
        base.Attack();
        if (hitCount > 0)
        {
        }
        Despawn();
    }

}
