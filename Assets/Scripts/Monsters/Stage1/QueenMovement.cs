using System.Linq;
using DG.Tweening;
using UnityEngine;

public class QueenMovement : BossMonster
{
    float stateTimer{get;set;}
    public float[] chaseTimeRange = new float[2]; 
    public float[] idleTimerRange = new float[2];
    enum QueenAction
    {
        Idle = 0,
        Chase = 1
    }
    protected override void checkeState()
    {
        //if를 사용한 이유: swith의 case문은 컴파일 시에 값이 정해져 있어야 한다
        if(monsterFSM.currentState == states[(int)QueenAction.Idle])
        {
            if(stateTimer > 0) return;

            stateTimer = Random.Range(chaseTimeRange.Min(),chaseTimeRange.Max());

            monsterFSM.ChangeState(states[(int)QueenAction.Chase]);
        }
        else if(monsterFSM.currentState == states[(int)QueenAction.Chase])
        {
            if(stateTimer > 0) return;

            stateTimer = Random.Range(idleTimerRange.Min(),idleTimerRange.Max());

            monsterFSM.ChangeState(states[(int)QueenAction.Idle]);
        }
        stateTimer -= Time.deltaTime;
    }

    protected override void initState()
    {
        states[(int)QueenAction.Idle] = new QueenIdle(this);
        states[(int)QueenAction.Chase] = new QueenChase(this);
    }
}

class QueenIdle : BaseState
{
    public QueenIdle(Monster monster) : base(monster)
    {
    }

    public override void OnStateEnter()
    {
        throw new System.NotImplementedException();
    }

    public override void OnStateExit()
    {
        
    }

    public override void OnStateUpdate()
    {
        
    }
}
class QueenChase : BaseState
{
    public QueenChase(Monster m) : base(m)
    {
    }

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
