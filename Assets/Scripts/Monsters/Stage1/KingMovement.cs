using UnityEngine;

public class KingMovement : BossMonster
{
    protected override void checkeState()
    {
        throw new System.NotImplementedException();
    }

    protected override void initState()
    {
        throw new System.NotImplementedException();
    }
}

public class KingMove : BaseState
{
    public KingMove(BossMonster bossMonster) : base(bossMonster)
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

public class KingIdle : BaseState
{
    public KingIdle(BossMonster bossMonster) : base(bossMonster)
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
