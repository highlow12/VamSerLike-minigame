using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 버튼을 통해 플레이어에게 경험치를 추가하는 기능을 제공하는 컴포넌트입니다.
/// </summary>
public class AddExp : MonoBehaviour
{
    [Header("경험치 설정")]
    [Tooltip("버튼을 누를 때마다 추가되는 경험치 양")]
    public float expAmount = 10f;
    
    [Tooltip("경험치 추가 시 텍스트로 표시할지 여부")]
    public bool showExpText = true;
    
    [Header("UI 참조")]
    [Tooltip("경험치 추가 버튼")]
    public Button addExpButton;
    
    [Tooltip("경험치 추가 시 표시될 텍스트")]
    public TextMeshProUGUI expAddedText;
    
    [Header("텍스트 설정")]
    [Tooltip("경험치 추가 시 표시되는 텍스트 지속 시간")]
    public float textDisplayDuration = 1.5f;
    
    [Tooltip("경험치 추가 사운드")]
    public AudioClip expAddSound;
    
    private AudioSource audioSource;
    private PlayerExpBar playerExpBar;
    private float textTimer = 0f;

    private void Awake()
    {
        // AudioSource 컴포넌트 가져오기 또는 추가하기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 버튼이 할당되지 않은 경우 현재 게임오브젝트에서 찾기
        if (addExpButton == null)
        {
            addExpButton = GetComponent<Button>();
        }
        
        // 버튼 이벤트 설정
        if (addExpButton != null)
        {
            addExpButton.onClick.AddListener(AddExperienceToPlayer);
        }
        else
        {
            Debug.LogError("AddExp 스크립트에 버튼이 할당되지 않았습니다!");
        }
        
        // 경험치 텍스트가 있을 경우 처음에는 비활성화
        if (expAddedText != null)
        {
            expAddedText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // PlayerExpBar 참조 찾기 (UI에 있을 가능성이 높음)
        playerExpBar = FindObjectOfType<PlayerExpBar>();
        if (playerExpBar == null)
        {
            Debug.LogWarning("PlayerExpBar를 찾을 수 없습니다. UI 업데이트가 자동으로 되지 않을 수 있습니다.");
        }
    }

    private void Update()
    {
        // 경험치 텍스트 표시 타이머 관리
        if (expAddedText != null && expAddedText.gameObject.activeSelf)
        {
            textTimer -= Time.deltaTime;
            if (textTimer <= 0f)
            {
                expAddedText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 플레이어에게 경험치를 추가하는 메서드
    /// </summary>
    public void AddExperienceToPlayer()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance가 null입니다!");
            return;
        }

        // 게임 상태 확인
        if (GameManager.Instance.gameState != GameManager.GameState.InGame)
        {
            Debug.Log("게임 중이 아닐 때는 경험치를 추가할 수 없습니다.");
            return;
        }

        // 경험치 추가
        GameManager.Instance.AddExperience(expAmount);
        Debug.Log($"플레이어에게 {expAmount} 경험치가 추가되었습니다. 현재 경험치: {GameManager.Instance.playerExperience}/{GameManager.Instance.experienceToLevelUp}");

        // 경험치 추가 사운드 재생
        if (audioSource != null && expAddSound != null)
        {
            audioSource.PlayOneShot(expAddSound);
        }

        // 경험치 추가 텍스트 표시
        if (showExpText && expAddedText != null)
        {
            expAddedText.text = $"+{expAmount} EXP";
            expAddedText.gameObject.SetActive(true);
            textTimer = textDisplayDuration;
        }

        // PlayerExpBar UI 수동 업데이트
        if (playerExpBar != null)
        {
            playerExpBar.UpdateUI();
        }
    }

    /// <summary>
    /// 추가할 경험치 양을 설정하는 메서드
    /// </summary>
    /// <param name="amount">추가할 경험치 양</param>
    public void SetExpAmount(float amount)
    {
        expAmount = amount;
        Debug.Log($"경험치 추가량이 {expAmount}로 변경되었습니다.");
    }
}