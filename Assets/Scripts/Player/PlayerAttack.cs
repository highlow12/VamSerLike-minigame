using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using BackEnd;

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
        initWeapon.weaponRare = GetWeaponRare(initWeapon.weaponType);
        initWeapon.InitStat();
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

    public Weapon.WeaponRare GetWeaponRare(Weapon.WeaponType weaponType)
    {
        // 테스트 용 코드입니다.
        if (!BackendManager.Instance.isSignedIn)
        {
            Backend.BMember.CustomLogin("admin", "12345678");
        }
        Where where = new();
        where.Equal("weaponType", (int)weaponType);
        var bro = Backend.GameData.GetMyData("Weapon", where);
        if (bro.IsSuccess() == false)
        {
            Debug.LogError($"GetMyData Failed: {bro.GetStatusCode()}\n{bro.GetMessage()}\n{bro}");
            return Weapon.WeaponRare.Common;
        }
        if (bro.GetReturnValuetoJSON()["rows"].Count == 0)
        {
            Debug.LogError("No weapon data");
            return Weapon.WeaponRare.Common;
        }
        return (Weapon.WeaponRare)Enum.Parse(typeof(Weapon.WeaponRare), bro.Rows()[0]["weaponRare"]["N"].ToString());
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
