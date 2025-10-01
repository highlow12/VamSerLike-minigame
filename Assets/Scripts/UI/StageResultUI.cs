using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageResultUI : MonoBehaviour
{
    UI.Stage.StageDataManager.StageResult stageResult;

    [SerializeField] private TextMeshProUGUI isClearText;
    [SerializeField] private TextMeshProUGUI stagePlayTimeText;
    public void Initialize()
    {
        Debug.Log("StageResultUI: " + UI.Stage.StageDataManager.Instance.hasStageResult + "->hasStageResult");
        if (UI.Stage.StageDataManager.Instance.hasStageResult)
        {
            stageResult = UI.Stage.StageDataManager.Instance.GetCurrentStageResult();
            Debug.Log($"Stage Result - Success: {stageResult.isStageSuccess}, Time: {stageResult.stagePlayTime}");
            // 여기서 UI 요소들을 업데이트합니다.
            isClearText.text = stageResult.isStageSuccess ? "스테이지 성공" : "스테이지 실패";
            stagePlayTimeText.text = $"플레이 시간: {stageResult.stagePlayTime}";
        }
        else
        {
            Debug.LogWarning("No stage result data available.");
        }
    }
}
