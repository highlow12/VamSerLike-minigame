using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitJson;

public class LevelUpUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] public List<Button> optionButtons = new List<Button>();
    [SerializeField] public List<Image> optionIcons = new List<Image>();
    [SerializeField] public List<TextMeshProUGUI> optionTexts = new List<TextMeshProUGUI>();

    [Header("레벨 인디케이터 설정")]
    [SerializeField] private List<Transform> levelIndicatorContainers = new List<Transform>();
    [SerializeField] private GameObject indicatorPrefab; // 원형 인디케이터 프리팹
    [SerializeField] private Color indicatorEmptyColor = Color.gray;
    [SerializeField] private Color indicatorFilledColor = Color.yellow;
    [SerializeField] private Color indicatorBlinkColor = Color.white;
    [SerializeField] private float blinkInterval = 0.5f; // 깜빡이는 간격 (초)

    [Header("Settings")]
    [SerializeField] private int maxSkillLevel = 5; // 최대 무기 레벨
    [SerializeField] private int maxSkillCount = 6; // 선택 가능한 최대 무기 개수
    [SerializeField] private float panelShowDuration = 0.5f;
    [SerializeField] private AudioClip levelUpSound;

    private AudioSource audioSource;
    private List<Coroutine> blinkCoroutines = new List<Coroutine>();
    private WeaponsInventoryUI weaponsInventoryUI;
    private WeaponStatProvider.SubWeaponStat lastSelectedWeapon;

    // 레벨 인디케이터 캐시용 배열
    private List<List<Image>> levelIndicators = new List<List<Image>>();

    // 패널 닫힘 이벤트 추가
    public event Action onPanelClosed = delegate { };

    private void Awake()
    {
        // 오디오 소스 컴포넌트 확인
        InitializeAudioSource();

        // 레벨업 패널 초기 비활성화
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[LevelUpUI] levelUpPanel is null!");
        }

        // 버튼 이벤트 설정
        SetupButtonEvents();

        // 레벨 인디케이터 초기화
        InitializeLevelIndicators();
    }

    private void InitializeAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[LevelUpUI] AudioSource 컴포넌트 추가됨");
        }
    }

    private void Start()
    {
        // WeaponsInventoryUI 참조 찾기
        weaponsInventoryUI = FindFirstObjectByType<WeaponsInventoryUI>();
        if (weaponsInventoryUI == null)
        {
            Debug.LogWarning("[LevelUpUI] WeaponsInventoryUI를 찾을 수 없습니다!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 정리
        onPanelClosed = delegate { };

        // 깜빡임 코루틴 정리
        StopAllBlinkCoroutines();
    }

    // 레벨 인디케이터 초기화
    private void InitializeLevelIndicators()
    {
        levelIndicators.Clear();

        // 각 무기 옵션에 대한 인디케이터 컨테이너 확인
        for (int i = 0; i < levelIndicatorContainers.Count; i++)
        {
            List<Image> indicators = new List<Image>();

            if (levelIndicatorContainers[i] != null)
            {
                // 이미 존재하는 인디케이터 사용
                foreach (Transform child in levelIndicatorContainers[i])
                {
                    Image indicator = child.GetComponent<Image>();
                    if (indicator != null)
                    {
                        indicators.Add(indicator);
                        indicator.color = indicatorEmptyColor;
                    }
                }

                // 인디케이터가 없거나 부족하면 프리팹으로 생성
                if (indicators.Count < maxSkillLevel && indicatorPrefab != null)
                {
                    int toCreate = maxSkillLevel - indicators.Count;
                    for (int j = 0; j < toCreate; j++)
                    {
                        GameObject newIndicator = Instantiate(indicatorPrefab, levelIndicatorContainers[i]);
                        Image indicatorImage = newIndicator.GetComponent<Image>();
                        if (indicatorImage != null)
                        {
                            indicators.Add(indicatorImage);
                            indicatorImage.color = indicatorEmptyColor;
                        }
                    }
                }
            }

            levelIndicators.Add(indicators);
        }
    }

    // 버튼 이벤트 설정
    private void SetupButtonEvents()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                int buttonIndex = i; // 클로저를 위한 로컬 변수
                optionButtons[i].onClick.AddListener(() => OnOptionButtonClicked(buttonIndex));
            }
            else
            {
                Debug.LogWarning($"[LevelUpUI] Option button at index {i} is null!");
            }
        }
    }

    // 레벨업 패널 표시
    public void ShowLevelUpPanel()
    {
        Debug.Log("[LevelUpUI] ShowLevelUpPanel 메서드 호출됨");

        // 이미 패널이 활성화되어 있는지 확인
        if (levelUpPanel != null && levelUpPanel.activeSelf)
        {
            Debug.Log("[LevelUpUI] 패널이 이미 활성화되어 있습니다. 중복 호출 무시.");
            return;
        }

        if (levelUpPanel == null)
        {
            Debug.LogError("[LevelUpUI] levelUpPanel이 null입니다!");
            return;
        }

        // 모든 깜빡임 코루틴 중지
        StopAllBlinkCoroutines();

        // 게임 일시 정지
        GameManager.IsGamePaused = true;

        // 레벨 텍스트 업데이트
        if (levelText != null && GameManager.Instance != null)
        {
            levelText.text = $"Level {GameManager.Instance.playerLevel}";
            Debug.Log($"[LevelUpUI] 레벨 텍스트 업데이트: {levelText.text}");
        }

        // 레벨업 효과음 재생
        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        // 옵션 초기화
        ClearOptionButtons();

        // 패널 활성화
        levelUpPanel.SetActive(true);
        Debug.Log("[LevelUpUI] 레벨업 패널 활성화됨");
    }

    // 모든 옵션 버튼 초기화
    private void ClearOptionButtons()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] != null)
            {
                optionButtons[i].gameObject.SetActive(false);
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => OnOptionButtonClicked(i));
            }

            if (i < optionIcons.Count && optionIcons[i] != null)
            {
                optionIcons[i].sprite = null;
                optionIcons[i].gameObject.SetActive(false);
            }

            if (i < optionTexts.Count && optionTexts[i] != null)
            {
                optionTexts[i].text = string.Empty;
                optionTexts[i].gameObject.SetActive(false);
            }

            // 레벨 인디케이터 초기화
            if (i < levelIndicators.Count)
            {
                foreach (var indicator in levelIndicators[i])
                {
                    indicator.color = indicatorEmptyColor;
                    indicator.gameObject.SetActive(false);
                }
            }
        }
    }

    // 서브 무기 옵션 설정
    public void SetSubWeaponItem(WeaponStatProvider.SubWeaponStat subWeaponStat, int buttonIndex, string displayName = null, Sprite icon = null)
    {
        if (buttonIndex >= optionButtons.Count || string.IsNullOrEmpty(subWeaponStat.displayName))
        {
            Debug.LogError($"[LevelUpUI] Invalid button index or subWeaponStat: {buttonIndex}");
            return;
        }

        // 무기 이름이 제공되지 않은 경우 가져오기
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = subWeaponStat.displayName;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = subWeaponStat.weaponType.ToString();
                Debug.LogWarning($"[LevelUpUI] 서브 무기 displayName이 비어 있음: {subWeaponStat.weaponType}");
            }
        }

        // 아이콘이 null이고 LevelUpSelectManager를 통해 로드 시도
        if (icon == null && LevelUpSelectManager.Instance != null)
        {
            icon = LevelUpSelectManager.Instance.LoadSubWeaponSprite(subWeaponStat);
        }

        // 버튼 설정
        if (optionButtons[buttonIndex] != null)
        {
            optionButtons[buttonIndex].gameObject.SetActive(true);

            // 데이터 캐시 (버튼 클릭 시 사용)
            optionButtons[buttonIndex].onClick.RemoveAllListeners();
            optionButtons[buttonIndex].onClick.AddListener(() => {
                if (LevelUpSelectManager.Instance != null)
                {
                    // 선택된 무기 저장
                    lastSelectedWeapon = subWeaponStat;

                    // 디버그 로그 추가
                    Debug.Log($"[LevelUpUI] 무기 선택됨: {subWeaponStat.weaponType} (등급 {subWeaponStat.weaponGrade})");

                    // 무기 생성 및 패널 닫기
                    LevelUpSelectManager.Instance.CreateSubWeaponItem(subWeaponStat);
                    ClosePanel();
                }
                else
                {
                    Debug.LogError("[LevelUpUI] LevelUpSelectManager.Instance is null!");
                }
            });

            // 아이콘 설정
            if (buttonIndex < optionIcons.Count && optionIcons[buttonIndex] != null)
            {
                optionIcons[buttonIndex].sprite = icon;
                optionIcons[buttonIndex].gameObject.SetActive(icon != null);
            }

            // 텍스트 설정
            if (buttonIndex < optionTexts.Count && optionTexts[buttonIndex] != null)
            {
                // 등급에 따른 텍스트 포맷팅
                string gradeText = GetGradeText(subWeaponStat.weaponGrade);
                optionTexts[buttonIndex].text = $"{displayName} ({gradeText})";
                optionTexts[buttonIndex].gameObject.SetActive(true);
            }

            // 레벨 인디케이터 업데이트
            UpdateLevelIndicators(subWeaponStat, buttonIndex);
        }
    }

    // 레벨 인디케이터 업데이트
    private void UpdateLevelIndicators(WeaponStatProvider.SubWeaponStat subWeaponStat, int buttonIndex)
    {
        if (buttonIndex >= levelIndicators.Count) return;

        // 무기 ID 생성 (타입만으로 ID 생성하여 동일 타입은 레벨 공유)
        string weaponId = $"Sub_{subWeaponStat.weaponType}";

        // 현재 무기 레벨 가져오기
        int currentLevel = GetWeaponLevel(weaponId);

        // 인디케이터 활성화 및 설정
        for (int i = 0; i < levelIndicators[buttonIndex].Count; i++)
        {
            Image indicator = levelIndicators[buttonIndex][i];
            indicator.gameObject.SetActive(true);

            if (i < currentLevel)
            {
                // 이미 획득한 레벨
                indicator.color = indicatorFilledColor;
            }
            else if (i == currentLevel && currentLevel < maxSkillLevel)
            {
                // 다음에 획득할 레벨 (깜빡임)
                indicator.color = indicatorEmptyColor;
                Coroutine blinkCoroutine = StartCoroutine(BlinkIndicator(indicator));
                blinkCoroutines.Add(blinkCoroutine);
            }
            else
            {
                // 아직 획득하지 않은 레벨
                indicator.color = indicatorEmptyColor;
            }
        }
    }

    // 버튼 클릭 처리
    private void OnOptionButtonClicked(int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] Option button {buttonIndex} clicked");
        // 이미 버튼에 리스너를 설정했으므로 추가 처리 필요 없음
    }

    // 패널 닫기
    public void ClosePanel()
    {
        Debug.Log("[LevelUpUI] ClosePanel 호출됨");

        // 선택된 무기가 있으면 인벤토리 업데이트
        if (lastSelectedWeapon.weaponType != 0 && weaponsInventoryUI != null)
        {
            // 무기 선택 정보 전달 (무기 타입만으로 ID 생성)
            string weaponId = $"Sub_{lastSelectedWeapon.weaponType}";

            // 디버그 정보 추가
            Debug.Log($"[LevelUpUI] 선택된 무기: {lastSelectedWeapon.weaponType}, ID: {weaponId}, 등급: {lastSelectedWeapon.weaponGrade}");

            // 무기 레벨 증가
            weaponsInventoryUI.IncrementWeaponSelectionCount(weaponId);
            Debug.Log($"[LevelUpUI] 무기 {weaponId}의 레벨이 증가됨");

            // 선택된 무기 초기화
            lastSelectedWeapon = new WeaponStatProvider.SubWeaponStat();
        }
        else
        {
            Debug.Log("[LevelUpUI] 선택된 무기가 없거나 weaponsInventoryUI가 null입니다.");
        }

        // 패널 비활성화
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // 깜빡임 코루틴 중지
        StopAllBlinkCoroutines();

        // 이벤트 발생
        onPanelClosed?.Invoke();
        Debug.Log("[LevelUpUI] onPanelClosed 이벤트 발생");

        // 게임 재개
        GameManager.IsGamePaused = false;
    }

    // 모든 깜빡임 코루틴 중지
    private void StopAllBlinkCoroutines()
    {
        foreach (Coroutine coroutine in blinkCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        blinkCoroutines.Clear();
    }

    // 인디케이터 깜빡임 효과
    private IEnumerator BlinkIndicator(Image indicator)
    {
        bool isBlinking = false;

        while (true)
        {
            isBlinking = !isBlinking;
            indicator.color = isBlinking ? indicatorBlinkColor : indicatorEmptyColor;

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    // 무기 레벨 가져오기
    private int GetWeaponLevel(string weaponId)
    {
        if (weaponsInventoryUI != null)
        {
            try
            {
                // 리플렉션으로 private 메서드 호출 (필요시)
                var selectionCountsField = weaponsInventoryUI.GetType().GetField("weaponSelectionCount",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (selectionCountsField != null)
                {
                    var selectionCounts = selectionCountsField.GetValue(weaponsInventoryUI) as Dictionary<string, int>;
                    if (selectionCounts != null && selectionCounts.ContainsKey(weaponId))
                    {
                        int level = selectionCounts[weaponId];
                        Debug.Log($"[LevelUpUI] 무기 {weaponId}의 현재 레벨: {level}");
                        return level;
                    }
                    else
                    {
                        Debug.Log($"[LevelUpUI] 무기 {weaponId}의 레벨 정보가 없습니다 (0 반환)");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LevelUpUI] weaponSelectionCount 접근 중 오류: {e.Message}");
            }
        }

        return 0;
    }

    // 무기 등급 텍스트 반환
    private string GetGradeText(int grade)
    {
        switch (grade)
        {
            case 0: return "신규";
            case 1: return "레벨 1";
            case 2: return "레벨 2";
            case 3: return "레벨 3";
            case 4: return "레벨 4";
            case 5: return "레벨 5";
            default: return $"레벨 {grade}";
        }
    }
}