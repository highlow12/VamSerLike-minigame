using System.Collections;
using UnityEngine;

public class HolyWaterObject : Projectile
{
    [SerializeField] private GameObject holyWaterSplashObject;

    protected override void Awake()
    {
        base.Awake();
    }

    public void DisableProjectileSprite()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        holyWaterSplashObject.SetActive(true);
        float scale = 1 / transform.localScale.x * attackRange;
        holyWaterSplashObject.transform.localScale = new Vector3(scale, scale, 1);
        StartCoroutine(Despawn());
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(3f);
        GetComponent<SpriteRenderer>().enabled = true;
        holyWaterSplashObject.SetActive(false);
        GetComponent<PoolAble>().ReleaseObject();
    }

}
