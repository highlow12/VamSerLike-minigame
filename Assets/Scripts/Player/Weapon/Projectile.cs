using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public abstract class Projectile : AttackObject
{
    public float speed;
    public float attackRange;
    public Vector2 initialPosition;
    protected Rigidbody2D _rb;
    protected float positionDelta;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
    }


}
