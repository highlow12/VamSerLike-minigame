using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public abstract class Projectile : MonoBehaviour
{
    public float speed;
    public float attackRange;
    public float attackDamage;
    public Vector2 initialPosition;
    protected Rigidbody2D _rb;
    protected float positionDelta;
    protected List<Weapon.MonsterHit> monstersHit;

    protected virtual void Awake()
    {
        monstersHit = default;
        _rb = GetComponent<Rigidbody2D>();
    }


}
