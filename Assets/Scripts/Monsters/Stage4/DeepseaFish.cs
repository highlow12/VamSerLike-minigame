using UnityEngine;

public class DeepseaFish : NormalMonster
{
    [SerializeField] float MaxDistanceFormPlayer = 10.0f;
    [SerializeField] float MinDistanceFromPlayer = 5.0f;
    [SerializeField] float chaseDuration = 2f;  // 추가: Chase 상태 지속 시간
    [SerializeField] float deltaAngle = 30f;    // 추가: 이동 각도 범위
    private float currentChaseTime = 5f;        // 추가: 현재 Chase 상태 경과 시간
    private bool isChasing = false;             // 추가: 현재 Chase 중인지 여부

    IMovement IdleMovement;
    IMovement ChaseMovement;

    protected override void Start()
    {
        
        IdleMovement = new DeepseaFishIdleMovement(moveSpeed, MaxDistanceFormPlayer, MinDistanceFromPlayer, deltaAngle, transform.position);
        ChaseMovement = new StraightMovement(moveSpeed);
        movement = IdleMovement;
        base.Start();
    }
    protected override void CheckAttackRange()
    {
        if (Vector2.Distance(transform.position, GameManager.Instance.player.transform.position) < attackRange)
        {
            if (!isChasing)
            {
                movement = ChaseMovement;
                isChasing = true;
                currentChaseTime = 0f;
            }
        }

        if (isChasing)
        {
            currentChaseTime += Time.deltaTime;
            if (currentChaseTime >= chaseDuration)
            {
                movement = IdleMovement;
                isChasing = false;
            }
        }
    }
}

class DeepseaFishIdleMovement : IMovement
{
    float moveSpeed = 1.0f;
    float MaxDistanceFormPlayer = 10.0f;
    float MinDistanceFromPlayer = 5.0f;
    float deltaAngle = 45f;
    Vector2 targetPosition;

    public DeepseaFishIdleMovement(float moveSpeed, float maxDistanceFromPlayer, float minDistanceFromPlayer,float deltaAngle,Vector2 initialPosition)
    {
        this.moveSpeed = moveSpeed;
        MaxDistanceFormPlayer = maxDistanceFromPlayer;
        MinDistanceFromPlayer = minDistanceFromPlayer;
        this.deltaAngle = deltaAngle;
        targetPosition = initialPosition;
    }

    public void CalculateMovement(Transform transform)
    {
        if(Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            Vector2 playerPosition = GameManager.Instance.player.transform.position;
            Vector2 currentPosition = transform.position;
            
            Vector2 directionFromPlayer = currentPosition - playerPosition;
            float currentAngle = Vector2.SignedAngle(Vector2.right, directionFromPlayer);
            float newAngle = currentAngle + Random.Range(-deltaAngle, deltaAngle);
            float randomDistance = Random.Range(MinDistanceFromPlayer, MaxDistanceFormPlayer);

            // Use Quaternion rotation to calculate new direction
            Quaternion rotation = Quaternion.Euler(0, 0, newAngle);
            Vector2 newDirection = (Vector2)(rotation * Vector2.right);
            targetPosition = playerPosition + (newDirection * randomDistance);
        }

        // Move towards target position
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }
}
