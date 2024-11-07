using UnityEngine;

public class ExampleNoamlMonster : NormalMonster
{
    protected override void Start()
    {
        movement = new StraightMovement();
        base.Start();
    }

    protected override void CheckAttackRange()
    {
        var distance = Vector2.Distance(transform.position, playerTransform.position);
        if (distance <= 1)
        {
            Attack();
        }
    }

    protected override void Attack()
    {
        Debug.Log("Attack");
        //playerTransform.gameObject.GetComponent<>
    }
    protected override void Die()
    {
        base.Die();
        Debug.Log("Die " + gameObject.name);
        Destroy(this);
    }
}
