using UnityEngine;

public class Jellyfish : NormalMonster
{
    [SerializeField] float RandomMoveTime;
    [SerializeField] float RandomMoveRange;
    [SerializeField] float DownTime;
    protected override void Start()
    {
        movement = new JellyfishIdleMovement(moveSpeed, RandomMoveTime, DownTime,RandomMoveRange);
        base.Start();
    }
}
class JellyfishIdleMovement : IMovement
{
    float speed;
    float RandomMoveTime;
    float RandomMoveRange;
    float DownTime;
    Vector2 targetPosition; 
    float timer;
    float currentSpeed; // new field for current speed
    bool wasMovingDown; // store previous movement mode

    public JellyfishIdleMovement(float speed, float RandomMoveTime, float DownTime,float RandomMoveRange)
    {
        this.speed = speed;
        this.RandomMoveTime = RandomMoveTime;
        this.DownTime = DownTime;
        this.RandomMoveRange = RandomMoveRange;
        timer = RandomMoveTime + DownTime; // initialize timer
        currentSpeed = speed; // initialize currentSpeed
        wasMovingDown = true; // initial mode is moving down (timer > RandomMoveTime)
    }
    public void CalculateMovement(Transform transform)
    {
        if(timer <= 0 || Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            timer = RandomMoveTime + DownTime; // reset timer; no speed reset here
            // new target computed, but do not reset speed
            targetPosition = Random.insideUnitCircle * Random.Range(1, RandomMoveRange) 
                + (Vector2)GameManager.Instance.player.transform.position;
        }

        // Determine current movement mode: moving down if timer > RandomMoveTime, or towards target otherwise.
        bool isMovingDown = timer > RandomMoveTime;
        if(isMovingDown != wasMovingDown)
        {
            currentSpeed = speed;  // reset speed on direction change
            wasMovingDown = isMovingDown; // update stored movement mode
        }

        timer -= Time.deltaTime;
        if (isMovingDown) // slowly move down
        {
            transform.position += currentSpeed/3 * Time.deltaTime * Vector3.down;
        }
        else // move to target position
        {
            transform.position += currentSpeed * Time.deltaTime * 
                (Vector3)(targetPosition - (Vector2)transform.position).normalized; 
        }
        // Decelerate currentSpeed gradually.
        currentSpeed = Mathf.Max(currentSpeed - speed/(RandomMoveTime+DownTime) * Time.deltaTime, speed/2);
    }
}