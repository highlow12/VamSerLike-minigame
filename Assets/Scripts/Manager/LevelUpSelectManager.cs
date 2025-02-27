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

    [SerializeField] private LevelUpUI levelUpUI;
    private List<LitJson.JsonData> mainWeaponOptions = new List<LitJson.JsonData>();
    private List<WeaponStatProvider.SubWeaponStat> subWeaponOptions = new List<WeaponStatProvider.SubWeaponStat>();

    private void Start()
    {
        // LevelUpUI 참조 가져오기
        if (levelUpUI == null)
        {
            levelUpUI = FindAnyObjectByType<LevelUpUI>();
            if (levelUpUI == null)
            {
                Debug.LogError("[LevelUpSelectManager] Cannot find LevelUpUI component in scene");
            }
        }
    }

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
        // 백엔드에서 메인 무기 데이터 가져오기
        LitJson.JsonData userMainWeaponData = BackendDataManager.Instance.GetUserMainWeaponData();

        // 데이터 확인용 상세 디버그 로그 추가
        Debug.Log($"[LevelUpSelectManager] MainWeapon Data Count: {userMainWeaponData?.Count ?? 0}");

        // 데이터 내용 상세 출력
        if (userMainWeaponData != null)
        {
            for (int i = 0; i < userMainWeaponData.Count; i++)
            {
                Debug.Log($"[LevelUpSelectManager] MainWeapon Data [{i}]: {userMainWeaponData[i].ToJson()}");
            }
        }
        else
        {
            Debug.LogError("[LevelUpSelectManager] userMainWeaponData is null!");
            return;
        }

        List<WeaponStatProvider.SubWeaponStat> subWeaponStats = WeaponStatProvider.Instance.subWeaponStats;

        PlayerAttack playerAttack = GameManager.Instance.playerAttack;
        Weapon.MainWeapon currentMainWeapon = playerAttack.mainWeapon;
        List<Weapon.SubWeapon> currentSubWeapons = playerAttack.subWeapons;

        // 이전 옵션 목록 초기화
        mainWeaponOptions.Clear();
        subWeaponOptions.Clear();

        int totalTryCount = 0;
        int successCount = 0;  // 성공적으로 생성된 아이템 카운트

        while (successCount < 3 && totalTryCount < MAX_TOTAL_RETRY_COUNT)
        {
            int itemIndex = UnityEngine.Random.Range(0, 3);
            SelectItemType selectItemType = (SelectItemType)itemIndex;
            bool succeeded = false;

            switch (selectItemType)
            {
                case SelectItemType.MainWeapon:
                    LitJson.JsonData userData = GetNonDuplicateMainWeapon(userMainWeaponData, currentMainWeapon);

                    Debug.Log($"[LevelUpSelectManager] 선택된 MainWeapon Data: {userData?.ToJson() ?? "null"}");

                    if (userData != null)
                    {
                        // UI 버튼에 메인 무기 정보 설정
                        mainWeaponOptions.Add(userData);

                        // 메인 무기 정보 디버그
                        Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(
                            typeof(Weapon.MainWeapon.WeaponType),
                            userData["weaponType"]["N"].ToString()
                        );
                        Debug.Log($"[LevelUpSelectManager] 선택된 무기 타입: {weaponType}");

                        levelUpUI.SetMainWeaponItem(userData, successCount);
                        succeeded = true;
                    }
                    break;

                case SelectItemType.SubWeapon:
                    // SubWeapon이 6개 이상인 경우에는 선택하지 않음
                    if (playerAttack.subWeapons.Count == 6)
                    {
                        totalTryCount++;
                        continue;
                    }
                    WeaponStatProvider.SubWeaponStat subWeaponStat = GetSubWeaponOption(subWeaponStats, currentSubWeapons);
                    if (!Equals(subWeaponStat, default))
                    {
                        // UI 버튼에 서브 무기 정보 설정
                        subWeaponOptions.Add(subWeaponStat);
                        levelUpUI.SetSubWeaponItem(subWeaponStat, successCount);
                        succeeded = true;
                    }
                    break;

                case SelectItemType.SupportItem:
                    // 현재 지원품 미구현으로 인해 건너뜀
                    totalTryCount++;
                    continue;
            }

            if (succeeded)
            {
                successCount++;
            }

            totalTryCount++;
        }

        // 만약 3개의 선택지를 채우지 못했다면 로그 출력
        if (successCount < 3)
        {
            Debug.LogWarning($"[LevelUpSelectManager] Only created {successCount} items after {totalTryCount} attempts");
        }
    }

    public void CreateMainWeaponItem(LitJson.JsonData userData)
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

    public void CreateSubWeaponItem(WeaponStatProvider.SubWeaponStat subWeaponStat)
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