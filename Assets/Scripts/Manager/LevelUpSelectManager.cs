using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LevelUpOptionType { SubWeapon, SupportItem }

public class LevelUpOption
{
    public LevelUpOptionType optionType;
    public WeaponStatProvider.SubWeaponStat subWeaponStat;
    public SupportItemSO supportItemSO;
    public int supportItemLevel;
}

public class LevelUpSelectManager : Singleton<LevelUpSelectManager>
{
    enum SelectItemType
    {
        // MainWeapon, // 메인 무기 타입 제외 (주석 처리)
        SubWeapon,
        SupportItem, // 아직 미구현이지만 확장성을 위해 유지
    }

    private const int MAX_RETRY_COUNT = 10;  // 최대 시도 횟수
    private const int MAX_TOTAL_RETRY_COUNT = 30;  // 전체 아이템 선택 최대 시도 횟수
    private const int MAX_SUB_WEAPONS = 6;   // 최대 보조 무기 수
    private const int DESIRED_OPTIONS_COUNT = 3; // 레벨업 시 표시할 옵션 개수

    [SerializeField] private LevelUpUI levelUpUI;
    // private List<LitJson.JsonData> mainWeaponOptions = new List<LitJson.JsonData>(); // 더 이상 메인 무기 옵션을 사용하지 않음
    private List<WeaponStatProvider.SubWeaponStat> subWeaponOptions = new List<WeaponStatProvider.SubWeaponStat>();

    // 캐싱용 딕셔너리 추가
    private Dictionary<string, Sprite> weaponSpriteCache = new Dictionary<string, Sprite>();
    const string SupportItemLocation = "ScriptableObjects/SupportItems";

    private void Start()
    {
        InitializeLevelUpUI();

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

    private void InitializeLevelUpUI()
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
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlayerLevelChanged -= OnPlayerLevelUp;
        }

        // 캐시 클리어
        weaponSpriteCache.Clear();
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

    private WeaponStatProvider.SubWeaponStat GetSubWeaponOption(List<WeaponStatProvider.SubWeaponStat> subWeaponStats, List<Weapon.SubWeapon> currentSubWeapons, List<WeaponStatProvider.SubWeaponStat> listedWeapons)
    {
        // 최대 보조 무기 수에 도달했는지 검사
        if (currentSubWeapons.Count >= MAX_SUB_WEAPONS)
        {
            // 이미 최대 개수에 도달한 경우, 기존 무기 업그레이드만 제공
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
            return default; // 모든 무기가 최대 레벨이거나 이미 나열되었다면 기본값 반환
        }

        // 현재 장착된 SubWeapon들 중 랜덤 선택 시도 (업그레이드)
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

    // 서브 무기 아이콘 로드 메서드 (캐싱 적용) - 개선된 버전
    public Sprite LoadSubWeaponSprite(WeaponStatProvider.SubWeaponStat subWeaponStat)
    {
        if (string.IsNullOrEmpty(subWeaponStat.displayName))
        {
            Debug.LogWarning("[LevelUpSelectManager] 서브 무기 데이터가 유효하지 않습니다.");
            return null;
        }

        string cacheKey = $"SubWeapon_{subWeaponStat.weaponType}_{subWeaponStat.weaponGrade}";

        // 캐시에 있으면 반환
        if (weaponSpriteCache.TryGetValue(cacheKey, out Sprite cachedSprite))
        {
            return cachedSprite;
        }

        Debug.Log($"[LevelUpSelectManager] 서브 무기 아이콘 로드 시도: {subWeaponStat.weaponType}, 등급: {subWeaponStat.weaponGrade}");

        // 1. SubWeapon 클래스와 동일한 경로 시도 (weaponType 기준)
        try
        {
            string path = $"ScriptableObjects/SubWeapon/{subWeaponStat.weaponType}/{subWeaponStat.weaponType}_{subWeaponStat.weaponGrade}";
            SubWeaponSO subWeaponSO = Resources.Load<SubWeaponSO>(path);
            if (subWeaponSO != null && subWeaponSO.weaponSprite != null)
            {
                // 캐시에 추가
                weaponSpriteCache[cacheKey] = subWeaponSO.weaponSprite;
                Debug.Log($"[LevelUpSelectManager] 서브 무기 아이콘 로드 성공 (weaponType 경로): {path}");
                return subWeaponSO.weaponSprite;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LevelUpSelectManager] weaponType 경로 로드 중 오류: {e.Message}");
        }

        // 2. displayName 기준 경로 시도
        try
        {
            string path = $"ScriptableObjects/SubWeapon/{subWeaponStat.displayName}/{subWeaponStat.displayName}_{subWeaponStat.weaponGrade}";
            SubWeaponSO subWeaponSO = Resources.Load<SubWeaponSO>(path);
            if (subWeaponSO != null && subWeaponSO.weaponSprite != null)
            {
                // 캐시에 추가
                weaponSpriteCache[cacheKey] = subWeaponSO.weaponSprite;
                Debug.Log($"[LevelUpSelectManager] 서브 무기 아이콘 로드 성공 (displayName 경로): {path}");
                return subWeaponSO.weaponSprite;
            }
        }
        catch (Exception) { }

        // 3. 상위 폴더에서 이름 검색
        try
        {
            // SubWeaponLoaderTest.cs와 유사한 방식으로 모든 SO 로드
            SubWeaponSO[] allSubWeapons = Resources.LoadAll<SubWeaponSO>("ScriptableObjects/SubWeapon");
            foreach (var weapon in allSubWeapons)
            {
                // weaponType과 weaponGrade가 일치하는 항목 찾기
                if (weapon.weaponType == subWeaponStat.weaponType && weapon.weaponGrade == subWeaponStat.weaponGrade && weapon.weaponSprite != null)
                {
                    weaponSpriteCache[cacheKey] = weapon.weaponSprite;
                    Debug.Log($"[LevelUpSelectManager] 서브 무기 아이콘 로드 성공 (LoadAll 검색): {weapon.name}");
                    return weapon.weaponSprite;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LevelUpSelectManager] LoadAll 검색 중 오류: {e.Message}");
        }

        // 4. 다양한 폴더 구조 시도
        string[] possiblePaths = new string[] {
        $"ScriptableObjects/SubWeapons/{subWeaponStat.weaponType}/{subWeaponStat.weaponType}_{subWeaponStat.weaponGrade}",
        $"ScriptableObjects/Weapons/SubWeapon/{subWeaponStat.weaponType}/{subWeaponStat.weaponType}_{subWeaponStat.weaponGrade}",
        $"SubWeapon/{subWeaponStat.weaponType}/{subWeaponStat.weaponType}_{subWeaponStat.weaponGrade}",
        $"Weapons/SubWeapon/{subWeaponStat.weaponType}/{subWeaponStat.weaponType}_{subWeaponStat.weaponGrade}"
    };

        foreach (string path in possiblePaths)
        {
            try
            {
                SubWeaponSO subWeaponSO = Resources.Load<SubWeaponSO>(path);
                if (subWeaponSO != null && subWeaponSO.weaponSprite != null)
                {
                    weaponSpriteCache[cacheKey] = subWeaponSO.weaponSprite;
                    Debug.Log($"[LevelUpSelectManager] 서브 무기 아이콘 로드 성공 (대체 경로): {path}");
                    return subWeaponSO.weaponSprite;
                }
            }
            catch (Exception) { }
        }

        // 5. 마지막 시도: 프리팹에서 아이콘 추출
        try
        {
            string prefabPath = $"Prefabs/Player/Weapon/SubWeapon/{subWeaponStat.weaponType}";
            GameObject weaponPrefab = Resources.Load<GameObject>(prefabPath);
            if (weaponPrefab != null)
            {
                // SpriteRenderer 또는 Image 컴포넌트에서 스프라이트 추출 시도
                SpriteRenderer spriteRenderer = weaponPrefab.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    weaponSpriteCache[cacheKey] = spriteRenderer.sprite;
                    Debug.Log($"[LevelUpSelectManager] 서브 무기 아이콘 로드 성공 (프리팹 SR): {prefabPath}");
                    return spriteRenderer.sprite;
                }

                // UI Image 컴포넌트 확인
                UnityEngine.UI.Image image = weaponPrefab.GetComponentInChildren<UnityEngine.UI.Image>();
                if (image != null && image.sprite != null)
                {
                    weaponSpriteCache[cacheKey] = image.sprite;
                    Debug.Log($"[LevelUpSelectManager] 서브 무기 아이콘 로드 성공 (프리팹 Image): {prefabPath}");
                    return image.sprite;
                }
            }
        }
        catch (Exception) { }

        // 6. 기본 아이콘 사용
        try
        {
            // 여러 가능한 기본 아이콘 경로 시도
            string[] defaultPaths = new string[] {
            "DefaultWeaponIcon",
            "Icons/DefaultWeaponIcon",
            "UI/DefaultWeaponIcon",
            "Sprites/DefaultWeaponIcon"
        };

            foreach (string path in defaultPaths)
            {
                Sprite defaultSprite = Resources.Load<Sprite>(path);
                if (defaultSprite != null)
                {
                    Debug.LogWarning($"[LevelUpSelectManager] 기본 아이콘 사용: {path}");
                    return defaultSprite;
                }
            }
        }
        catch (Exception) { }

        Debug.LogError($"[LevelUpSelectManager] 서브 무기 아이콘 로드 실패: {subWeaponStat.weaponType}, 등급: {subWeaponStat.weaponGrade}");

        // 아무 이미지도 찾지 못한 경우 null 반환 (UI에서 아이콘 없이 표시됨)
        return null;
    }

    public void CreateItemList()
    {
        Debug.Log("[LevelUpSelectManager] CreateItemList started");

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

        if (levelUpUI == null)
        {
            InitializeLevelUpUI();
            if (levelUpUI == null)
            {
                Debug.LogError("[LevelUpSelectManager] Could not find LevelUpUI in scene!");
                return;
            }
        }

        levelUpUI.ShowLevelUpPanel();

        PlayerAttack playerAttack = GameManager.Instance.playerAttack;
        if (playerAttack == null)
        {
            Debug.LogError("[LevelUpSelectManager] PlayerAttack is null!");
            return;
        }

        if (WeaponStatProvider.Instance == null)
        {
            Debug.LogError("[LevelUpSelectManager] WeaponStatProvider.Instance is null!");
            return;
        }

        List<LevelUpOption> candidates = new List<LevelUpOption>();

        // 1. 서브웨폰 후보 추가
        var subWeaponStats = WeaponStatProvider.Instance.subWeaponStats;
        var currentSubWeapons = playerAttack.subWeapons;
        foreach (var stat in subWeaponStats)
        {
            // 신규 무기
            if (stat.weaponGrade == 0 && !currentSubWeapons.Any(c => c.weaponType == stat.weaponType))
            {
                candidates.Add(new LevelUpOption
                {
                    optionType = LevelUpOptionType.SubWeapon,
                    subWeaponStat = stat
                });
            }
            // 업그레이드 후보
            var owned = currentSubWeapons.FirstOrDefault(sw => sw.weaponType == stat.weaponType);
            if (owned != null && owned.weaponGrade < owned.maxWeaponGrade && stat.weaponGrade == owned.weaponGrade + 1)
            {
                candidates.Add(new LevelUpOption
                {
                    optionType = LevelUpOptionType.SubWeapon,
                    subWeaponStat = stat
                });
            }
        }

        // 2. 서포트아이템 후보 추가
        SupportItemSO[] allSupportItems = Resources.LoadAll<SupportItemSO>(SupportItemLocation);
        if (allSupportItems == null || allSupportItems.Length == 0)
        {
            Debug.LogError($"[LevelUpSelectManager] 지원아이템을 찾을 수 없습니다! 경로: {SupportItemLocation}");
        }
        Debug.Log($"[LevelUpSelectManager] SupportItemSO LoadAll 경로 '{SupportItemLocation}'에서 {allSupportItems.Length}개 발견.");
        foreach (var supportItem in allSupportItems)
        {
            int currentLevel = 0;
            if (GameManager.Instance != null)
            {
                currentLevel = GameManager.Instance.GetSupportItemLevel(supportItem);
                Debug.Log($"[LevelUpSelectManager] 지원아이템: {supportItem.name}, 현재레벨: {currentLevel}, 최대레벨: {supportItem.maxLevel}");
            }
            else
            {
                Debug.LogWarning("[LevelUpSelectManager] GameManager.Instance가 null입니다! 지원아이템 레벨을 0으로 처리.");
            }

            if (currentLevel < supportItem.maxLevel)
            {
                Debug.Log($"[LevelUpSelectManager] 지원아이템 '{supportItem.name}' 후보에 추가 (현재레벨: {currentLevel}, 최대레벨: {supportItem.maxLevel})");
                candidates.Add(new LevelUpOption
                {
                    optionType = LevelUpOptionType.SupportItem,
                    supportItemSO = supportItem,
                    supportItemLevel = currentLevel
                });
            }
            else
            {
                Debug.Log($"[LevelUpSelectManager] 지원아이템 '{supportItem.name}' 후보에서 제외 (현재레벨: {currentLevel}, 최대레벨: {supportItem.maxLevel})");
            }
        }

        // 3. 랜덤 섞기
        System.Random rnd = new System.Random();
        candidates = candidates.OrderBy(x => rnd.Next()).ToList();

        // 4. UI에 표시
        int count = Mathf.Min(DESIRED_OPTIONS_COUNT, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            var option = candidates[i];
            if (option.optionType == LevelUpOptionType.SubWeapon)
            {
                var icon = LoadSubWeaponSprite(option.subWeaponStat);
                levelUpUI.SetSubWeaponItem(option.subWeaponStat, i, option.subWeaponStat.displayName, icon);
            }
            else if (option.optionType == LevelUpOptionType.SupportItem)
            {
                levelUpUI.SetSupportItem(option.supportItemSO, i, option.supportItemSO.itemName, option.supportItemSO.icon, option.supportItemLevel);
            }
        }

        // 나머지 버튼 비활성화
        for (int i = count; i < DESIRED_OPTIONS_COUNT; i++)
        {
            if (i < levelUpUI.optionButtons.Count)
                levelUpUI.optionButtons[i].gameObject.SetActive(false);
        }
    }

    // 서브 무기 아이템 생성 메서드 (LevelUpUI에서 호출됨)
    public void CreateSubWeaponItem(WeaponStatProvider.SubWeaponStat subWeaponStat)
    {
        PlayerAttack playerAttack = GameManager.Instance.playerAttack;
        if (playerAttack == null)
        {
            Debug.LogError("[LevelUpSelectManager] PlayerAttack is null!");
            return;
        }

        try
        {
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
        catch (Exception e)
        {
            Debug.LogError($"[LevelUpSelectManager] Error creating/upgrading sub weapon: {e.Message}");
        }
    }

    public void CreateSupportItem(SupportItemSO supportItem)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelUpSupportItem(supportItem);
            Debug.Log($"[LevelUpSelectManager] Support item added/upgraded: {supportItem.itemName}");
        }
        else
        {
            Debug.LogError("[LevelUpSelectManager] GameManager.Instance is null! Support item not added.");
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