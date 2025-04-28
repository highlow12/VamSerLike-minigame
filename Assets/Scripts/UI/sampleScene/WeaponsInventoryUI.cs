using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponsInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform weaponsContainer; // 무기 아이콘들이 표시될 컨테이너
    [SerializeField] private GameObject weaponSlotPrefab; // 무기 슬롯 프리팹 (ItemSlot > Bg, Item, Text)

    [Header("Settings")]
    [SerializeField] private int maxSlots = 6; // 최대 표시할 무기 슬롯 수
    [SerializeField] private int maxSelectionCount = 5; // 무기 선택 최대 카운트

    // 내부 사용 변수
    private PlayerAttack playerAttack;
    private LevelUpUI levelUpUI;
    private List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
    private Dictionary<string, int> weaponSelectionCount = new Dictionary<string, int>(); // 무기별 선택 횟수 (세션 내 유지 저장)

    // 스프라이트 캐싱용 딕셔너리 추가
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    // 무기 슬롯 정보 관리 클래스
    [System.Serializable]
    private class WeaponSlot
    {
        public GameObject slotObject;
        public Image bgImage;
        public Image itemImage;
        public TextMeshProUGUI countText;
        public string weaponId; // 무기 식별자 (타입 + 등급)
        public bool isMainWeapon;
    }

    private void Awake()
    {
        // 무기 슬롯 초기화
        InitializeWeaponSlots();

        // 세션 시작 시 무기 선택 카운트 초기화
        weaponSelectionCount.Clear();
    }

    private void Start()
    {
        // 플레이어 어택 참조 가져오기
        if (GameManager.Instance != null && GameManager.Instance.playerAttack != null)
        {
            playerAttack = GameManager.Instance.playerAttack;
            Debug.Log("[WeaponsInventoryUI] PlayerAttack 참조를 찾았습니다.");
        }
        else
        {
            Debug.LogError("[WeaponsInventoryUI] PlayerAttack 참조를 찾을 수 없습니다!");
            return;
        }

        // LevelUpUI 찾기
        levelUpUI = FindObjectOfType<LevelUpUI>();
        if (levelUpUI != null)
        {
            // 패널 닫힘 이벤트 구독
            levelUpUI.onPanelClosed += OnLevelUpPanelClosed;
            Debug.Log("[WeaponsInventoryUI] LevelUpUI 이벤트에 구독했습니다.");
        }
        else
        {
            Debug.LogWarning("[WeaponsInventoryUI] LevelUpUI 컴포넌트를 씬에서 찾을 수 없습니다!");
        }

        // 무기 상태 초기 업데이트
        UpdateWeaponsDisplay();

        // 진단 도구 실행
        DiagnoseWeaponResources();

        // 무기 레벨 정보 출력
        PrintWeaponLevels();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (levelUpUI != null)
        {
            levelUpUI.onPanelClosed -= OnLevelUpPanelClosed;
        }

        // 캐시 클리어
        spriteCache.Clear();
        weaponSelectionCount.Clear();
    }

    // 레벨업 패널이 닫힐 때 호출되는 이벤트 핸들러
    private void OnLevelUpPanelClosed()
    {
        Debug.Log("[WeaponsInventoryUI] 레벨업 패널이 닫혔습니다. 무기 정보 업데이트");

        // 이전 무기 상태와 비교하여 새로 추가/업그레이드된 무기 카운트 증가
        UpdateWeaponCounts();

        // 무기 UI 업데이트
        UpdateWeaponsDisplay();

        // 무기 레벨 정보 출력 (디버깅용)
        PrintWeaponLevels();
    }

    private void InitializeWeaponSlots()
    {
        if (weaponsContainer == null || weaponSlotPrefab == null)
        {
            Debug.LogError("[WeaponsInventoryUI] 무기 컨테이너 또는 슬롯 프리팹이 할당되지 않았습니다!");
            return;
        }

        // 기존 슬롯 제거
        foreach (Transform child in weaponsContainer)
        {
            Destroy(child.gameObject);
        }
        weaponSlots.Clear();

        // 슬롯 생성
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObject = Instantiate(weaponSlotPrefab, weaponsContainer);
            slotObject.name = $"WeaponSlot_{i}";

            // 슬롯 컴포넌트 가져오기
            Image bgImage = null;
            Image itemImage = null;
            TextMeshProUGUI countText = null;

            // Bg 찾기
            Transform bgTransform = slotObject.transform.Find("Bg");
            if (bgTransform != null)
            {
                bgImage = bgTransform.GetComponent<Image>();
            }

            // Item 찾기
            Transform itemTransform = slotObject.transform.Find("Item");
            if (itemTransform != null)
            {
                itemImage = itemTransform.GetComponent<Image>();
            }

            // Text 찾기
            Transform textTransform = slotObject.transform.Find("Text (TMP)"); // 정확한 이름으로 변경
            if (textTransform == null)
            {
                textTransform = slotObject.transform.Find("Text"); // 대체 이름 시도
            }

            if (textTransform != null)
            {
                countText = textTransform.GetComponent<TextMeshProUGUI>();
            }

            // 컴포넌트 검증
            if (bgImage == null)
            {
                Debug.LogError($"[WeaponsInventoryUI] 슬롯 {i}의 Bg 이미지를 찾을 수 없습니다!");
                continue;
            }

            if (itemImage == null)
            {
                Debug.LogError($"[WeaponsInventoryUI] 슬롯 {i}의 Item 이미지를 찾을 수 없습니다!");
                continue;
            }

            if (countText == null)
            {
                Debug.LogError($"[WeaponsInventoryUI] 슬롯 {i}의 Text를 찾을 수 없습니다!");
                continue;
            }

            // 모든 컴포넌트가 유효한 경우에만 슬롯 추가
            // 기본 상태 설정
            itemImage.sprite = null;
            itemImage.gameObject.SetActive(false); // 비활성화로 변경
            countText.gameObject.SetActive(false); // 비활성화로 변경

            // 슬롯 정보 저장
            WeaponSlot slot = new WeaponSlot
            {
                slotObject = slotObject,
                bgImage = bgImage,
                itemImage = itemImage,
                countText = countText,
                weaponId = "",
                isMainWeapon = false
            };

            weaponSlots.Add(slot);
            slotObject.SetActive(true); // 모든 슬롯을 활성화 상태로 유지

            Debug.Log($"[WeaponsInventoryUI] 슬롯 {i} 초기화 성공");
        }

        Debug.Log($"[WeaponsInventoryUI] 총 {weaponSlots.Count}개의 무기 슬롯 초기화됨");
    }

    // 무기 카운트 업데이트 - 현재 선택된 무기의 카운트만 증가시킴
    private void UpdateWeaponCounts()
    {
        if (playerAttack == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerAttack != null)
            {
                playerAttack = GameManager.Instance.playerAttack;
            }
            else
            {
                Debug.LogError("[WeaponsInventoryUI] PlayerAttack 참조를 찾을 수 없습니다!");
                return;
            }
        }

        // 현재 선택된 무기 가져오기 (LevelUpUI에서 선택한 무기)
        Weapon.SubWeapon selectedWeapon = GetLastSelectedWeapon();

        if (selectedWeapon != null)
        {
            string weaponType = selectedWeapon.weaponType.ToString();
            // 무기 타입만으로 ID 생성 (등급 제외) - 같은 타입의 무기는 등급과 상관없이 같은 카운트 공유
            string weaponId = $"Sub_{weaponType}";

            IncrementWeaponSelectionCount(weaponId);
            Debug.Log($"[WeaponsInventoryUI] 선택된 서브무기 {weaponId}의 선택 횟수를 업데이트했습니다.");
        }
        else
        {
            Debug.LogWarning("[WeaponsInventoryUI] 선택된 무기를 찾을 수 없습니다.");
        }
    }

    // 마지막으로 선택된 서브무기 찾기 (임시 구현 - 실제 게임 시스템에 맞게 수정 필요)
    private Weapon.SubWeapon GetLastSelectedWeapon()
    {
        // 임시 구현: 실제로는 LevelUpUI나 다른 시스템에서 방금 선택한 무기 정보를 가져와야 함
        // 여기서는 가장 최근에 추가된 무기를 반환 (예시)
        if (playerAttack != null && playerAttack.subWeapons.Count > 0)
        {
            return playerAttack.subWeapons[playerAttack.subWeapons.Count - 1];
        }

        return null;
    }

    // 서브 무기 스프라이트 로드 - 수정된 버전
    private Sprite LoadSubWeaponSprite(string displayName, int weaponGrade)
    {
        // 캐시 키 생성
        string cacheKey = $"SubWeapon_{displayName}_{weaponGrade}";

        // 로그 출력 - 디버깅용
        Debug.Log($"[WeaponsInventoryUI] 서브 무기 스프라이트 로드 시도: {displayName}_{weaponGrade}, 캐시 키: {cacheKey}");

        // 캐시에 있으면 반환
        if (spriteCache.TryGetValue(cacheKey, out Sprite cachedSprite))
        {
            Debug.Log($"[WeaponsInventoryUI] 캐시에서 서브 무기 스프라이트 로드 성공: {displayName}_{weaponGrade}");
            return cachedSprite;
        }

        Sprite weaponSprite = null;

        try
        {
            // 1. ScriptableObject에서 스프라이트 로드 시도
            SubWeaponSO subWeaponSO = Resources.Load<SubWeaponSO>($"ScriptableObjects/SubWeapon/{displayName}/{displayName}_{weaponGrade}");
            if (subWeaponSO != null && subWeaponSO.weaponSprite != null)
            {
                weaponSprite = subWeaponSO.weaponSprite;
                Debug.Log($"[WeaponsInventoryUI] SO에서 서브 무기 스프라이트 로드 성공: {displayName}_{weaponGrade}");
                spriteCache[cacheKey] = weaponSprite;
                return weaponSprite;
            }
            else
            {
                Debug.Log($"[WeaponsInventoryUI] SO에서 스프라이트 로드 실패: ScriptableObjects/SubWeapon/{displayName}/{displayName}_{weaponGrade}");
            }

            // 2. 일반 스프라이트 경로 시도
            string[] pathsToTry = new string[]
            {
                $"Sprites/SubWeapons/{displayName}_{weaponGrade}",
                $"Sprites/Weapons/{displayName}_{weaponGrade}",
                $"Sprites/UI/SubWeapons/{displayName}_{weaponGrade}",
                $"Sprites/Items/{displayName}_{weaponGrade}",
                $"SubWeapons/{displayName}_{weaponGrade}",
                $"Weapons/{displayName}_{weaponGrade}",
                
                // 공백이 문제일 수 있으므로 공백 제거 버전도 시도
                $"Sprites/SubWeapons/{displayName.Replace(" ", "")}_{weaponGrade}",
                $"Sprites/Weapons/{displayName.Replace(" ", "")}_{weaponGrade}",
                
                // 무기 이름만 시도 (등급 없이)
                $"Sprites/SubWeapons/{displayName}",
                $"Sprites/Weapons/{displayName}",
                $"SubWeapons/{displayName}",
                $"Weapons/{displayName}"
            };

            foreach (string path in pathsToTry)
            {
                weaponSprite = Resources.Load<Sprite>(path);
                if (weaponSprite != null)
                {
                    Debug.Log($"[WeaponsInventoryUI] 경로 '{path}'에서 서브 무기 스프라이트 로드 성공");
                    spriteCache[cacheKey] = weaponSprite;
                    return weaponSprite;
                }
                else
                {
                    Debug.Log($"[WeaponsInventoryUI] 경로 '{path}'에서 스프라이트 로드 실패");
                }
            }

            // 3. 기본 아이콘 시도
            weaponSprite = Resources.Load<Sprite>("Sprites/UI/DefaultWeaponIcon");
            if (weaponSprite != null)
            {
                Debug.Log($"[WeaponsInventoryUI] 기본 아이콘을 사용합니다: {displayName}_{weaponGrade}");
                spriteCache[cacheKey] = weaponSprite;
                return weaponSprite;
            }

            Debug.LogError($"[WeaponsInventoryUI] 모든 방법으로 서브 무기 스프라이트 로드 실패: {displayName}_{weaponGrade}");

            // 4. Resources 폴더 탐색
            LogResourcesPaths();
        }
        catch (Exception e)
        {
            Debug.LogError($"[WeaponsInventoryUI] 서브 무기 스프라이트 로드 중 오류: {e.Message}\n{e.StackTrace}");
        }

        // 마지막 수단: 런타임에 기본 스프라이트 생성
        Texture2D texture = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = new Color(0.8f, 0.3f, 0.3f, 1.0f); // 빨간색 플레이스홀더
        texture.SetPixels(colors);
        texture.Apply();
        Sprite fallbackSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
        spriteCache[cacheKey] = fallbackSprite;
        return fallbackSprite;
    }

    // Resources 폴더의 경로를 로깅하는 도우미 메서드
    private void LogResourcesPaths()
    {
        Debug.Log("[WeaponsInventoryUI] Resources 디렉토리를 검색합니다...");
        string resourcesPath = System.IO.Path.Combine(Application.dataPath, "Resources");
        if (System.IO.Directory.Exists(resourcesPath))
        {
            Debug.Log($"[WeaponsInventoryUI] Resources 디렉토리 경로: {resourcesPath}");

            // 모든 디렉토리 로깅 (최대 2단계 깊이)
            try
            {
                Debug.Log("[WeaponsInventoryUI] Resources 디렉토리 구조:");
                foreach (string dir in System.IO.Directory.GetDirectories(resourcesPath))
                {
                    string dirName = System.IO.Path.GetFileName(dir);
                    Debug.Log($" - {dirName}/");

                    foreach (string subDir in System.IO.Directory.GetDirectories(dir))
                    {
                        string subDirName = System.IO.Path.GetFileName(subDir);
                        Debug.Log($"   - {dirName}/{subDirName}/");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WeaponsInventoryUI] 디렉토리 검색 중 오류: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[WeaponsInventoryUI] Resources 디렉토리를 찾을 수 없습니다");
        }
    }

    // 무기 리소스 진단 도구
    public void DiagnoseWeaponResources()
    {
        Debug.Log("=== 무기 리소스 진단 시작 ===");

        // Resources 디렉토리가 존재하는지 확인
        string resourcesPath = System.IO.Path.Combine(Application.dataPath, "Resources");
        bool resourcesExists = System.IO.Directory.Exists(resourcesPath);
        Debug.Log($"Resources 디렉토리 존재 여부: {resourcesExists}");

        if (resourcesExists)
        {
            try
            {
                // 모든 SubWeaponSO 리소스 로그
                SubWeaponSO[] soAssets = Resources.LoadAll<SubWeaponSO>("");
                Debug.Log($"발견된 SubWeaponSO 리소스: {soAssets.Length}개");
                foreach (SubWeaponSO asset in soAssets)
                    Debug.Log($"  - {asset.name} (타입: {asset.weaponType}, 등급: {asset.weaponGrade}, 스프라이트: {(asset.weaponSprite != null ? "있음" : "없음")})");

                // Enum에 있는 무기 타입 로그
                Debug.Log("Enum에 있는 무기 타입:");
                foreach (Weapon.SubWeapon.WeaponType weaponType in Enum.GetValues(typeof(Weapon.SubWeapon.WeaponType)))
                    Debug.Log($"  - {weaponType}");

                // 각 무기 타입별로 스프라이트 로드 시도
                Debug.Log("각 무기 타입별 스프라이트 로드 시도:");
                foreach (Weapon.SubWeapon.WeaponType weaponType in Enum.GetValues(typeof(Weapon.SubWeapon.WeaponType)))
                {
                    string weaponTypeName = weaponType.ToString();
                    for (int grade = 1; grade <= 5; grade++)
                    {
                        Sprite sprite = LoadSubWeaponSprite(weaponTypeName, grade);
                        Debug.Log($"  - {weaponTypeName} (등급 {grade}): {(sprite != null ? "성공" : "실패")}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"진단 중 오류 발생: {e.Message}\n{e.StackTrace}");
            }
        }

        Debug.Log("=== 진단 완료 ===");
    }

    public void UpdateWeaponsDisplay()
    {
        Debug.Log("[WeaponsInventoryUI] 무기 디스플레이 업데이트 중...");

        if (playerAttack == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerAttack != null)
            {
                playerAttack = GameManager.Instance.playerAttack;
            }
            else
            {
                Debug.LogError("[WeaponsInventoryUI] PlayerAttack 참조를 찾을 수 없습니다!");
                return;
            }
        }

        // 모든 슬롯을 빈 상태로 초기화
        foreach (WeaponSlot slot in weaponSlots)
        {
            slot.itemImage.sprite = null;
            slot.itemImage.gameObject.SetActive(false); // 비활성화
            slot.countText.gameObject.SetActive(false); // 비활성화
            slot.weaponId = "";
            slot.isMainWeapon = false;

            // 슬롯 자체는 계속 활성화 상태로 유지
            slot.slotObject.SetActive(true);
        }

        int slotIndex = 0;

        // 메인 무기는 표시하지 않음 (서브 무기만 표시하도록 변경)

        // 서브 무기 표시 - 무기 타입별로 분류
        // 먼저 무기 타입별로 가장 높은 등급의 무기만 표시하도록 정리
        var highestGradeWeapons = new Dictionary<string, Weapon.SubWeapon>();

        foreach (Weapon.SubWeapon subWeapon in playerAttack.subWeapons)
        {
            string weaponType = subWeapon.weaponType.ToString();

            // 이미 이 무기 타입이 있는지 확인하고, 더 높은 등급으로 업데이트
            if (highestGradeWeapons.ContainsKey(weaponType))
            {
                if (subWeapon.weaponGrade > highestGradeWeapons[weaponType].weaponGrade)
                {
                    highestGradeWeapons[weaponType] = subWeapon;
                }
            }
            else
            {
                highestGradeWeapons[weaponType] = subWeapon;
            }
        }

        // 이제 정리된 무기 목록으로 UI 업데이트
        foreach (var weaponEntry in highestGradeWeapons)
        {
            if (slotIndex < weaponSlots.Count)
            {
                WeaponSlot slot = weaponSlots[slotIndex];
                Weapon.SubWeapon subWeapon = weaponEntry.Value;
                string weaponType = subWeapon.weaponType.ToString();
                string displayName = GetSubWeaponDisplayName(subWeapon.weaponType);

                try
                {
                    // 서브 무기 스프라이트 로드
                    Sprite weaponSprite = LoadSubWeaponSprite(displayName, subWeapon.weaponGrade);

                    // 스프라이트 적용
                    if (weaponSprite != null)
                    {
                        slot.itemImage.sprite = weaponSprite;
                        slot.itemImage.gameObject.SetActive(true); // 활성화로 변경

                        // 무기 타입만으로 ID 생성 (같은 타입은 레벨 공유)
                        string weaponId = $"Sub_{weaponType}";
                        slot.weaponId = weaponId;
                        slot.isMainWeapon = false;

                        // 선택 횟수(레벨) 표시
                        int count = GetWeaponSelectionCount(weaponId);
                        Debug.Log($"[WeaponsInventoryUI] 서브무기 {weaponId}의 선택 횟수(레벨): {count}");

                        if (count > 0)
                        {
                            slot.countText.text = count.ToString();
                            slot.countText.gameObject.SetActive(true); // 활성화로 변경
                            Debug.Log($"[WeaponsInventoryUI] 서브무기 {weaponId}의 레벨 {count}를 표시합니다.");
                        }
                        else
                        {
                            slot.countText.gameObject.SetActive(false);
                            Debug.Log($"[WeaponsInventoryUI] 서브무기 {weaponId}의 레벨이 0이므로 텍스트를 표시하지 않습니다.");
                        }
                    }
                    else
                    {
                        // 스프라이트가 없어도 선택 횟수는 표시
                        string weaponId = $"Sub_{weaponType}";
                        int count = GetWeaponSelectionCount(weaponId);
                        Debug.Log($"[WeaponsInventoryUI] 스프라이트 없음, 서브무기 {weaponId}의 레벨: {count}");

                        if (count > 0)
                        {
                            // 스프라이트가 없더라도 텍스트는 표시 (임시로 빈 이미지 표시)
                            slot.itemImage.gameObject.SetActive(true); // 투명 이미지로 활성화
                            slot.countText.text = count.ToString();
                            slot.countText.gameObject.SetActive(true);
                            Debug.Log($"[WeaponsInventoryUI] 서브무기 {weaponId} 스프라이트 없음, 레벨만 표시: {count}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[WeaponsInventoryUI] 서브 무기 표시 중 오류: {e.Message}\n{e.StackTrace}");
                }

                slotIndex++;
            }
        }

        // 디버그 정보 출력
        DebugWeaponSelections();
        Debug.Log($"[WeaponsInventoryUI] {slotIndex}개의 무기 슬롯이 업데이트되었습니다.");
    }

    // 특정 무기의 선택 횟수 업데이트 (최대 5로 제한)
    public void IncrementWeaponSelectionCount(string weaponId)
    {
        // 무기 ID별로 고유한 카운트 관리
        if (!weaponSelectionCount.ContainsKey(weaponId))
        {
            weaponSelectionCount[weaponId] = 0;
            Debug.Log($"[WeaponsInventoryUI] 새로운 무기 {weaponId}의 카운트를 초기화했습니다.");
        }

        // 최대 카운트 이하인 경우에만 증가
        if (weaponSelectionCount[weaponId] < maxSelectionCount)
        {
            weaponSelectionCount[weaponId]++;
            Debug.Log($"[WeaponsInventoryUI] 무기 {weaponId}의 선택 횟수가 {weaponSelectionCount[weaponId]}로 증가했습니다.");

            // 카운트가 변경되면 즉시 UI 업데이트 (선택적으로 활성화 가능)
            // UpdateWeaponsDisplay();
        }
        else
        {
            Debug.Log($"[WeaponsInventoryUI] 무기 {weaponId}의 선택 횟수가 이미 최대({maxSelectionCount})에 도달했습니다.");
        }

        // 디버그용 출력
        DebugWeaponSelections();
    }

    // 무기 선택 횟수 가져오기
    private int GetWeaponSelectionCount(string weaponId)
    {
        if (weaponSelectionCount.ContainsKey(weaponId))
        {
            Debug.Log($"[WeaponsInventoryUI] 무기 {weaponId}의 선택 카운트: {weaponSelectionCount[weaponId]}");
            return weaponSelectionCount[weaponId];
        }

        // 디버그를 위한 로그 추가
        Debug.Log($"[WeaponsInventoryUI] 무기 {weaponId}의 선택 카운트가 없습니다 (0 반환)");
        return 0;
    }

    // 서브 무기 표시명 가져오기
    private string GetSubWeaponDisplayName(Weapon.SubWeapon.WeaponType weaponType)
    {
        if (WeaponStatProvider.Instance != null)
        {
            var subWeaponStat = WeaponStatProvider.Instance.subWeaponStats.Find(x => x.weaponType == weaponType);

            // 구조체는 null과 직접 비교할 수 없으므로 displayName 속성만 확인
            if (!string.IsNullOrEmpty(subWeaponStat.displayName))
            {
                // 로그 추가 - 디버그용
                Debug.Log($"[WeaponsInventoryUI] GetSubWeaponDisplayName - 타입: {weaponType}, 이름: {subWeaponStat.displayName}");
                return subWeaponStat.displayName;
            }
        }
        else
        {
            Debug.LogWarning("[WeaponsInventoryUI] WeaponStatProvider.Instance가 null입니다. 기본 타입명을 사용합니다.");
        }

        // 기본값으로 타입명 반환
        return weaponType.ToString();
    }

    // 현재 선택한 무기의 디버그 정보 출력 (테스트용)
    private void DebugWeaponSelections()
    {
        Debug.Log("===== 현재 무기 선택 카운트 정보 =====");
        foreach (var kvp in weaponSelectionCount)
        {
            Debug.Log($"무기 ID: {kvp.Key}, 선택 횟수: {kvp.Value}");
        }
        Debug.Log("===================================");
    }

    // 모든 무기 레벨을 초기화 (테스트용)
    public void ResetWeaponSelectionCounts()
    {
        weaponSelectionCount.Clear();
        Debug.Log("[WeaponsInventoryUI] 모든 무기 선택 카운트가 초기화되었습니다.");
        UpdateWeaponsDisplay(); // UI 즉시 업데이트
    }

    // 업데이트된 다이어그램 출력 메서드 (디버깅 도구 강화)
    public void PrintWeaponLevels()
    {
        Debug.Log("===== 현재 무기 레벨 상태 =====");

        if (weaponSelectionCount.Count == 0)
        {
            Debug.Log("무기 레벨 정보가 없습니다.");
            return;
        }

        foreach (var kvp in weaponSelectionCount)
        {
            string weaponId = kvp.Key;
            int level = kvp.Value;

            // 무기 타입 추출 (Sub_PaperPlane에서 PaperPlane 추출)
            string weaponTypeName = "알 수 없음";
            if (weaponId.StartsWith("Sub_"))
            {
                weaponTypeName = weaponId.Substring(4);
            }

            // 레벨 시각화
            string levelBar = "";
            for (int i = 0; i < maxSelectionCount; i++)
            {
                if (i < level)
                    levelBar += "0"; // 채워진 레벨
                else
                    levelBar += "-"; // 비어있는 레벨
            }

            Debug.Log($"{weaponTypeName}: {levelBar} ({level}/{maxSelectionCount})");
        }

        Debug.Log("==============================");
    }
}