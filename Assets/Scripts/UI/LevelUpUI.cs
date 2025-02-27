using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Image experienceBarFill; // 슬라이더 대신 Image를 사용할 경우
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Item Selection UI")]
    [SerializeField] private GameObject[] itemButtons; // 3개의 선택 버튼
    [SerializeField] private Image[] itemIcons;
    [SerializeField] private TextMeshProUGUI[] itemNames;
    [SerializeField] private TextMeshProUGUI[] itemDescriptions;

    // 아이템 아이콘 매핑
    [SerializeField] private Sprite[] mainWeaponIcons;
    [SerializeField] private Sprite[] subWeaponIcons;
    [SerializeField] private Sprite[] supportItemIcons;

    // 메인 무기, 서브 무기, 서포트 아이템 데이터 저장
    private LitJson.JsonData mainWeaponData;
    private WeaponStatProvider.SubWeaponStat subWeaponStat;
    private int selectedItemIndex = -1;


    // Awake 메서드 추가 - 가장 일찍 구독하기 위함
    private void Awake()
    {
        Debug.Log("[LevelUpUI] Awake 호출됨");
        // 게임 오브젝트와 컴포넌트가 활성화되어 있는지 확인
        Debug.Log($"[LevelUpUI] 게임 오브젝트 활성화 상태: {gameObject.activeInHierarchy}");

        // Awake에서 이벤트 구독 (가장 빠른 시점)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlayerLevelChanged += OnPlayerLevelUp;
            Debug.Log("[LevelUpUI] GameManager.onPlayerLevelChanged 이벤트 구독 (Awake)");
        }
        else
        {
            Debug.LogError("[LevelUpUI] Awake에서 GameManager.Instance가 null입니다!");
        }
    }
    private void OnEnable()
    {
        Debug.Log("[LevelUpUI] OnEnable 호출됨");
        // OnEnable에서도 이벤트 재구독
        if (GameManager.Instance != null)
        {
            // 중복 구독 방지를 위해 먼저 구독 해제
            GameManager.Instance.onPlayerLevelChanged -= OnPlayerLevelUp;
            GameManager.Instance.onPlayerLevelChanged += OnPlayerLevelUp;
            Debug.Log("[LevelUpUI] GameManager.onPlayerLevelChanged 이벤트 재구독 (OnEnable)");
        }
    }

    private void Start()
    {
        Debug.Log("[LevelUpUI] Start 호출됨");

        // 레벨업 패널과 선택 패널 초기 상태 확인
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
            Debug.Log("[LevelUpUI] levelUpPanel 초기 상태 설정");
        }
        else
        {
            Debug.LogError("[LevelUpUI] levelUpPanel 참조가 없습니다!");
        }

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
            Debug.Log("[LevelUpUI] selectionPanel 초기 상태 설정");
        }
        else
        {
            Debug.LogError("[LevelUpUI] selectionPanel 참조가 없습니다!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlayerLevelChanged -= OnPlayerLevelUp;
            Debug.Log("[LevelUpUI] GameManager.onPlayerLevelChanged 이벤트 구독 해제");
        }
    }

    private void Update()
    {
        // 게임 플레이 중에 경험치 바 업데이트
        if (GameManager.Instance != null && GameManager.Instance.gameState == GameManager.GameState.InGame)
        {
            UpdateExperienceBar();
        }
    }

    // 경험치 바 업데이트 메서드
    private void UpdateExperienceBar()
    {
        // 경험치 바를 PlayerExpBar 컴포넌트가 관리하므로 여기서는 불필요
        // 레벨 텍스트만 업데이트
        if (levelText != null)
        {
            levelText.text = "Lv. " + GameManager.Instance.playerLevel;
        }
    }

    // 레벨업 이벤트 핸들러
    private void OnPlayerLevelUp()
    {
        Debug.Log("[LevelUpUI] 레벨업 호출됨");

        GameManager.IsGamePaused = true;

        // 레벨업 패널 표시
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            Debug.Log("[LevelUpUI] levelUpPanel 활성화");
        }

        // 선택 패널 표시
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            Debug.Log("[LevelUpUI] selectionPanel 활성화");
            GenerateItemOptions();
        }
    }

    //private IEnumerator ShowLevelUpAnimation()
    //{
    //    Debug.Log("[LevelUpUI] ShowLevelUpAnimation 시작");

    //    // 레벨업 애니메이션 대기 (예: 1초 대기)
    //    yield return new WaitForSecondsRealtime(1f);

    //    // 무기 선택 패널 표시
    //    if (selectionPanel != null)
    //    {
    //        selectionPanel.SetActive(true);
    //        Debug.Log("[LevelUpUI] selectionPanel 활성화");

    //        // 무기 선택 옵션 생성
    //        GenerateItemOptions();
    //    }
    //    else
    //    {
    //        Debug.LogError("[LevelUpUI] selectionPanel 참조가 없습니다!");
    //    }
    //}

    // 아이템 선택지 생성
    public void GenerateItemOptions()
    {
        Debug.Log("[LevelUpUI] GenerateItemOptions 시작");

        // 아이템 버튼 초기화
        ResetItemButtons();

        // 강제로 버튼 활성화 (테스트용)
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] != null)
            {
                itemButtons[i].SetActive(true);
                Debug.Log($"[LevelUpUI] 강제 버튼 활성화: {i}번 버튼");
            }
        }

        // 레벨업 선택 매니저를 통해 아이템 리스트 생성
        if (LevelUpSelectManager.Instance != null)
        {
            LevelUpSelectManager.Instance.CreateItemList();
            Debug.Log("[LevelUpUI] LevelUpSelectManager.CreateItemList 호출");
        }
        else
        {
            Debug.LogError("[LevelUpUI] LevelUpSelectManager.Instance is null!");
        }

        // 버튼 클릭 리스너 설정
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] != null)
            {
                int buttonIndex = i;
                Button button = itemButtons[i].GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SelectItem(buttonIndex));
                    Debug.Log($"[LevelUpUI] {i}번 버튼 리스너 설정");
                }
            }
        }
    }

    // 아이템 선택 처리
    public void SelectItem(int index)
    {
        Debug.Log($"[LevelUpUI] SelectItem({index}) 호출됨");
        selectedItemIndex = index;

        // 선택된 아이템 적용
        ApplySelectedItem();

        // UI 닫기
        CloseSelectionUI();
    }

    // 선택된 아이템 적용
    private void ApplySelectedItem()
    {
        Debug.Log($"[LevelUpUI] ApplySelectedItem 호출됨, 선택 인덱스: {selectedItemIndex}");

        // 실제 구현에서는 LevelUpSelectManager의 메서드를 호출하여 아이템 적용
        // 이 부분은 선택한 아이템 타입에 따라 달라짐
    }

    // 선택 UI 닫기
    private void CloseSelectionUI()
    {
        Debug.Log("[LevelUpUI] CloseSelectionUI 호출됨");

        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // 게임 재개
        GameManager.IsGamePaused = false;  // 이것이 Time.timeScale을 1로 변경
        Debug.Log($"[LevelUpUI] 게임 재개: IsGamePaused={GameManager.IsGamePaused}, TimeScale={Time.timeScale}");

        // 경험치 바 업데이트
        UpdateExperienceBar();
    }

    // 아이템 버튼 초기화 메서드 수정
    private void ResetItemButtons()
    {
        Debug.Log("[LevelUpUI] ResetItemButtons 시작");

        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] != null)
            {
                // 아이콘 초기화
                if (itemIcons.Length > i && itemIcons[i] != null)
                {
                    itemIcons[i].sprite = null;
                }

                // 이름 초기화
                if (itemNames.Length > i && itemNames[i] != null)
                {
                    itemNames[i].text = $"무기 {i + 1}";
                }

                // 설명 초기화
                if (itemDescriptions.Length > i && itemDescriptions[i] != null)
                {
                    itemDescriptions[i].text = "새로운 무기를 선택하세요.";
                }

                // 강제로 버튼 활성화
                itemButtons[i].SetActive(true);
            }
        }

        selectedItemIndex = -1;
    }

    // 메인 무기 아이템 UI 설정
    public void SetMainWeaponItem(LitJson.JsonData weaponData, int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] SetMainWeaponItem 시작, 버튼 인덱스: {buttonIndex}");

        // 버튼 인덱스 유효성 검사
        if (buttonIndex < 0 || buttonIndex >= itemButtons.Length)
        {
            Debug.LogError($"[LevelUpUI] 버튼 인덱스 범위 초과: {buttonIndex}");
            return;
        }

        // itemButtons 널 체크
        if (itemButtons[buttonIndex] == null)
        {
            Debug.LogError($"[LevelUpUI] itemButtons[{buttonIndex}]이(가) null입니다!");
            return;
        }

        mainWeaponData = weaponData;

        Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)System.Enum.Parse(
            typeof(Weapon.MainWeapon.WeaponType),
            weaponData["weaponType"]["N"].ToString()
        );

        // 무조건 버튼 활성화
        itemButtons[buttonIndex].SetActive(true);

        // 아이콘 설정 (최소한의 처리)
        if (itemIcons.Length > buttonIndex && itemIcons[buttonIndex] != null)
        {
            itemIcons[buttonIndex].sprite = null; // 아이콘 없어도 괜찮
        }

        // 이름 설정
        if (itemNames.Length > buttonIndex && itemNames[buttonIndex] != null)
        {
            itemNames[buttonIndex].text = weaponType.ToString();
        }

        // 설명 설정
        if (itemDescriptions.Length > buttonIndex && itemDescriptions[buttonIndex] != null)
        {
            itemDescriptions[buttonIndex].text = "새로운 주무기";
        }

        // 버튼 클릭 리스너 설정
        Button button = itemButtons[buttonIndex].GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => {
                Debug.Log("[LevelUpUI] 주무기 버튼 클릭됨");
                if (LevelUpSelectManager.Instance != null)
                {
                    LevelUpSelectManager.Instance.CreateMainWeaponItem(weaponData);
                }
                CloseSelectionUI();
            });
        }
        else
        {
            Debug.LogWarning("[LevelUpUI] 버튼 컴포넌트를 찾을 수 없습니다.");
        }
    }

    // 서브 무기 아이템 UI 설정
    public void SetSubWeaponItem(WeaponStatProvider.SubWeaponStat stat, int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] SetSubWeaponItem 호출됨, 버튼 인덱스: {buttonIndex}");

        if (buttonIndex < 0 || buttonIndex >= itemButtons.Length) return;

        subWeaponStat = stat;
        itemButtons[buttonIndex].SetActive(true);

        // 무기 타입에 따른 아이콘 설정
        int iconIndex = (int)stat.weaponType;
        if (iconIndex < subWeaponIcons.Length)
        {
            itemIcons[buttonIndex].sprite = subWeaponIcons[iconIndex];
        }

        itemNames[buttonIndex].text = stat.weaponType.ToString();

        // 새 무기인지 업그레이드인지에 따라 설명 변경
        if (stat.weaponGrade == 0)
        {
            itemDescriptions[buttonIndex].text = "새로운 서브 무기를 추가합니다.";
        }
        else
        {
            itemDescriptions[buttonIndex].text = $"서브 무기를 레벨 {stat.weaponGrade}로 업그레이드합니다.";
        }

        // 버튼 클릭 이벤트 업데이트
        Button button = itemButtons[buttonIndex].GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            Debug.Log("[LevelUpUI] 서브 무기 버튼 클릭됨");
            if (LevelUpSelectManager.Instance != null)
            {
                LevelUpSelectManager.Instance.CreateSubWeaponItem(stat);
            }
            CloseSelectionUI();
        });
    }

    // 디버그용 메서드 - 인스펙터에서 테스트용으로 호출 가능
    public void TestShowLevelUpPanel()
    {
        Debug.Log("[LevelUpUI] TestShowLevelUpPanel 호출됨");
        OnPlayerLevelUp();
    }
}