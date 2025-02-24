using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelUpSelectManager : Singleton<LevelUpSelectManager>
{
    enum SelectItemType
    {
        MainWeapon,
        SubWeapon,
        SupportItem,
    }

    private const int MAX_RETRY_COUNT = 10;  // 최대 시도 횟수
    private const int MAX_TOTAL_RETRY_COUNT = 30;  // 전체 아이템 선택 최대 시도 횟수

    private LitJson.JsonData GetNonDuplicateMainWeapon(LitJson.JsonData userMainWeaponData, Weapon.MainWeapon currentMainWeapon)
    {
        int tryCount = 0;
        while (tryCount < MAX_RETRY_COUNT)
        {
            int mainWeaponIndex = UnityEngine.Random.Range(0, userMainWeaponData.Count);
            LitJson.JsonData userData = userMainWeaponData[mainWeaponIndex];

            if (userData == null || userData.Count == 0)
            {
                tryCount++;
                continue;
            }

            Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(typeof(Weapon.MainWeapon.WeaponType), userData["weaponType"]["N"].ToString());

            // 현재 장착된 무기와 다른 타입인 경우에만 반환
            if (currentMainWeapon == null || weaponType != currentMainWeapon.weaponType)
            {
                return userData;
            }

            tryCount++;
        }

        return null;  // 유효한 무기를 찾지 못한 경우
    }

    private WeaponStatProvider.SubWeaponStat GetSubWeaponOption(List<WeaponStatProvider.SubWeaponStat> subWeaponStats, List<Weapon.SubWeapon> currentSubWeapons)
    {
        // 현재 장착된 SubWeapon들 중 랜덤 선택 시도
        if (currentSubWeapons.Count > 0)
        {
            int tryCount = 0;
            while (tryCount < MAX_RETRY_COUNT)
            {
                int weaponIndex = UnityEngine.Random.Range(0, currentSubWeapons.Count);
                Weapon.SubWeapon currentWeapon = currentSubWeapons[weaponIndex];

                // 최대 레벨이 아닌 경우에만 선택
                if (currentWeapon.weaponGrade < currentWeapon.maxWeaponGrade)
                {
                    return subWeaponStats.Find(x =>
                        x.weaponType == currentWeapon.weaponType &&
                        x.weaponGrade == currentWeapon.weaponGrade + 1
                    );
                }
                tryCount++;
            }
        }

        // 새로운 SubWeapon 선택 (weaponGrade = 0)
        var availableNewWeapons = subWeaponStats.Where(x =>
            x.weaponGrade == 0 &&
            !currentSubWeapons.Any(c => c.weaponType == x.weaponType)
        ).ToList();

        if (availableNewWeapons.Count > 0)
        {
            int newWeaponIndex = UnityEngine.Random.Range(0, availableNewWeapons.Count);
            return availableNewWeapons[newWeaponIndex];
        }

        return default;
    }

    public void CreateItemList()
    {
        LitJson.JsonData userMainWeaponData = BackendDataManager.Instance.GetUserMainWeaponData();
        List<WeaponStatProvider.SubWeaponStat> subWeaponStats = WeaponStatProvider.Instance.subWeaponStats;
        // List<WeaponStatProvider.SupportItemStat> supportItemStats = new();

        PlayerAttack playerAttack = GameManager.Instance.playerAttack;
        Weapon.MainWeapon currentMainWeapon = playerAttack.mainWeapon;
        List<Weapon.SubWeapon> currentSubWeapons = playerAttack.subWeapons;

        int totalTryCount = 0;
        for (int i = 0; i < 3; i++)
        {
            if (totalTryCount >= MAX_TOTAL_RETRY_COUNT)
            {
                Debug.LogWarning($"[LevelUpSelectManager] Failed to create item list after {MAX_TOTAL_RETRY_COUNT} attempts");
                break;
            }

            int itemIndex = UnityEngine.Random.Range(0, 3);
            SelectItemType selectItemType = (SelectItemType)itemIndex;
            bool succeeded = false;

            switch (selectItemType)
            {
                case SelectItemType.MainWeapon:
                    LitJson.JsonData userData = GetNonDuplicateMainWeapon(userMainWeaponData, currentMainWeapon);
                    if (userData != null)
                    {
                        // UI 버튼에 이벤트리스너를 추가하여 선택 시 해당 무기로 변경
                        CreateMainWeaponItem(userData);
                        succeeded = true;
                    }
                    break;
                case SelectItemType.SubWeapon:
                    // SubWeapon이 6개 이상인 경우에는 선택하지 않음
                    if (playerAttack.subWeapons.Count == 6)
                    {
                        i--;
                        continue;
                    }
                    WeaponStatProvider.SubWeaponStat subWeaponStat = GetSubWeaponOption(subWeaponStats, currentSubWeapons);
                    if (!Equals(subWeaponStat, default))
                    {
                        bool isUpgrade = subWeaponStat.weaponGrade > 0;
                        // UI 버튼에 이벤트리스너를 추가하여 선택 시 해당 무기 추가 또는 업그레이드
                        CreateSubWeaponItem(subWeaponStat);
                        succeeded = true;
                    }
                    break;
                case SelectItemType.SupportItem:
                    i--;
                    continue;
                    // 현재 지원품 미구현으로 인해 주석 처리
                    // int supportItemIndex = Random.Range(0, supportItemStats.Count);
                    // WeaponStatProvider.SupportItemStat supportItemStat = supportItemStats[supportItemIndex];
                    // CreateSupportItem(supportItemStat);
                    succeeded = true;
                    break;
            }

            if (!succeeded)
            {
                i--; // 현재 시도를 다시 하도록 인덱스 감소
                totalTryCount++;
            }
        }
    }


    private void CreateMainWeaponItem(LitJson.JsonData userData)
    {
        PlayerAttack playerAttack = GameManager.Instance.playerAttack;

        try
        {
            Destroy(playerAttack.mainWeapon.attackObject);
            Destroy(playerAttack.mainWeapon);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LevelUpSelectManager] Failed to destroy main weapon: {e.Message}");
        }
        playerAttack.mainWeapon = null;
        Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(typeof(Weapon.MainWeapon.WeaponType), userData["weaponType"]["N"].ToString());
        string weaponTypeString = weaponType.ToString();
        Weapon.WeaponRare weaponRare = (Weapon.WeaponRare)Enum.Parse(typeof(Weapon.WeaponRare), userData["weaponRare"]["N"].ToString());
        Type type = Type.GetType(weaponTypeString);
        if (type == null)
        {
            Debug.LogWarning($"[LevelUpSelectManager] Failed to create main weapon: {weaponTypeString}");
            return;
        }
        Weapon.MainWeapon newMainWeapon = (Weapon.MainWeapon)playerAttack.gameObject.AddComponent(type);
        newMainWeapon.weaponRare = weaponRare;
        playerAttack.mainWeapon = newMainWeapon;
        Debug.Log($"[LevelUpSelectManager] Main weapon changed to {newMainWeapon.weaponType}");

    }

    private void CreateSubWeaponItem(WeaponStatProvider.SubWeaponStat subWeaponStat)
    {
        PlayerAttack playerAttack = GameManager.Instance.playerAttack;
        if (subWeaponStat.weaponGrade == 0)
        {
            playerAttack.AddSubWeapon(subWeaponStat.weaponType);
            Debug.Log($"[LevelUpSelectManager] Sub weapon added: {subWeaponStat.weaponType}");
        }
        else
        {
            playerAttack.ModifySubWeaponGrade(subWeaponStat.weaponType, subWeaponStat.weaponGrade);
            Debug.Log($"[LevelUpSelectManager] Sub weapon upgraded: {subWeaponStat.weaponType} to grade {subWeaponStat.weaponGrade}");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelUpSelectManager))]
    public class LevelUpSelectManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelUpSelectManager manager = (LevelUpSelectManager)target;

            if (GUILayout.Button("테스트: 아이템 리스트 생성"))
            {
                manager.CreateItemList();
            }
        }
    }
#endif

}