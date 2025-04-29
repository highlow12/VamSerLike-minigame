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
        // 비숍이 움직이기 시작할 때 사운드 재생
        PlayMoveSound();
    }
    public void StopMovement()
    {
        isMoving = false;
        // 비숍이 움직임을 멈출 때 사운드 중지
        StopMoveSound();
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
    private bool wasMoving = false;

    public ChessBishopMovement(BishopMovement bishop, float speed, float frequency, float amplitude)
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
            // 이동 상태 전환 시 사운드 재생
            if (!wasMoving)
            {
                bishop.PlayMoveSound();
                wasMoving = true;
            }
            
            d = 0.1f;
            transform.position += (Vector3)targetDir * speed * Time.deltaTime;
        }
        else
        {
            // 정지 상태 전환 시 사운드 중지
            if (wasMoving)
            {
                bishop.StopMoveSound();
                wasMoving = false;
            }
            
            d += Time.deltaTime * frequency;
            targetDir = (GameManager.Instance.player.transform.position - transform.position).normalized;
            transform.position += new Vector3(0, Mathf.Sin(d) * amplitude * Time.deltaTime, 0);
        }
    }
}
