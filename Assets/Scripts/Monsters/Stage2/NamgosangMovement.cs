using UnityEngine;

public class NamgosangMovement : NormalMonster
{
    public float amplitude = 1f; // 진폭
    public float frequency = 1f; // 주기
    protected override void Start()
    {
        movement = new SinMovement(amplitude, frequency, moveSpeed);
        base.Start();
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.TryGetComponent<Player>(out var player))
            {
                //player.TakeDamage(damage);
            }
        }
    }
}
