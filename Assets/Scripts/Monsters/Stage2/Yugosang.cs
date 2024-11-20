using UnityEngine;

public class Yugosang : NormalMonster
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        movement = new FloatMovement(1,0);
        base.Start();
    }

}
