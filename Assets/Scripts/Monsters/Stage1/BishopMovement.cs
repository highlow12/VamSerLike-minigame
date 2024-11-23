using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BishopMovement :  NormalMonster
{
    public Animator animator;
    protected override void Start()
    {
        movement = new ChessBishopMovement(this);
        base.Start();
    }
}
class ChessBishopMovement : IMovement
{
    
    float length = 0;
    float delay = 0;
    float duration =0;
    Vector2 c = Vector2.zero;
    Vector2 p = Vector2.zero;
    Vector2 t = Vector2.zero;
    BishopMovement tr = null;

    Tween activeTween = null;
    public ChessBishopMovement(BishopMovement tr ,float len = 5, float delay = 3, float duration = 1 )
    {
        length = len;
        this.delay = delay;
        this.duration = duration;
        this.tr =tr;
    }
    public void CalculateMovement(Transform transform)
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = currentPosition + new Vector2(length, length); // 예제: 대각선 이동

        if (activeTween == null || (bool)!activeTween?.IsPlaying())
        {
            startmotion(currentPosition, targetPosition);
        }
    }
    
    void startmotion(Vector2 currentPosition, Vector2 targetPosition)
    {
        var dir = targetPosition - currentPosition;
        dir = dir.normalized;
        c = currentPosition;
        t = dir * length; 
        

        activeTween = DOVirtual.DelayedCall(delay, () =>
        {
            tr.animator.SetTrigger("StartTrigger");
            Vector2 startPosition = c; // 현재 위치
            Vector2 endPosition = t; // 목표 위치
            // Lerp와 유사한 트윈 설정
            DOTween.To(() => 0f, (val) => 
            {
               p = Vector2.Lerp(c,t,val);
            },1f,duration).SetEase(Ease.Linear);
        });
    }
    
}
