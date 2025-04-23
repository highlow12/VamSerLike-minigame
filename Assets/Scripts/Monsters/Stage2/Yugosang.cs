using UnityEngine;

public class Yugosang : NormalMonster
{
    public float amplitude = 1f; // 진폭
    public float frequency = 1f; // 주기
    protected override void Start()
    {
        movement = new SinMovement(amplitude, frequency, moveSpeed);
        base.Start();
    }
// 콜라이더2D를 사용하여 몬스터의 범위에 들어온 플레이어를 감지합니다.
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                player.TakeDamage(damage);
            }
        }
    }
}
