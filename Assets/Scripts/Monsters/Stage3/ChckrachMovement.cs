using Unity.VisualScripting;
using UnityEngine;

public class ChckrachMovement : NormalMonster
{
    protected override void Start()
    {
        movement = new StraightMovement(moveSpeed);
        base.Start();
    }

    protected override void Update()
    {
        transform.localRotation = Quaternion.Euler(0, 0, GetZRotation(transform.position, GameManager.Instance.player.transform.position) - 90);
        base.Update();
        
    }
    public static float GetZRotation(Vector2 A, Vector2 B)
    {
        // B로 향하는 방향 벡터를 계산
        Vector2 direction = B - A;

        // 방향 벡터로부터 각도를 계산 (라디안 -> 도)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Debug.Log(angle);
        // Z축 회전을 반환
        return angle;
    }
}

