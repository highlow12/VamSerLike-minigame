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
    }

    void Update()
    {
        attackDirection = CalculateAttackDirection(mainWeapon.weaponAttackDirectionType);
        attackDirectionObject.transform.rotation = CalculateAttackDirectionObjectRotation(attackDirection);
        if (!mainWeapon.isAttackCooldown)
        {
            mainWeapon.attackDamage = GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackDamage, mainWeapon.baseAttackDamage);
            mainWeapon.attackRange = GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackRange, mainWeapon.baseAttackRange);
            mainWeapon.attackSpeed = GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackSpeed, mainWeapon.baseAttackSpeed);
            mainWeapon.attackTarget = (int)GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackTarget, mainWeapon.baseAttackTarget);
            mainWeapon.projectileCount = (int)GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackProjectileCount, mainWeapon.baseProjectileCount);
            mainWeapon.projectileSpeed = GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackProjectileSpeed, mainWeapon.baseProjectileSpeed);

            StartCoroutine(mainWeapon.Attack(attackDirection));
        }
        foreach (Weapon.SubWeapon subWeapon in subWeapons)
        {
            if (!subWeapon.isAttackCooldown)
            {
                Vector2 subWeaponAttackDirection = CalculateAttackDirection(subWeapon.weaponAttackDirectionType);
                subWeapon.attackDamage = GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackDamage, subWeapon.baseAttackDamage);
                subWeapon.attackRange = GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackRange, subWeapon.baseAttackRange);
                subWeapon.attackSpeed = GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackSpeed, subWeapon.baseAttackSpeed);
                subWeapon.attackTarget = (int)GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackTarget, subWeapon.baseAttackTarget);
                subWeapon.projectileCount = (int)GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackProjectileCount, subWeapon.baseProjectileCount);
                subWeapon.projectileSpeed = GameManager.Instance.GetPlayerStatValue(Player.BonusStat.AttackProjectileSpeed, subWeapon.baseProjectileSpeed);
                StartCoroutine(subWeapon.Attack(subWeaponAttackDirection));
            }
        }
    }

    public void AddSubWeapon(Weapon.SubWeapon.WeaponType weaponType, int initialWeaponGrade = 0)
    {
        Type type = Type.GetType(weaponType.ToString());
        // Error handling for invalid weapon type
        if (type == null)
        {
            Debug.LogError($"Type {weaponType} not found");
            DebugConsole.Line errorLine = new()
            {
                text = $"Type {weaponType} not found",
                messageType = DebugConsole.MessageType.Local,
                tick = GameManager.Instance.gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLine, "#FF0000");
            return;
        }
        // Error handling for duplicate subweapons
        if (subWeapons.Exists(x => x.weaponType == weaponType))
        {
            Debug.LogError($"SubWeapon {weaponType} already exists");
            DebugConsole.Line errorLine = new()
            {
                text = $"SubWeapon {weaponType} already exists",
                messageType = DebugConsole.MessageType.Local,
                tick = GameManager.Instance.gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLine, "#FF0000");
            return;
        }
        // Add subweapon
        Weapon.SubWeapon subWeapon = gameObject.AddComponent(type) as Weapon.SubWeapon;
        subWeapon.weaponGrade = initialWeaponGrade;
        subWeapons.Add(subWeapon);
    }

    public void ModifySubWeaponGrade(Weapon.SubWeapon.WeaponType weaponType, int weaponGrade)
    {
        Weapon.SubWeapon subWeapon = subWeapons.Find(x => x.weaponType == weaponType);
        // Error handling for invalid weapon type
        if (subWeapon == null)
        {
            Debug.LogError($"SubWeapon {weaponType} not found");
            DebugConsole.Line errorLine = new()
            {
                text = $"SubWeapon {weaponType} not found",
                messageType = DebugConsole.MessageType.Local,
                tick = GameManager.Instance.gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLine, "#FF0000");
            return;
        }
        subWeapon.weaponGrade = weaponGrade;
    }

    public void RemoveSubWeapon(Weapon.SubWeapon.WeaponType weaponType)
    {
        Weapon.SubWeapon subWeapon = subWeapons.Find(x => x.weaponType == weaponType);
        // Error handling for invalid weapon type
        if (subWeapon == null)
        {
            Debug.LogError($"SubWeapon {weaponType} not found");
            DebugConsole.Line errorLine = new()
            {
                text = $"SubWeapon {weaponType} not found",
                messageType = DebugConsole.MessageType.Local,
                tick = GameManager.Instance.gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLine, "#FF0000");
            return;
        }
        subWeapons.Remove(subWeapon);
        Destroy(subWeapon.attackObject);
        Destroy(subWeapon);
    }

    public Weapon.WeaponRare GetWeaponRare(Weapon.MainWeapon.WeaponType weaponType)
    {
        LitJson.JsonData bro = BackendDataManager.Instance.GetUserMainWeaponData();
        if (bro == null)
        {
            return Weapon.WeaponRare.Common;
        }
        int broIndex = -1;
        for (int i = 0; i < bro.Count; i++)
        {
            if (bro[i]["weaponType"]["N"].ToString() == ((int)weaponType).ToString())
            {
                broIndex = i;
                break;
            }
        }
        if (broIndex == -1)
        {
            return Weapon.WeaponRare.Common;
        }
        return (Weapon.WeaponRare)Enum.Parse(typeof(Weapon.WeaponRare), bro[broIndex]["weaponRare"]["N"].ToString());
    }

    private Vector2 CalculateAttackDirection(Weapon.WeaponAttackDirectionType weaponAttackDirectionType)
    {
        switch (weaponAttackDirectionType)
        {
            case Weapon.WeaponAttackDirectionType.Nearest:
                GameObject[] mosnters = GameObject.FindGameObjectsWithTag("Monster");
                if (mosnters.Length == 0)
                {
                    return attackDirection;
                }
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
