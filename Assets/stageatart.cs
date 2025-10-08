using UnityEngine;

public class stageatart : MonoBehaviour
{   
    [SerializeField]private int currentStageIndex = 0; // 현재 선택된 스테이지 인덱스
    
    private void Start()
    {
        //플레이어 프리퍼런스에서 현재 스테이지 인덱스를 가져옵니다.
        //currentStageIndex = PlayerPrefs.GetInt("CurrentStageIndex", 1);
    }


    public void StartGame()
    {
        currentStageIndex = UI.Stage.HomeScreenManager.Instance.currentStageIndex;

        StageLoadManager.Instance.LoadSceneAsync("Stage " + (currentStageIndex + 1), true, "LoadingScene");
        StageLoadManager.Instance.AddLoadingCompletedCallbackOnce(() =>
        {
            Debug.Log("***로딩 완료! 게임 시작!***");
            GameManager.Instance.gameState = GameManager.GameState.InGame;
            GameManager.Instance.SetStage(currentStageIndex);

        });
    }
    public void LoadStageSelectScene()
    {
        //StageLoadManager.Instance.LoadSceneAsync("SceneSelect scene", true, "LoadingScene");
    }
}
