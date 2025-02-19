using UnityEngine;
using System.Collections.Generic;

public class Shark : BossMonster
{
    public float moveSpeed;
    public float ChargeSpeed;
    [SerializeField] float ChargeTime = 3f;
    float timer = 0;
    public Animator animator{get; private set;}
    enum SharkAction
    {
        Chase = 0,
        Attack = 1
    }
    protected override void checkeState()
    {
        timer += Time.deltaTime;
        // Check current state type and switch accordingly
        if (monsterFSM.currentState is SharkAttack)
        {
            if (timer > 1f)
            {
                monsterFSM.ChangeState(states[(int)SharkAction.Chase]);
                timer = 0;
            }
        }
        else // Assume current state is Chase
        {
            if (timer > ChargeTime)
            {
                monsterFSM.ChangeState(states[(int)SharkAction.Attack]);
                timer = 0;
            }
        }
    }

    protected override void initState()
    {
        animator = GetComponent<Animator>();

        states = new Dictionary<int, BaseState>
        {
            { (int)SharkAction.Chase, new SharkChase(this) },
            { (int)SharkAction.Attack, new SharkAttack(this) }
        };
        monsterFSM = new MonsterFSM(states[(int)SharkAction.Chase]);
    }
}

class SharkChase : BaseState
{
    // Change targetPosition to Vector2 for 2D movement on xy plane.
    Vector2 targetPosition;
    float radius = 5f;

    public SharkChase(Monster m) : base(m) { }

    public override void OnStateEnter()
    {
        // Use only the xy components of the player's position
        targetPosition = GetRandomPosition();
    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        if(Vector2.Distance(monster.transform.position, targetPosition) < 0.1f)
        {
            targetPosition = GetRandomPosition();
        }
        monster.transform.position += ((Vector3)(targetPosition - (Vector2)monster.transform.position)).normalized * ((Shark)monster).moveSpeed * Time.deltaTime;
    }

    Vector2 GetRandomPosition()
    {
        var v = Random.insideUnitCircle * radius + (Vector2)GameManager.Instance.player.transform.position;
        return new(v.x * 0.5f,v.y);
    }
}

class SharkAttack : BaseState
{
    Vector2 targetdir = Vector2.zero;
    public SharkAttack(Monster m) : base(m) { }

    public override void OnStateEnter()
    {
        targetdir = GameManager.Instance.player.transform.position - monster.transform.position;
        ((Shark)monster).animator.SetTrigger("Attack");
    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        monster.transform.position += (Vector3)targetdir.normalized * (((Shark)monster).ChargeSpeed * Time.deltaTime);
    }
}
