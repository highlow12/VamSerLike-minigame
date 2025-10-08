using UnityEngine;

public class stageatart : MonoBehaviour
{
    public int currentStageIndex = 0; // 현재 선택된 스테이지 인덱스
    
    private void Start()
    {
        //플레이어 프리퍼런스에서 현재 스테이지 인덱스를 가져옵니다.
        //currentStageIndex = PlayerPrefs.GetInt("CurrentStageIndex", 1);
    }


    public void StartGame()
    {
        //스테이지 선택 부분에서 저장하고 관리하는게 좋을 듯

        //PlayerPrefs.SetInt("CurrentStageIndex", HomeScreenManger.Instance.currentStageIndex);
        //currentStageIndex = PlayerPrefs.GetInt("CurrentStageIndex", 1);

        StageLoadManager.Instance.LoadSceneAsync("Stage " + currentStageIndex, true, "LoadingScene");
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
