using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Stage
{
    public class StagePortalUI : MonoBehaviour
    {
        public Image portalImage;
        public Text stageNumberText;
        public GameObject lockIcon;

        private int stageIndex;
        private StageData stageData;

        public void Setup(StageData data, int index)
        {
            this.stageData = data;
            this.stageIndex = index;

            // 스테이지 번호 표시
            stageNumberText.text = (index + 1).ToString();

            // 포털 이미지가 있는 경우 설정
            if (portalImage != null && data.portalImage != null)
            {
                portalImage.sprite = data.portalImage;
            }

            // 잠금 아이콘 업데이트
            UpdateLockStatus();
        }

        private void UpdateLockStatus()
        {
            // 스테이지 1은 항상 잠금 해제
            bool isUnlocked = (stageIndex == 0) || PlayerPrefs.GetInt("Stage_" + stageIndex + "_Unlocked", 0) == 1;

            // 잠금 아이콘 표시 여부
            if (lockIcon != null)
            {
                lockIcon.SetActive(!isUnlocked);
            }

            // 포털 이미지 투명도 조정
            if (portalImage != null)
            {
                Color color = portalImage.color;
                color.a = isUnlocked ? 1.0f : 0.5f;
                portalImage.color = color;
            }
        }
    }
}