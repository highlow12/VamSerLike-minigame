using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public float speed;
    public float attackRange;
    public float attackDamage;
    public Vector2 initialPosition;
    protected Rigidbody2D _rb;
    protected float positionDelta;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }


}
