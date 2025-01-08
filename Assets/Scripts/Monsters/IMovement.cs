using System;
using System.Collections.Generic;
using UnityEngine;


public interface IMovement
{
    void CalculateMovement(Transform transform);

}

public class StraightMovement : IMovement
{
    float speed = 5f;
    
    public StraightMovement(float speed)
    {
        this.speed = speed;
        
    }
    public void CalculateMovement(Transform transform)
    {
        transform.position = Vector2.MoveTowards(transform.position, GameManager.Instance.player.transform.position, speed * Time.deltaTime);
        
    }
}

public class SinMovement : IMovement
{
    private readonly float amplitude;    // 좌우 흔들림의 폭
    private readonly float frequency;    // 좌우 흔들림의 빈도
    private float elapsedTime;          // 경과 시간

    public SinMovement(float amplitude = 2f, float frequency = 2f, float speed = 5f)
    {
        this.amplitude = amplitude;
        this.frequency = frequency;
        ResetTime();
    }

    public void CalculateMovement(Transform transform)
    {
        // 시간 업데이트
        elapsedTime += Time.deltaTime;

        // 기본 방향 계산
        Vector2 directionToTarget = (GameManager.Instance.player.transform.position - transform.position).normalized;
        
        // 진행 방향에 수직인 벡터 계산 (2D에서는 90도 회전)
        Vector2 perpendicularDirection = new Vector2(-directionToTarget.y, directionToTarget.x);
        
        // 사인파를 이용한 좌우 움직임 계산
        float zigzagOffset = Mathf.Sin(elapsedTime * frequency) * amplitude;
        
        // 최종 방향 계산 (기본 방향 + 좌우 움직임)
        Vector2 finalDirection = directionToTarget + (perpendicularDirection * zigzagOffset);
        finalDirection.Normalize();
        
        // 속도를 곱한 최종 이동 벡터 반환
        transform.position += (Vector3)finalDirection;
    }

    // 필요한 경우 시간을 리셋하는 메서드
    public void ResetTime()
    {
        elapsedTime = 0f;
    }
}
