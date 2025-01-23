using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class FamillyPictureObject : AttackObject
{
    private float barrierHealth;
    public GameObject picture;
    public override void Init(float attackDamage, float attackRange, int attackIntervalInTicks, int attackTarget)
    {
        base.Init(attackDamage, attackRange, attackIntervalInTicks, attackTarget);
        gameObject.SetActive(true);
        picture.transform.localScale = new Vector3(attackRange, attackRange, 1);
    }

    protected override void Awake()
    {
        animation = GetComponent<Animation>();
        SubWeaponInit();
    }

    void OnEnable()
    {
        barrierHealth = attackDamage;
        picture.SetActive(true);
        Attack();
    }

    protected override void Start()
    {

    }

    public override void SubWeaponInit()
    {
        spriteRenderer = picture.GetComponent<SpriteRenderer>();
        // spriteRenderer.sprite = subWeaponSO.weaponSprite;
    }

    public override void Attack()
    {
        picture.transform.DOMove(transform.position + new Vector3(0, 3, 0), 0.5f).OnComplete(() =>
        {
            picture.SetActive(false);
        });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            barrierHealth -= attackDamage;
            if (barrierHealth <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
