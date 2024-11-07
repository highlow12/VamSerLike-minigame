using System.Collections;
using UnityEngine;

public class PawnMovement : Monster
{
    public PawnMovement()
    {
        maxHealth = 100;
        damage = 10;
        defense = 5;
        detectionRange = 10;
        attackRange = 1.5f;
        isDead = false;
    }

    private Coroutine moveCoroutine;
    private Animator animator;
    public IMovement movement;
    public float moveCooldown = 1f;
    public float moveDistance = 1.25f;
    public bool isMoveCooldown = false;


    protected override void Attack()
    {
    }

    protected override void Start()
    {
        movement = new StraightMovement();
        animator = GetComponent<Animator>();
        base.Start();
    }

    IEnumerator MoveCoro(float distance)
    {
        if (!(distance <= detectionRange && distance > attackRange) || isMoveCooldown)
        {
            moveCoroutine = null;
            yield break;
        }
        isMoveCooldown = true;
        animator.SetInteger("MoveState", 1);
        float positionDelta = 0.0f;
        Vector2 movementVector = movement.CalculateMovement(
            transform.position,
            playerTransform.position
        );
        while (positionDelta < moveDistance)
        {
            transform.position += (Vector3)movementVector * Time.deltaTime;
            positionDelta += movementVector.magnitude * Time.deltaTime;
            yield return null;
        }
        animator.SetInteger("MoveState", 0);
        yield return new WaitForSeconds(moveCooldown);
        moveCoroutine = null;
        isMoveCooldown = false;
    }

    private void Move(float distance)
    {
        if (distance <= detectionRange && distance > attackRange)
        {
            Vector2 movementVector = movement.CalculateMovement(
                transform.position,
                playerTransform.position
            );
            Debug.Log(movementVector);
            transform.position += (Vector3)movementVector * Time.deltaTime;
        }
    }

    private void Update()
    {
        if (!isDead)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            moveCoroutine ??= StartCoroutine(MoveCoro(distance));
            CheckAttackRange(distance);
        }
    }

    private void CheckAttackRange(float distance)
    {
        if (distance <= attackRange)
        {
            Attack();
        }
    }


}
