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
    Tweener activeTween;
    public ChessStepMovement(float jumpPower = 2, float duration = 1)
    {
        this.jumpPower = jumpPower;
        this.duration = duration;
    }
    public Vector2 CalculateMovement(Vector2 currentPosition, Vector2 targetPosition)
    {
        if (activeTween == null || (bool)!activeTween?.IsPlaying())
        {
            StartJumpMotion(currentPosition,targetPosition,jumpPower,duration,(pos) => tweeningPos = pos );
        }
        return tweeningPos;
    }

    public Vector2 GetJumpPosition(Vector2 startPos, Vector2 endPos, float jumpPower, float progress)
    {
        // 수평 이동 계산
        Vector2 horizontalMove = Vector2.Lerp(startPos, endPos, progress);
        
        // 점프 높이 계산 (포물선)
        float jumpHeight = jumpPower * Mathf.Cos(progress * Mathf.PI);
        
        // 최종 위치 계산
        return horizontalMove + Vector2.up * jumpHeight;
    }

    public void StartJumpMotion(Vector2 startPos, Vector2 endPos, float jumpPower = 2f, float speed = 1f, Action<Vector2> onPositionUpdate = null)
    {
        var dir = endPos - startPos;
        dir = dir.normalized;
        var newEndPos = startPos + dir * speed;
        Debug.Log(startPos+" "+newEndPos);
        // 진행도를 0에서 1까지 트위닝
        activeTween = DOTween.To(() => 0f, (progress) =>
        {
            // 현재 위치 계산
            Vector2 currentPos = GetJumpPosition(startPos, newEndPos, jumpPower, progress);
            var returnPos = startPos - currentPos;
            // 위치 업데이트 콜백 호출
            onPositionUpdate?.Invoke(-returnPos);
            
        }, 1f, duration)
        .SetEase(Ease.Linear); // 일정한 속도로 진행
    }
}