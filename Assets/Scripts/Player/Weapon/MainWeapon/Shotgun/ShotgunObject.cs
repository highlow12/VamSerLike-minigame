using UnityEngine;

public class ShotgunObject : AttackObject
{
    protected Animator animator;
    protected float baseAngle;
    protected float spreadDegree;
    protected int projectileCount;
    protected float projectileSpeed;
    protected float attackSpeed;
    public float firePosXOffset = 0.55f;

    public override void Setup(float baseAngle, float spreadDegree, int projectileCount, float projectileSpeed, float attackSpeed)
    {
        this.baseAngle = baseAngle;
        this.spreadDegree = spreadDegree;
        this.projectileCount = projectileCount;
        this.projectileSpeed = projectileSpeed;
        this.attackSpeed = attackSpeed;
    }

    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks = 0, int attackTarget = 0)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
    }

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void Attack()
    {
        Debug.Log($"[ShotgunFire] baseAngle: {baseAngle}, spreadDegree: {spreadDegree}, projectileCount: {projectileCount}, projectileSpeed: {projectileSpeed}, attackSpeed: {attackSpeed}, attackDamage: {attackDamage}, attackRange: {attackRange}");
        animator.SetFloat("AttackSpeed", attackSpeed);
        for (int i = 0; i < projectileCount; i++)
        {
            GameObject bullet = ObjectPoolManager.instance.GetGo("ShotgunBullet");
            float spreadAngle = (i - (projectileCount - 1) / 2f) * spreadDegree;
            Quaternion rotation = Quaternion.Euler(0, 0, baseAngle + spreadAngle);
            Vector3 firePosOffset;
            if (baseAngle > 90 || baseAngle < -90)
            {
                firePosOffset = new Vector3(-firePosXOffset * (rotation.z * Mathf.Deg2Rad), 0, 0);
            }
            else
            {
                firePosOffset = new Vector3(firePosXOffset * (rotation.z * Mathf.Deg2Rad), 0, 0);
            }
            Projectile bulletComponent = bullet.GetComponent<Projectile>();
            bulletComponent.initialPosition = transform.position + firePosOffset;
            bullet.transform.SetPositionAndRotation(bulletComponent.initialPosition, rotation);
            bulletComponent.speed = projectileSpeed;
            bulletComponent.attackRange = attackRange;
            bulletComponent.attackDamage = attackDamage;
        }
    }
}
