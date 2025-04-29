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

            // �������� ��ȣ ǥ��
            stageNumberText.text = (index + 1).ToString();

            // ���� �̹����� �ִ� ��� ����
            if (portalImage != null && data.portalImage != null)
            {
                portalImage.sprite = data.portalImage;
            }

            // ��� ������ ������Ʈ
            UpdateLockStatus();
        }

        private void UpdateLockStatus()
        {
            // �������� 1�� �׻� ��� ����
            bool isUnlocked = (stageIndex == 0) || PlayerPrefs.GetInt("Stage_" + stageIndex + "_Unlocked", 0) == 1;

            // ��� ������ ǥ�� ����
            if (lockIcon != null)
            {
                lockIcon.SetActive(!isUnlocked);
            }

            // ���� �̹��� ���� ����
            if (portalImage != null)
            {
                Color color = portalImage.color;
                color.a = isUnlocked ? 1.0f : 0.5f;
                portalImage.color = color;
            }
        }
    }
}