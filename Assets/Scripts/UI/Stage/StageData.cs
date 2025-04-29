using UnityEngine;

namespace UI.Stage
{
    [System.Serializable]
    public class StageData
    {
        public string stageName; // 스테이지 이름
        public string stageDescription; // 스테이지 설명
        public string sceneName; // 씬 이름
        public Sprite portalImage; // 포털 이미지
        public int difficulty; // 난이도 (1-5)
    }
}