using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using BackEnd;
using System.Data;

public class PlayerAttack : MonoBehaviour
{
    public Weapon.MainWeapon mainWeapon;
    public List<Weapon.SubWeapon> subWeapons = new();
    [SerializeField] private GameObject attackDirectionObject;
    private PlayerMove playerMove;
    private Vector2 attackDirection = new(0, 1);

    void Start()
    {
        playerMove = GetComponent<PlayerMove>();
        Weapon.MainWeapon initWeapon = gameObject.AddComponent<Shotgun>();
        initWeapon.weaponRare = GetWeaponRare(initWeapon.weaponType);
        mainWeapon = initWeapon;
        subWeapons.Add(gameObject.AddComponent<PaperPlane>());
    }

    void Update()
    {
        attackDirection = CalculateAttackDirection(mainWeapon.weaponAttackDirectionType);
        attackDirectionObject.transform.rotation = CalculateAttackDirectionObjectRotation(attackDirection);
        if (!mainWeapon.isAttackCooldown)
        {
            StartCoroutine(mainWeapon.Attack(attackDirection));
        }
        foreach (Weapon.SubWeapon subWeapon in subWeapons)
        {
            if (!subWeapon.isAttackCooldown)
            {
                Vector2 subWeaponAttackDirection = CalculateAttackDirection(subWeapon.weaponAttackDirectionType);
                StartCoroutine(subWeapon.Attack(subWeaponAttackDirection));
            }
        }
    }

    public Weapon.WeaponRare GetWeaponRare(Weapon.MainWeapon.WeaponType weaponType)
    {
        // 테스트 용 코드입니다.
        if (BackendDataManager.Instance.isSignedIn == false)
        {
            Backend.Initialize();
            Backend.BMember.CustomLogin("admin", "12345678");
            BackendDataManager.Instance.isSignedIn = true;
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
