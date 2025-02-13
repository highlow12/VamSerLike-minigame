
using UnityEngine;

public class BishopMovement : NormalMonster
{
    public bool isMoving { get; private set; }
    public float amplitude = .1f;
    public float frequency = 2;
    protected override void Start()
    {
        movement = new ChessBishopMovement(this, moveSpeed, frequency, amplitude);
        base.Start();
    }
    public void StartMovement()
    {
        isMoving = true;
    }
    public void StopMovement()
    {
        isMoving = false;
    }
}
class ChessBishopMovement : IMovement
{
    Vector2 targetDir;
    BishopMovement bishop;
    float speed = 1;
    float d = 0.1f;
    float amplitude;
    float frequency;

    public ChessBishopMovement(BishopMovement bishop, float speed, float frequency,float amplitude)
    {
        this.bishop = bishop;
        this.speed = speed;
        this.amplitude = amplitude;
        this.frequency = frequency;
    }

    public void CalculateMovement(Transform transform)
    {
        if (bishop.isMoving)
        {
            d = 0.1f;
            transform.position += (Vector3)targetDir * speed * Time.deltaTime;
        }
        else
        {
            d += Time.deltaTime * frequency;
            targetDir = (GameManager.Instance.player.transform.position - transform.position).normalized;
            transform.position += new Vector3(0, Mathf.Sin(d) * amplitude * Time.deltaTime, 0);

        }
    }



}
