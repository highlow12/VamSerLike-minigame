using UnityEngine;
using System.Collections.Generic;

public class TrainCrew : BossMonster
{
    enum State
    {
        walk = 0,
        FastWalk = 1,
        Attack = 2
    }
    
    public Animator animator { get; private set; }
    public float walkSpeed = 2f;
    public float fastWalkSpeed = 4f;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected override void checkeState()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);
    
        // Smooth state transition based on distance
        if (distance <= attackRange)
        {
            // Attack: strike animation with fire extinguisher
            if (!(monsterFSM.currentState is TrainCrewAttack))
            {
                monsterFSM.ChangeState(states[(int)State.Attack]);
            }
        }
        else if (distance < detectionRange * 0.5f) // adjust threshold as needed for fast approach
        {
            // Fast-walk: momentarily speed up to approach the player
            if (!(monsterFSM.currentState is TrainCrewFastWalk))
            {
                monsterFSM.ChangeState(states[(int)State.FastWalk]);
            }
        }
        else
        {
            // Regular walk
            if (!(monsterFSM.currentState is TrainCrewWalk))
            {
                monsterFSM.ChangeState(states[(int)State.walk]);
            }
        }
    }

    protected override void initState()
    {
        states = new Dictionary<int, BaseState>
        {
            { (int)State.walk, new TrainCrewWalk(this) },
            { (int)State.FastWalk, new TrainCrewFastWalk(this) },
            { (int)State.Attack, new TrainCrewAttack(this) }
        };
        monsterFSM = new MonsterFSM(states[(int)State.walk]);
    }
}

// Updated state classes:

class TrainCrewWalk : BaseState
{
    public TrainCrewWalk(Monster m) : base(m) { }
    public override void OnStateEnter()
    {
        // Reset fastwalk animation
        TrainCrew tc = monster as TrainCrew;
        if(tc && tc.animator)
            tc.animator.SetBool("fastwalk", false);
    }
    public override void OnStateExit()
    {
        // ...exit walk state logic...
    }
    public override void OnStateUpdate()
    {
        // Move toward the player at normal walk speed
        TrainCrew tc = monster as TrainCrew;
        if(tc)
        {
            monster.transform.position = Vector3.MoveTowards(
                monster.transform.position, 
                GameManager.Instance.player.transform.position, 
                tc.walkSpeed * Time.deltaTime
            );
        }
    }
}

class TrainCrewFastWalk : BaseState
{
    public TrainCrewFastWalk(Monster m) : base(m) { }
    public override void OnStateEnter()
    {
        // Set fastwalk animation parameter
        TrainCrew tc = monster as TrainCrew;
        if(tc && tc.animator)
            tc.animator.SetBool("fastwalk", true);
        // ...enter additional fast-walk logic...
    }
    public override void OnStateExit()
    {
        TrainCrew tc = monster as TrainCrew;
        if(tc && tc.animator)
            tc.animator.SetBool("fastwalk", false);
        // ...exit fast-walk state logic...
    }
    public override void OnStateUpdate()
    {
        // Move toward the player at fast walk speed
        TrainCrew tc = monster as TrainCrew;
        if(tc)
        {
            monster.transform.position = Vector3.MoveTowards(
                monster.transform.position,
                GameManager.Instance.player.transform.position,
                tc.fastWalkSpeed * Time.deltaTime
            );
        }
    }
}

class TrainCrewAttack : BaseState
{
    public TrainCrewAttack(Monster m) : base(m) { }
    public override void OnStateEnter()
    {
        // Use the animator from TrainCrew to trigger the attack animation.
        TrainCrew tc = monster as TrainCrew;
        if(tc && tc.animator)
            tc.animator.SetTrigger("Attack");
        // ...enter attack state logic...
    }
    public override void OnStateExit()
    {
        // ...exit attack state logic...
    }
    public override void OnStateUpdate()
    {
        // ...update attack state logic...
    }
}
