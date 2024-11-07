using UnityEngine;

public class ShotgunBullet : Projectile
{
    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {

    }

    void FixedUpdate()
    {
        float yVelocity = Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.z) * speed;
        float xVelocity = Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.z) * speed;
        _rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        positionDelta = Vector2.Distance(initialPosition, transform.position);
        if (positionDelta > attackRange)
        {
            GetComponent<PoolAble>().ReleaseObject();
        }
    }
}
