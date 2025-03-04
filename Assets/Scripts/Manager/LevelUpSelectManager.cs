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
            Debug.Log("[LevelUpSelectManager] LevelUpUI 참조를 찾는 중...");
            levelUpUI = FindAnyObjectByType<LevelUpUI>();
            if (levelUpUI == null)
            {
                Debug.LogError("[LevelUpSelectManager] LevelUpUI 컴포넌트를 씬에서 찾을 수 없습니다!");
                return;
            }
            Debug.Log("[LevelUpSelectManager] LevelUpUI 참조 찾음");
        }

        // 레벨업 이벤트에 구독
        if (GameManager.Instance != null)
        {
            Debug.Log("[LevelUpSelectManager] GameManager.Instance 이벤트에 구독합니다.");
            GameManager.Instance.onPlayerLevelChanged += OnPlayerLevelUp;
        }
        else
        {
            Debug.LogError("[LevelUpSelectManager] GameManager.Instance가 null입니다!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlayerLevelChanged -= OnPlayerLevelUp;
        }
    }

    // 레벨업 이벤트 처리
    private void OnPlayerLevelUp()
    {
        // 레벨업 옵션 생성
        Debug.Log("[LevelUpSelectManager] OnPlayerLevelUp 이벤트 발생! 레벨업 옵션을 생성합니다.");

        // 현재 활성화된 패널이 있는지 확인
        if (levelUpUI != null && levelUpUI.gameObject.activeInHierarchy)
        {
            // 이미 표시된 패널이 있는지 확인
            Transform panelTransform = levelUpUI.transform.Find("LevelUpPanel");
            if (panelTransform != null && panelTransform.gameObject.activeSelf)
            {
                Debug.Log("[LevelUpSelectManager] 레벨업 패널이 이미 활성화되어 있습니다. 새 패널을 표시하지 않습니다.");
                return;
            }
        }

        // 아이템 목록 생성
        CreateItemList();
    }

    private LitJson.JsonData GetNonDuplicateMainWeapon(LitJson.JsonData userMainWeaponData, Weapon.MainWeapon currentMainWeapon, List<LitJson.JsonData> listedWeapons)
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

            Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(typeof(Weapon.MainWeapon.WeaponType), userData["weaponType"].ToString());
            if (listedWeapons.Any(x => x["weaponType"].ToString() == weaponType.ToString()))
            {
                tryCount++;
                continue;
            }
            // 현재 장착된 무기와 다른 타입인 경우에만 반환
            if (currentMainWeapon == null || weaponType != currentMainWeapon.weaponType)
            {
                return userData;
            }

            tryCount++;
        }

        return null;  // 유효한 무기를 찾지 못한 경우
    }

    private WeaponStatProvider.SubWeaponStat GetSubWeaponOption(List<WeaponStatProvider.SubWeaponStat> subWeaponStats, List<Weapon.SubWeapon> currentSubWeapons, List<WeaponStatProvider.SubWeaponStat> listedWeapons)
    {
        // 현재 장착된 SubWeapon들 중 랜덤 선택 시도
        if (currentSubWeapons.Count > 0)
        {
            int tryCount = 0;
            while (tryCount < MAX_RETRY_COUNT)
            {
                int weaponIndex = UnityEngine.Random.Range(0, currentSubWeapons.Count);
                Weapon.SubWeapon currentWeapon = currentSubWeapons[weaponIndex];
                if (currentWeapon == null)
                {
                    tryCount++;
                    continue;
                }
                if (listedWeapons.Any(x => x.weaponType == currentWeapon.weaponType))
                {
                    tryCount++;
                    continue;
                }
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
            !currentSubWeapons.Any(c => c.weaponType == x.weaponType) &&
            !listedWeapons.Any(l => l.weaponType == x.weaponType)
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
        Debug.Log("[LevelUpSelectManager] CreateItemList started");

        // 모든 필수 인스턴스 null 체크
        if (BackendDataManager.Instance == null)
        {
            Debug.LogError("[LevelUpSelectManager] BackendDataManager.Instance is null!");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[LevelUpSelectManager] GameManager.Instance is null!");
            return;
        }

        // 중요: 여기서 PlayerAttack null 체크 전에 레벨업 패널을 먼저 표시
        if (levelUpUI == null)
        {
            Debug.LogError("[LevelUpSelectManager] levelUpUI is null!");
            // UI 찾기 시도
            levelUpUI = FindAnyObjectByType<LevelUpUI>();
            if (levelUpUI == null)
            {
                Debug.LogError("[LevelUpSelectManager] Could not find LevelUpUI in scene!");
                return;
            }
        }

        // 레벨업 패널 표시 명령 직접 호출
        if (levelUpUI != null)
        {
            Debug.Log("[LevelUpSelectManager] 레벨업 UI 표시 명령 실행");
            // 레벨업 패널 표시 메서드를 직접 호출
            levelUpUI.ShowLevelUpPanel();
        }

        // PlayerAttack은 아이템 생성에만 필요하므로 여기서 null 체크
        PlayerAttack playerAttack = GameManager.Instance.playerAttack;
        if (playerAttack == null)
        {
            Debug.LogError("[LevelUpSelectManager] PlayerAttack is null!");
            // PlayerAttack이 null이면 아이템 생성은 건너뛰지만, 패널은 이미 표시됨
            return;
        }

        if (WeaponStatProvider.Instance == null)
        {
            Debug.LogError("[LevelUpSelectManager] WeaponStatProvider.Instance is null!");
            return;
        }

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

        Weapon.MainWeapon currentMainWeapon = playerAttack.mainWeapon;
        List<Weapon.SubWeapon> currentSubWeapons = playerAttack.subWeapons;

        List<LitJson.JsonData> listedMainWeapons = new List<LitJson.JsonData>();
        List<WeaponStatProvider.SubWeaponStat> listedSubWeapons = new List<WeaponStatProvider.SubWeaponStat>();
        // 지원품 구현 후 listedDrops 추가 필요

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
                    LitJson.JsonData userData = GetNonDuplicateMainWeapon(userMainWeaponData, currentMainWeapon, listedMainWeapons);

                    Debug.Log($"[LevelUpSelectManager] 선택된 MainWeapon Data: {userData?.ToJson() ?? "null"}");

                    if (userData != null)
                    {
                        // UI 버튼에 메인 무기 정보 설정
                        mainWeaponOptions.Add(userData);

                        // 메인 무기 정보 디버그
                        Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(
                            typeof(Weapon.MainWeapon.WeaponType),
                            userData["weaponType"].ToString()
                        );

                        // 메인 무기 표시명 가져오기
                        string displayName = WeaponStatProvider.Instance.weaponStats.Find(x => x.weaponType == weaponType).displayName;
                        Debug.Log($"[LevelUpSelectManager] 선택된 무기 타입: {weaponType}, 표시명: {displayName}");

                        // 메인 무기 아이콘 가져오기
                        Sprite weaponIcon = null;
                        GameObject weaponPrefab = Resources.Load<GameObject>($"Prefabs/Player/Weapon/MainWeapon/{weaponType}");
                        if (weaponPrefab != null && weaponPrefab.GetComponent<SpriteRenderer>() != null)
                        {
                            weaponIcon = weaponPrefab.GetComponent<SpriteRenderer>().sprite;
                            Debug.Log($"[LevelUpSelectManager] 메인 무기 아이콘 로드 성공: {weaponType}");
                        }
                        else
                        {
                            Debug.LogWarning($"[LevelUpSelectManager] 메인 무기 아이콘 로드 실패: {weaponType}");
                        }

                        levelUpUI.SetMainWeaponItem(userData, successCount, displayName, weaponIcon);
                        listedMainWeapons.Add(userData);
                        succeeded = true;
                    }
                    break;

                case SelectItemType.SubWeapon:
                    // SubWeapon이 6개 이상인 경우에는 선택하지 않음
                    if (playerAttack.subWeapons.Count >= 6)
                    {
                        totalTryCount++;
                        continue;
                    }
                    WeaponStatProvider.SubWeaponStat subWeaponStat = GetSubWeaponOption(subWeaponStats, currentSubWeapons, listedSubWeapons);
                    if (!Equals(subWeaponStat, default))
                    {
                        // 서브 무기 표시명 가져오기
                        string displayName = subWeaponStat.displayName;
                        Debug.Log($"[LevelUpSelectManager] 선택된 서브 무기: {subWeaponStat.weaponType}, 표시명: {displayName}, 등급: {subWeaponStat.weaponGrade}");

                        // 서브 무기 아이콘 가져오기
                        Sprite weaponIcon = null;
                        SubWeaponSO subWeaponSO = Resources.Load<SubWeaponSO>($"ScriptableObjects/SubWeapon/{displayName}/{displayName}_{subWeaponStat.weaponGrade}");
                        if (subWeaponSO != null)
                        {
                            weaponIcon = subWeaponSO.weaponSprite;
                            Debug.Log($"[LevelUpSelectManager] 서브 무기 아이콘 로드 성공: {displayName}_{subWeaponStat.weaponGrade}");
                        }
                        else
                        {
                            Debug.LogWarning($"[LevelUpSelectManager] 서브 무기 아이콘 로드 실패: {displayName}_{subWeaponStat.weaponGrade}");
                        }

                        // UI 버튼에 서브 무기 정보 설정
                        subWeaponOptions.Add(subWeaponStat);
                        levelUpUI.SetSubWeaponItem(subWeaponStat, successCount, displayName, weaponIcon);
                        listedSubWeapons.Add(subWeaponStat);
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

        Debug.Log($"[LevelUpSelectManager] Total try count: {totalTryCount}, Success count: {successCount}");

        // 만약 3개의 선택지를 채우지 못했다면 로그 출력
        if (successCount < 3)
        {
            Debug.LogError($"[LevelUpSelectManager] Failed to create full item list. Only {successCount} items created.");
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
        Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(typeof(Weapon.MainWeapon.WeaponType), userData["weaponType"].ToString());
        string weaponTypeString = weaponType.ToString();
        Weapon.WeaponRare weaponRare = (Weapon.WeaponRare)Enum.Parse(typeof(Weapon.WeaponRare), userData["weaponRare"].ToString());
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