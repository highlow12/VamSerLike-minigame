using UnityEngine;

public class KnightMovement : NormalMonster
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        movement = new ChessStepMovement(3,1,moveSpeed);
        base.Start();
    }
}
