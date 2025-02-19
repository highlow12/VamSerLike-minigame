using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashMonster : BossMonster
{
    GameObject Attackrange;
    public Animator animator{get; private set;}
    public float moveSpeed;
    public float jumpHeight = 10;
    [SerializeField] float AttackTimeMax = 10;
    [SerializeField] float AttackTimeMin = 5;

    float AttackTime;
    float timer = 0;

    enum TrashMonsterAction
    {
        Attack = 0,
        Move = 1
    }
    public void JumpEnd()
    {
        monsterFSM.ChangeState(states[(int)TrashMonsterAction.Move]);
    }
    public void TurnOnAttackRange()
    {
        Attackrange.SetActive(true);
    }
    public void TurnOffAttackRange()
    {
        Attackrange.SetActive(false);
    }
    protected override void checkeState()
    {
        if(monsterFSM.currentState is TrashMonsterMove)timer += Time.deltaTime;

        if (timer > AttackTime && monsterFSM.currentState is TrashMonsterMove)
        {
            monsterFSM.ChangeState(states[(int)TrashMonsterAction.Attack]);
            timer = 0;
            AttackTime = Random.Range(AttackTimeMin, AttackTimeMax);
        }
    }

    protected override void initState()
    {
        animator = GetComponent<Animator>();
        Attackrange = transform.GetChild(0).gameObject;
        Attackrange.SetActive(false);
        AttackTime = Random.Range(AttackTimeMin, AttackTimeMax);
        states = new Dictionary<int, BaseState>
        {
            { (int)TrashMonsterAction.Attack, new TrashMonsterAttack(this) },
            { (int)TrashMonsterAction.Move, new TrashMonsterMove(this) }
        };
        monsterFSM = new MonsterFSM(states[(int)TrashMonsterAction.Move]);
    }
}

class TrashMonsterAttack : BaseState
{
    float timer = 0;
    Vector2 CurrentPosition;
    Vector2 TargetPosition;
    TrashMonster trashMonster;
    public TrashMonsterAttack(Monster m) : base(m)
    {
        trashMonster = m as TrashMonster;
    }

    public override void OnStateEnter()
    {
        trashMonster.animator.SetTrigger("Jump");
        timer = 0;
        CurrentPosition = monster.transform.position;
        TargetPosition = GameManager.Instance.player.transform.position;
    }

    public override void OnStateExit()
    {
        monster.transform.position = new Vector3(monster.transform.position.x,TargetPosition.y, 0);
        trashMonster.TurnOffAttackRange();
    }

    public override void OnStateUpdate()
    {
        timer += Time.deltaTime;
        var height = func(timer/2.5f) * trashMonster.jumpHeight;
        var pos = Vector2.Lerp(CurrentPosition, TargetPosition, timer/2.5f);
        Debug.Log(pos);
        monster.transform.position = new Vector3(pos.x, pos.y + height, 0);
    }

    protected float func(float x)
    {
        return -1 * Mathf.Pow(2*x - 1, 2) + 1;
    }
}

class TrashMonsterMove : BaseState
{
    TrashMonster trashMonster;
    public TrashMonsterMove(Monster m) : base(m)
    {
        trashMonster = m as TrashMonster;
    }

    public override void OnStateEnter()
    {
        trashMonster.animator.SetTrigger("Move");
    }

    public override void OnStateExit()
    {
    }

    public override void OnStateUpdate()
    {
        //Move to player
        Vector2 targetPosition = GameManager.Instance.player.transform.position;
        monster.transform.position += ((TrashMonster)monster).moveSpeed * Time.deltaTime * ((Vector3)(targetPosition - (Vector2)monster.transform.position)).normalized;
    }


}
