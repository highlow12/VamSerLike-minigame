using UnityEngine;

public class SnowballProjectile : MonoBehaviour
{
    public float damage = 1f;
    public float speed = 5f;
    public float lifetime = 3f;
    public Vector2 direction;

    private float startTime;

    void OnEnable()
    {
        startTime = Time.time;
    }

    void Start()
    {
        // 방향이 설정되지 않았다면 플레이어 방향으로 설정
        if (direction == Vector2.zero && GameManager.Instance.player != null)
        {
            Transform playerTransform = GameManager.Instance.player.transform;
            direction = (playerTransform.position - transform.position).normalized;
        }
    }

    void Update()
    {
        // 수명이 다했으면 파괴
        if (Time.time - startTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 지정된 방향으로 이동
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 충돌 시 데미지
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }

    }
}