using UnityEngine;

public class StartPanelKeeper : MonoBehaviour
{
    public GameObject startPanel;
    
    // 이 스크립트는 StartPanel을 항상 활성화 상태로 유지하는 역할을 합니다
    
    void Awake()
    {
        // Awake에서 StartPanel을 활성화합니다
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Debug.Log("StartPanel이 Awake에서 활성화되었습니다.");
        }
        else
        {
            Debug.LogWarning("StartPanel이 할당되지 않았습니다!");
        }
    }
    
    void Update()
    {
        // Update에서 StartPanel이 비활성화되었는지 지속적으로 체크합니다
        if (startPanel != null && !startPanel.activeSelf)
        {
            startPanel.SetActive(true);
            Debug.Log("StartPanel이 Update에서 다시 활성화되었습니다.");
        }
    }
    
    // 게임 시작 버튼으로 사용할 공개 메서드
    public void StartGame()
    {
        Debug.Log("게임 시작 버튼이 클릭되었습니다.");
        // 여기에 게임 시작 로직을 추가하세요
        // 예: SceneManager.LoadScene("GameScene");
        
        // StartPanel을 유지하려면 아래 코드를 주석 처리하세요
        // if (startPanel != null)
        // {
        //     startPanel.SetActive(false);
        // }
    }
}
