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

    [Header("Settings")]
    [SerializeField] private float panelShowDuration = 0.5f;
    [SerializeField] private AudioClip levelUpSound;

    private AudioSource audioSource;

    // 패널 닫힘 이벤트 추가
    public event Action onPanelClosed = delegate { };

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 레벨업 패널 초기 비활성화
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // 버튼 이벤트 설정
        SetupButtonEvents();
    }

    private void Start()
    {
        // 레벨업 이벤트 구독
        if (GameManager.Instance != null)
        {
            Debug.Log("[LevelUpUI] GameManager.Instance 이벤트에 구독합니다.");
            GameManager.Instance.onPlayerLevelChanged += ShowLevelUpPanel;
        }
        else
        {
            Debug.LogError("[LevelUpUI] GameManager.Instance가 null입니다!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onPlayerLevelChanged -= ShowLevelUpPanel;
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

        // 게임 일시 정지
        GameManager.IsGamePaused = true;

        // 레벨 텍스트 업데이트
        if (levelText != null)
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
            }

            if (i < optionIcons.Count && optionIcons[i] != null)
            {
                optionIcons[i].sprite = null;
            }

            if (i < optionTexts.Count && optionTexts[i] != null)
            {
                optionTexts[i].text = string.Empty;
            }
        }
    }

    // 메인 무기 옵션 설정
    public void SetMainWeaponItem(JsonData userData, int buttonIndex, string displayName = null, Sprite icon = null)
    {
        if (buttonIndex >= optionButtons.Count || userData == null)
        {
            Debug.LogError($"[LevelUpUI] Invalid button index or userData: {buttonIndex}");
            return;
        }

        // 무기 타입 파싱
        Weapon.MainWeapon.WeaponType weaponType = (Weapon.MainWeapon.WeaponType)Enum.Parse(
            typeof(Weapon.MainWeapon.WeaponType),
            userData["weaponType"].ToString()
        );

        // 무기 이름이 제공되지 않은 경우 가져오기
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = WeaponStatProvider.Instance.weaponStats.Find(x => x.weaponType == weaponType).displayName;
        }

        // 무기 아이콘이 제공되지 않은 경우 가져오기
        if (icon == null)
        {
            GameObject weaponPrefab = Resources.Load<GameObject>($"Prefabs/Player/Weapon/MainWeapon/{weaponType}");
            if (weaponPrefab != null && weaponPrefab.GetComponent<SpriteRenderer>() != null)
            {
                icon = weaponPrefab.GetComponent<SpriteRenderer>().sprite;
            }
        }

        // 버튼 설정
        if (optionButtons[buttonIndex] != null)
        {
            optionButtons[buttonIndex].gameObject.SetActive(true);

            // 데이터 캐싱 (버튼 클릭 시 사용)
            optionButtons[buttonIndex].onClick.RemoveAllListeners();
            optionButtons[buttonIndex].onClick.AddListener(() => {
                LevelUpSelectManager.Instance.CreateMainWeaponItem(userData);
                ClosePanel();
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
                optionTexts[buttonIndex].text = displayName;
            }
        }
    }

    // 서브 무기 옵션 설정
    public void SetSubWeaponItem(WeaponStatProvider.SubWeaponStat subWeaponStat, int buttonIndex, string displayName = null, Sprite icon = null)
    {
        if (buttonIndex >= optionButtons.Count || Equals(subWeaponStat, default))
        {
            Debug.LogError($"[LevelUpUI] Invalid button index or subWeaponStat: {buttonIndex}");
            return;
        }

        // 무기 이름이 제공되지 않은 경우 가져오기
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = subWeaponStat.displayName;
        }

        // 무기 아이콘이 제공되지 않은 경우 가져오기
        if (icon == null)
        {
            SubWeaponSO subWeaponSO = Resources.Load<SubWeaponSO>($"ScriptableObjects/SubWeapon/{displayName}/{displayName}_{subWeaponStat.weaponGrade}");
            if (subWeaponSO != null)
            {
                icon = subWeaponSO.weaponSprite;
            }
        }

        // 버튼 설정
        if (optionButtons[buttonIndex] != null)
        {
            optionButtons[buttonIndex].gameObject.SetActive(true);

            // 데이터 캐싱 (버튼 클릭 시 사용)
            optionButtons[buttonIndex].onClick.RemoveAllListeners();
            optionButtons[buttonIndex].onClick.AddListener(() => {
                LevelUpSelectManager.Instance.CreateSubWeaponItem(subWeaponStat);
                ClosePanel();
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
            }
        }
    }

    // 버튼 클릭 처리
    private void OnOptionButtonClicked(int buttonIndex)
    {
        Debug.Log($"[LevelUpUI] Option button {buttonIndex} clicked");
        // 이미 버튼에 리스너를 설정했으므로 추가 처리는 필요 없음
    }

    // 패널 닫기
    public void ClosePanel()
    {
        Debug.Log("[LevelUpUI] ClosePanel 호출됨");

        // 패널 비활성화 (먼저 실행)
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        // 이벤트 발생
        onPanelClosed?.Invoke();
        Debug.Log("[LevelUpUI] onPanelClosed 이벤트 발생");

        // GameManager에 알림 (마지막에 실행)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelUpPanelClosed();
        }
        else
        {
            Debug.LogError("[LevelUpUI] GameManager.Instance가 null입니다!");
        }

        // 게임 재개 (가장 마지막에 실행)
        GameManager.IsGamePaused = false;
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