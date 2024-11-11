using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BishopMovement :  NormalMonster
{
    protected override void Start()
    {
        movement = new ChessBishopMovement();
        base.Start();
    }
}
class ChessBishopMovement : IMovement
{
    Sequence sq = null;
    float length = 0;
    float delay = 0;
    float duration =0;
    Vector2 c = Vector2.zero;
    Vector2 p = Vector2.zero;
    Vector2 t = Vector2.zero;

    Tween activeTween = null;
    public ChessBishopMovement(float len = 10, float delay = 3, float duration = 1)
    {
        length = len;
        this.delay = delay;
        this.duration = duration;
    }
    public Vector2 CalculateMovement(Vector2 currentPosition, Vector2 targetPosition)
    {
        if (activeTween == null || (bool)!activeTween?.IsPlaying()) 
        {
            startmotion(currentPosition, targetPosition);
        }
        
        return p;
            
    }
    
    void startmotion(Vector2 currentPosition, Vector2 targetPosition)
    {
        var dir = targetPosition - currentPosition;
        dir = dir.normalized;
        c = currentPosition;
        t = dir * length; 

        activeTween = DOVirtual.DelayedCall(delay, () =>
        {
            Vector2 startPosition = c; // 현재 위치
            Vector2 endPosition = t; // 목표 위치
            // Lerp와 유사한 트윈 설정
            DOTween.To(() => 0f, (val) => 
            {
               p = Vector2.Lerp(c,t,val);
            },1f,duration);
        });
    }
    
}
