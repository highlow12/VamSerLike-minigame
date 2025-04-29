using UnityEngine;

namespace UI.Stage
{
    [System.Serializable]
    public class StageData
    {
        public string stageName; // �������� �̸�
        public string stageDescription; // �������� ����
        public string sceneName; // �� �̸�
        public Sprite portalImage; // ���� �̹���
        public int difficulty; // ���̵� (1-5)
    }
}