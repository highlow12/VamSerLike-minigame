using UnityEngine;
public abstract class BaseState
{
    protected Monster monster;
    protected Transform monsterTransform;
    protected BaseState(Monster m)
    {
        monster = m;
        monsterTransform = m.transform;
    }

    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public abstract void OnStateExit();
}
public class MonsterFSM
{
    public BaseState currentState{get; private set;}

    public MonsterFSM(BaseState initState)
    {
        currentState = initState;
    }

    public void ChangeState(BaseState nextState)
    {
        if (currentState == nextState) return;

        if(currentState != null) currentState.OnStateExit();

        currentState = nextState;
        currentState.OnStateEnter();
    }

    public void UpdateState()
    {
        currentState?.OnStateUpdate();
    }
}

//example states
class IdleState : BaseState
{
    public IdleState(Monster m) : base(m) {}
    public override void OnStateEnter()
    {
        throw new System.NotImplementedException();
    }

    public override void OnStateExit()
    {
        throw new System.NotImplementedException();
    }

    public override void OnStateUpdate()
    {
        throw new System.NotImplementedException();
    }
}

class MoveState : BaseState
{
    public MoveState(Monster m) : base(m) {}
    public override void OnStateEnter()
    {
        throw new System.NotImplementedException();
    }

    public override void OnStateExit()
    {
        throw new System.NotImplementedException();
    }

    public override void OnStateUpdate()
    {
        throw new System.NotImplementedException();
    }
}

class AttackState : BaseState
{
    public AttackState(Monster m) : base(m) {}
    public override void OnStateEnter()
    {
        throw new System.NotImplementedException();
    }

    public override void OnStateExit()
    {
        throw new System.NotImplementedException();
    }

    public override void OnStateUpdate()
    {
        throw new System.NotImplementedException();
    }
}