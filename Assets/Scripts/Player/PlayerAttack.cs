using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PlayerAttack : MonoBehaviour
{
    public Weapon weapon;
    [SerializeField] private GameObject attackDirectionObject;
    private PlayerMove playerMove;
    private Vector2 attackDirection;

    void Start()
    {
        playerMove = GetComponent<PlayerMove>();
        Weapon initWeapon = gameObject.AddComponent<Shotgun>();
        initWeapon.InitStat(Weapon.WeaponRare.Common);
        weapon = initWeapon;
    }

    void Update()
    {
        attackDirection = CalculateAttackDirection(weapon.weaponAttackDirectionType);
        attackDirectionObject.transform.rotation = CalculateAttackDirectionObjectRotation(attackDirection);
        if (!weapon.isAttackCooldown)
        {
            StartCoroutine(weapon.Attack(attackDirection));
        }
    }

    private Vector2 CalculateAttackDirection(Weapon.WeaponAttackDirectionType weaponAttackDirectionType)
    {
        switch (weaponAttackDirectionType)
        {
            case Weapon.WeaponAttackDirectionType.Nearest:
                GameObject[] mosnters = GameObject.FindGameObjectsWithTag("Monster");
                GameObject nearestMonster = mosnters.OrderBy(x => Math.Abs(Vector2.Distance(x.transform.position, transform.position))).FirstOrDefault();
                return nearestMonster.transform.position - transform.position;
            case Weapon.WeaponAttackDirectionType.Aim:
                if ((Vector2)playerMove.inputVec == Vector2.zero)
                {
                    return attackDirection;
                }
                return playerMove.inputVec;
            default:
                return Vector2.zero;
        }
    }

    private Quaternion CalculateAttackDirectionObjectRotation(Vector2 attackDirection)
    {
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg - 90;
        return Quaternion.Euler(new Vector3(0, 0, angle));
    }

}
