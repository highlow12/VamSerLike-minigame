using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class buttonTest : MonoBehaviour
{
    [Header("테스트 버튼")]
    [SerializeField] private Button addXpButton;     // 경험치 추가 버튼
    [SerializeField] private Button levelUpButton;   // 강제 레벨업 버튼

    [Header("설정")]
    [SerializeField] private float xpAmountToAdd = 50f;  // 추가할 경험치량

    void Start()
    {
        // 버튼 이벤트 연결
        if (addXpButton != null)
        {
            addXpButton.onClick.AddListener(AddExperience);
        }

        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(ForcePlayerLevelUp);
        }
    }

    // 경험치 추가 함수
    public void AddExperience()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddExperience(xpAmountToAdd);
            Debug.Log($"경험치 {xpAmountToAdd} 추가됨. 현재 경험치: {GameManager.Instance.playerExperience}/{GameManager.Instance.experienceToLevelUp}");
        }
        else
        {
            Debug.LogError("GameManager를 찾을 수 없습니다!");
        }
    }

    // 강제 레벨업 함수
    public void ForcePlayerLevelUp()
    {
        if (GameManager.Instance != null)
        {
            // 이미 레벨업 처리 중인지 확인
            if (GameManager.Instance.isProcessingLevelUp)
            {
                Debug.Log("현재 레벨업 처리 중입니다. 기다려주세요.");
                return;
            }

            // 현재 경험치부터 필요 경험치까지의 차이 + 1만큼 경험치 추가
            float expNeeded = GameManager.Instance.experienceToLevelUp - GameManager.Instance.playerExperience + 1;

            // 필요한 경험치가 0보다 큰 경우에만 추가
            if (expNeeded > 0)
            {
                Debug.Log($"레벨업을 위해 {expNeeded} 경험치 추가");
                GameManager.Instance.AddExperience(expNeeded);
            }
            else
            {
                // 이미 경험치가 충분하므로 직접 AddExperience 호출
                Debug.Log("이미 레벨업 조건 충족됨");
                GameManager.Instance.AddExperience(0); // 0 경험치 추가해도 레벨업 체크는 수행됨
            }
        }
        else
        {
            Debug.LogError("GameManager를 찾을 수 없습니다!");
        }
    }
}