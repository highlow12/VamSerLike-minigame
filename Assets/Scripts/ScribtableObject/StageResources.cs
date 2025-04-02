using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageResources", menuName = "Scriptable Objects/StageResources")]
public class StageResources : ScriptableObject
{
    [Serializable]
    public class StageData
    {
        public int stageNumber;
        public List<Sprite> mapBackgrounds = new List<Sprite>(); // 여러 맵 배경 스프라이트 목록
        public List<Sprite> obstacleSprites = new List<Sprite>(); // 장애물에 사용할 수 있는 스프라이트 목록
    }
    
    public List<StageData> stages = new List<StageData>();
    
    // 스테이지 번호로 스테이지 데이터 가져오기
    public StageData GetStageData(int stageNumber)
    {
        return stages.Find(stage => stage.stageNumber == stageNumber);
    }
}
