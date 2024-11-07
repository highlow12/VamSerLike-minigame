using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerAttack : MonoBehaviour
{
    public Weapon weapon;

    void Start()
    {
        Shotgun shotgun = gameObject.AddComponent<Shotgun>();
        shotgun.InitStat(Weapon.WeaponRare.Common);
        weapon = shotgun;
    }

    void Update()
    {
        if (!weapon.isAttackCooldown)
        {
            GameObject[] mosnters = GameObject.FindGameObjectsWithTag("Monster");
            GameObject nearestMonster = mosnters.OrderBy(x => Math.Abs(Vector2.Distance(x.transform.position, transform.position))).FirstOrDefault();
            Vector2 attackDirection = nearestMonster.transform.position - transform.position;
            StartCoroutine(weapon.Attack(attackDirection));
        }
    }
}
