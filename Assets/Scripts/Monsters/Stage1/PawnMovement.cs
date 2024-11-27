using UnityEngine;
using DG.Tweening;
using System;

public class PawnMoveMent : NormalMonster
{
    protected override void Start()
    {
        movement = new ChessStepMovement();
        base.Start();
    }
}

class ChessStepMovement : IMovement
{
    Vector2 tweeningPos = Vector2.zero;
    float jumpPower = 2;
    float duration = 1;
    float speed = 1;
    Tweener activeTween;

    public ChessStepMovement(float jumpPower = 2, float duration = 1, float speed = 1)
    {
        this.jumpPower = jumpPower;
        this.duration = duration;
        this.speed = speed;
    }

    public void CalculateMovement(Transform transform)
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = currentPosition + new Vector2(0, speed); // 예제: 앞으로 이동

        if (activeTween == null || (bool)!activeTween?.IsPlaying())
        {
            StartJumpMotion(currentPosition, targetPosition, jumpPower, speed, (pos) => 
            {
                tweeningPos = pos;
                transform.position = tweeningPos; // 트랜스폼 위치 업데이트
            });
        }
    }

    public void StartJumpMotion(Vector2 startPos, Vector2 endPos, float jumpPower = 2f, float speed = 1f, Action<Vector2> onPositionUpdate = null)
    {
        var dir = endPos - startPos;
        dir = dir.normalized;
        var newEndPos = startPos + dir * speed;

        // 진행도를 0에서 1까지 트위닝
        activeTween = DOTween.To(() => 0f, (progress) =>
        {
            // 현재 위치 계산
            Vector2 currentPos = GetJumpPosition(startPos, newEndPos, jumpPower, progress);
            var returnPos = startPos - currentPos;
            // 위치 업데이트 콜백 호출
            onPositionUpdate?.Invoke(-returnPos);

        }, 1f, duration);
    }

    private Vector2 GetJumpPosition(Vector2 startPos, Vector2 endPos, float jumpPower, float progress)
    {
        // 점프 위치 계산 로직 (예제)
        float height = Mathf.Sin(Mathf.PI * progress) * jumpPower;
        return Vector2.Lerp(startPos, endPos, progress) + new Vector2(0, height);
    }
}