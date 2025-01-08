
using UnityEngine;

public class Principal : NormalMonster
{
    Animator animator;
    protected override void Start()
    {
        movement = new PrinMovement(moveSpeed);
        animator = GetComponent<Animator>();
        base.Start();
    }
}

public class PrinMovement : IMovement
{
    float speed;
    public PrinMovement(float speed)
    {
        this.speed = speed;
    }
    public void CalculateMovement(Transform transform)
    {
        var tartget = GameManager.Instance.player.transform.position;
        transform.rotation = Quaternion.Euler(0, 0, GetZRotation(transform.position, tartget) + 90);
        transform.position = Vector2.MoveTowards(transform.position, tartget, speed * Time.deltaTime);
    }

    // A가 B를 바라보도록 하는 Z축 회전을 반환하는 함수
    public static float GetZRotation(Vector2 A, Vector2 B)
    {
        // B로 향하는 방향 벡터를 계산
        Vector2 direction = B - A;

        // 방향 벡터로부터 각도를 계산 (라디안 -> 도)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Z축 회전을 반환
        return angle;
    }
}
