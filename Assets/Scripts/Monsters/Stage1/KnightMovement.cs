
using UnityEngine;

public class KnightMovement : NormalMonster
{
    [SerializeField] float jumpPower = 1;
    [SerializeField] float delay = 0;
    public Animator animator;
    [HideInInspector]
    public bool attackrange = false;
    protected override void Start()
    {
        movement = new ChessStepMovementKnight(this,jumpPower, delay, moveSpeed);
        base.Start();
    }

    protected override void CheckAttackRange()
    {
        if (Vector2.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            attackrange = true;
        }
        else
        {
            attackrange = false;
        }
    }
}

class ChessStepMovementKnight : ChessStepMovement
{
    KnightMovement KM;
    public ChessStepMovementKnight(KnightMovement KM,float jumpPower, float delay, float speed) : base(jumpPower, delay, speed)
    {
        this.KM = KM;
    }

    public override void CalculateMovement(Transform transform)
    {
        Timer += Time.deltaTime;

        if(KM.attackrange)
        {
            KM.animator.SetTrigger("Attack");
            return;
        }

        if (Timer >= delay && !isJumping)
        {
            Timer = 0;
            startPos = transform.position;
            targetPos = transform.position + (GameManager.Instance.player.transform.position - transform.position).normalized * speed;
            elapsedTime = 0;
            isJumping = true;
            transform.GetComponent<KnightMovement>().animator.SetTrigger("Move");
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
}

