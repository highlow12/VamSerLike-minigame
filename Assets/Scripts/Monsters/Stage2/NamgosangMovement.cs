using UnityEngine;

public class NamgosangMovement : NormalMonster
{
    protected override void Start()
    {
        movement = new FloatMovement();
        base.Start();
    }
}

class FloatMovement : IMovement
{
    float frequency = 0;
    float amplitude = 0;
    float delta = 0;
    public FloatMovement(float frequency = 1, float amplitude = 1)
    {
        frequency = frequency == 0 ? 1 : frequency; // Solve divide by 0
        this.frequency = frequency;
        amplitude = Mathf.Clamp01(amplitude);
        this.amplitude = amplitude;
    }
    public Vector2 CalculateMovement(Vector2 currentPosition, Vector2 targetPosition)
    {
        delta += Time.deltaTime;

        var returnVec = new Vector2(0,Mathf.Sin(delta * Mathf.PI / frequency) * amplitude);
        delta = delta >= frequency ? 0 : delta;
        var dir = targetPosition - currentPosition;
        dir.Normalize();

        returnVec += dir;

        return returnVec;
    }
}