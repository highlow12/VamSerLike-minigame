using UnityEngine;

public class Suwii : NormalMonster
{
    public GameObject TriggerArea;
    private PolygonCollider2D polygonCollider;

    protected override void Start()
    {
        movement = new SinMovement(1f, 1f, moveSpeed);
        //polygonCollider = TriggerArea.GetComponent<PolygonCollider2D>();
        base.Start();
    }

    protected override void Update()
    {
        Vector2 playerPosition = GameManager.Instance.player.transform.position;
        //GameManager.Instance.player.SpeedDebuff = polygonCollider.OverlapPoint(playerPosition);
        base.Update();
    }
    protected override void Die()
    {
        GameManager.Instance.player.SpeedDebuff = false;
        base.Die();
    }
}
