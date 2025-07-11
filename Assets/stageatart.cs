using UnityEngine;

public class stageatart : MonoBehaviour
{
    public int surrentStageIndex = 0; // 현재 선택된 스테이지 인덱스
    public void StartGame()
    {
        StageLoadManager.Instance.LoadSceneAsync("Stage " + surrentStageIndex, true, "LoadingScene");
    }
    private void Start()
    {
        //플레이어 프리퍼런스에서 현재 스테이지 인덱스를 가져옵니다.
        surrentStageIndex = PlayerPrefs.GetInt("CurrentStageIndex", 1);
    }
    public void LoadStageSelectScene()
    {
        StageLoadManager.Instance.LoadSceneAsync("SceneSelect scene", true, "LoadingScene");
    }
}
