using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public int speed;
    public int attackRange;
    public Vector2 initialPosition;
    protected Rigidbody2D _rb;
    protected float positionDelta;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }


}
