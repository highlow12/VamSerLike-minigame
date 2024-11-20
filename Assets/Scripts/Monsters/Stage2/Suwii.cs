using UnityEngine;

public class Suwii : NormalMonster
{
    public GameObject TriggerArea;
    private PolygonCollider2D polygonCollider;

    protected override void Start()
    {
        movement = new FloatMovement(0, 0);
        polygonCollider = TriggerArea.GetComponent<PolygonCollider2D>();
        base.Start();
    }

    protected override void Update()
    {
        Vector2 playerPosition = GameManager.Instance.player.transform.position;
        GameManager.Instance.player.SpeedDebuff = polygonCollider.OverlapPoint(playerPosition);
        base.Update();
    }
}
