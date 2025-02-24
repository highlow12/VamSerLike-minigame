using UnityEngine;

public class Passenger : NormalMonster
{
    protected override void Start()
    {
        movement = new StraightMovement(moveSpeed);
        base.Start();
    }
}

