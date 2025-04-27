using UnityEngine;
using DG.Tweening;
using System;
using Unity.Collections;

public class PawnMoveMent : NormalMonster
{
    [SerializeField] float jumpPower = 1;
    [SerializeField] float delay = 0;
    protected override void Start()
    {
        movement = new ChessStepMovement(this, jumpPower, delay, moveSpeed);
        base.Start();
    }
}

class ChessStepMovement : IMovement
{
    protected Vector2 startPos;
    protected Vector2 targetPos;
    protected float jumpPower;
    protected float delay;
    protected float speed;
    protected float Timer = 0;
    protected float elapsedTime = 0;
    protected bool isJumping = false;
    protected float gravity = -1f;
    protected float initialVelocityY;
    protected Monster monster; // 몬스터 참조 추가
    protected bool isJumpSoundPlayed = false;

    public ChessStepMovement(float jumpPower, float delay, float speed)
    {
        this.jumpPower = jumpPower;
        this.delay = delay;
        this.speed = speed;
        this.initialVelocityY = jumpPower;
        this.monster = null;
    }
    
    // 생성자 오버로드 - 몬스터 참조를 받는 버전 추가
    public ChessStepMovement(Monster monster, float jumpPower, float delay, float speed)
    {
        this.monster = monster;
        this.jumpPower = jumpPower;
        this.delay = delay;
        this.speed = speed;
        this.initialVelocityY = jumpPower;
    }

    public virtual void CalculateMovement(Transform transform)
    {
        Timer += Time.deltaTime;

        if (Timer >= delay && !isJumping)
        {
            Timer = 0;
            startPos = transform.position;
            targetPos = transform.position + (GameManager.Instance.player.transform.position - transform.position).normalized * speed;
            elapsedTime = 0;
            isJumping = true;
            
            // 몬스터 참조가 있을 경우 점프 시 이동 사운드 재생
            if (monster != null)
            {
                monster.SendMessage("PlayMoveSound", SendMessageOptions.DontRequireReceiver);
                isJumpSoundPlayed = true;
            }
        }

        if (isJumping)
        {
            if (elapsedTime >= 1)
            {
                // 점프 종료 시 사운드 중지
                if (monster != null && isJumpSoundPlayed)
                {
                    monster.SendMessage("StopMoveSound", SendMessageOptions.DontRequireReceiver);
                    isJumpSoundPlayed = false;
                }
                isJumping = false;
                return;
            }

            elapsedTime += Time.deltaTime;
            
            var newPosX = Mathf.Lerp(startPos.x, targetPos.x, elapsedTime);
            var newPosY = func(elapsedTime) * jumpPower + Mathf.Lerp(startPos.y, targetPos.y, elapsedTime);

            transform.position = new Vector2(newPosX, newPosY);

        }
    }

    protected float func(float x)
    {
        return -1 * Mathf.Pow(2*x - 1, 2) + 1;
    }
}