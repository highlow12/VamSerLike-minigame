using UnityEngine;

public class NamgosangMovement : NormalMonster
{
    public Animator animator;
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
        this.frequency = frequency == 0 ? 1 : frequency; // 0으로 나누는 것을 방지
        this.amplitude = Mathf.Clamp01(amplitude);
    }

    public void CalculateMovement(Transform transform)
    {
        delta += Time.deltaTime;
        Vector2 currentPosition = transform.position;

        float offsetY = Mathf.Sin(delta * Mathf.PI / frequency) * amplitude;
        delta = delta >= frequency ? 0 : delta;

        transform.position = new Vector2(currentPosition.x, currentPosition.y + offsetY);
    }
}