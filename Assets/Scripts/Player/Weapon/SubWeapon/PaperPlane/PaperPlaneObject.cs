using UnityEngine;
using System.Linq;
using System;
using System.Collections;
using DG.Tweening;

public class PaperPlaneObject : AttackObject
{
    public GameObject targetMonster;
    public Vector3 targetPosition;
    public float targetDistance;
    private float maxTargetDistance = 5f;
    public bool waitingFlyBeforeRetarget = false;

    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks = 0, int attackTarget = 0)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
        this.attackRange = attackRange;
    }

    protected override void Awake()
    {
        base.Awake();
        colliderObject.transform.localScale = new Vector3(attackRange, attackRange, 1);
    }

    void OnEnable()
    {
        base.Start();
        targetMonster = null;
        targetPosition = Vector3.zero;
        targetDistance = 0;
        waitingFlyBeforeRetarget = false;
    }

    private void FixedUpdate()
    {
        if (targetMonster == null || !targetMonster.activeSelf)
        {
            // Set target monster to the nearest monster
            GameObject[] mosnters = GameObject.FindGameObjectsWithTag("Monster");
            targetMonster = mosnters.OrderBy(x => Math.Abs(Vector2.Distance(x.transform.position, transform.position))).FirstOrDefault();
            targetPosition = targetMonster.transform.position;
            targetDistance = Vector2.Distance(targetPosition, transform.position);
            if (targetDistance > maxTargetDistance)
            {
                targetMonster = null;
                if (!waitingFlyBeforeRetarget)
                {
                    FlyBeforeRetarget();
                }
            }
            else
            {
                transform.DOPause();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.05f);
            targetPosition = targetMonster.transform.position;
            targetDistance = Vector2.Distance(targetPosition, transform.position);
            if (targetDistance < 0.1f)
            {
                Attack();
                Despawn();
            }
        }
    }

    private void FlyBeforeRetarget()
    {
        waitingFlyBeforeRetarget = true;
        Vector3 center = GameManager.Instance.player.transform.position;
        float radius = 1f;
        float angle = Time.time * 5f * UnityEngine.Random.Range(0.5f, 1.5f);
        Vector3 orbit = new(
            center.x + radius * Mathf.Cos(angle),
            center.y + radius * Mathf.Sin(angle),
            center.z
        );
        transform.DOMove(orbit, 1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            waitingFlyBeforeRetarget = false;
        });
    }

    private void Despawn()
    {
        if (TryGetComponent<PoolAble>(out var poolAble))
        {
            poolAble.ReleaseObject();
        }
    }


}
