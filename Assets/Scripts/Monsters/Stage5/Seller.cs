using UnityEngine;

public class Seller : NormalMonster
{
    Animator animator;
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        movement = new StraightMovement(moveSpeed);
        base.Start();
    }

    protected override void CheckAttackRange()
    {
        if (Vector2.Distance(transform.position, GameManager.Instance.player.transform.position) < attackRange)
        {
            animator.SetTrigger("Attack");
        }
    }
}
