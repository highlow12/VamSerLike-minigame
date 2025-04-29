using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using DG.Tweening;

public class FishingRodObject : AttackObject
{
    public GameObject hookObject;
    private GameObject nearestMonster;
    private const float HOOK_HEIGHT_OFFSET = 1.0f; // 몬스터 머리 위 높이 오프셋

    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks, int attackTarget)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
    }

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        weaponPositionXOffset = 0.3f;
    }

    public override void SubWeaponInit()
    {
        base.SubWeaponInit();
    }

    void OnEnable()
    {
        base.Start();
    }

    private void FixedUpdate()
    {

    }

    public override void Attack()
    {
        hookObject.SetActive(true);
        hookObject.transform.parent = null;

        // find nearest monster
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        monsters = monsters.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).ToArray();
        nearestMonster = monsters.Length > 0 ? monsters[0] : null;

        if (nearestMonster != null)
        {
            float degree = Mathf.Atan2(nearestMonster.transform.position.y - transform.position.y,
                                      nearestMonster.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
            Flip(degree);

            // 몬스터의 머리 위치 계산
            Vector3 hookPosition = GetMonsterHeadPosition(nearestMonster);

            hookObject.transform.position = hookPosition;
            StartCoroutine(Hook());
        }
        else
        {
            // 몬스터가 없을 경우 훅 비활성화
            hookObject.transform.parent = transform;
            hookObject.transform.localPosition = Vector3.zero;
            hookObject.SetActive(false);
        }
    }

    // 몬스터의 머리 위치를 계산하는 함수
    private Vector3 GetMonsterHeadPosition(GameObject monster)
    {
        Vector3 monsterPosition = monster.transform.position;

        SpriteRenderer renderer = monster.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // 스프라이트의 상단 중앙 위치
            Vector3 topCenter = renderer.bounds.center;
            topCenter.y = renderer.bounds.max.y;
            return topCenter;
        }

        return monsterPosition;

    }
    // 낚시찌의 하단 위치를 계산하는 함수
    private Vector3 GetHookPosition(Vector3 hookDestination)
    {
        SpriteRenderer renderer = hookObject.GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            // 스프라이트의 하단 중앙 위치
            Vector3 bottomCenter = renderer.bounds.center;
            bottomCenter.y = renderer.bounds.min.y;
            float dy = hookObject.transform.position.y - bottomCenter.y;
            return hookDestination - new Vector3(0, dy + HOOK_HEIGHT_OFFSET / 2, 0);
        }
        return hookObject.transform.position;
    }

    private IEnumerator Hook()
    {
        animator.SetTrigger("Attack");

        if (nearestMonster != null)
        {
            Vector3 prevPosition = nearestMonster.transform.position;
            Vector3 monsterHeadPosition = GetMonsterHeadPosition(nearestMonster);
            Vector3 hookDestination = monsterHeadPosition + new Vector3(0, HOOK_HEIGHT_OFFSET, 0);
            Vector3 hookPosition = GetHookPosition(hookDestination);
            hookObject.transform.position = monsterHeadPosition;
            hookObject.transform.DOMove(hookDestination, tweeningDuration);
            // 몬스터를 훅 위치로 이동
            nearestMonster.transform.DOMove(hookPosition, tweeningDuration).OnComplete(() =>
            {
                if (nearestMonster != null) // 몬스터가 아직 존재하는지 확인
                {
                    Monster monster = nearestMonster.GetComponent<Monster>();
                    if (monster != null)
                    {
                        monster.TakeDamage(attackDamage);
                    }
                }

                // 훅 원위치
                hookObject.transform.parent = transform;
                hookObject.transform.localPosition = Vector3.zero;
                hookObject.SetActive(false);

                if (nearestMonster != null)
                {
                    nearestMonster.transform.DOMove(prevPosition, tweeningDuration);
                }
            });
        }

        yield return null;
    }

}
