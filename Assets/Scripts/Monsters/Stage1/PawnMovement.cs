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
        movement = new ChessStepMovement(jumpPower, delay, moveSpeed);
        base.Start();
    }
}

class ChessStepMovement : IMovement
{
    Vector2 startPos;
    Vector2 targetPos;
    float jumpPower;
    float delay;
    float speed;
    float Timer = 0;
    float elapsedTime = 0;
    bool isJumping = false;
    float gravity = -1f;
    float initialVelocityY;

    public ChessStepMovement(float jumpPower, float delay, float speed)
    {
        this.jumpPower = jumpPower;
        this.delay = delay;
        this.speed = speed;
        this.initialVelocityY = jumpPower;
    }

    public void CalculateMovement(Transform transform)
    {
        Timer += Time.deltaTime;

        if (Timer >= delay && !isJumping)
        {
            Timer = 0;
            startPos = transform.position;
            targetPos = transform.position + (GameManager.Instance.player.transform.position - transform.position).normalized;
            elapsedTime = 0;
            isJumping = true;
        }

        if (isJumping)
        {
            if (elapsedTime >= 1)
            {
                isJumping = false;
                return;
            }

            elapsedTime += Time.deltaTime;
            
            var newPosX = Mathf.Lerp(startPos.x, targetPos.x, elapsedTime);
            var newPosY = func(elapsedTime) * jumpPower + Mathf.Lerp(startPos.y, targetPos.y, elapsedTime);

            transform.position = new Vector2(newPosX, newPosY);

        }
    }

    float func(float x)
    {
        return -1 * Mathf.Pow(2*x - 1, 2) + 1;
    }
}