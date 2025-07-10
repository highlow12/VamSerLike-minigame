using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UI.Stage
{
    public class HomeScreenManager : MonoBehaviour
    {
        [Header("Ȩ ȭ�� UI ���")]
        public Button enterButton; // ���� ��ư
        public Button stageSelectButton; // �������� ���� ��ư
        public GameObject homeScreenUI; // Ȩ ȭ�� UI �׷�

        [Header("�������� ����")]
        public GameObject portalVisual; // ���� �ð� ȿ��
        public Text portalStageNameText; // ���� �������� �̸�
        public Text portalStageDescriptionText; // ���� �������� ����

        [Header("�ִϸ��̼�")]
        public float fadeSpeed = 1.0f; // ���̵� �ӵ�
        public Animator portalAnimator; // ���� �ִϸ�����

        // �������� ���� �Ŵ��� ����
        private StageSelectionManager stageSelectionManager;

        // ���� ���õ� ��������
        private int currentStageIndex = 0;

        // ��� ������ ��������
        private List<bool> stageUnlocked = new List<bool>();

        void Start()
        {
            stageSelectionManager = FindObjectOfType<StageSelectionManager>();

            // ��ư ������ ����
            if (enterButton != null)
                enterButton.onClick.AddListener(EnterSelectedStage);

            if (stageSelectButton != null)
                stageSelectButton.onClick.AddListener(OpenStageSelectScreen);

            // �������� �ʱ�ȭ
            InitializeStages();

            // ���� ǥ�� ������Ʈ
            UpdatePortalDisplay();
        }

        private void InitializeStages()
        {
            // ���� ���ӿ����� ���� �����Ϳ��� �ε��� ��
            stageUnlocked = new List<bool>(10); // �� 10�� �������� ����

            // �ּ��� �������� 1�� �׻� ��� ����
            stageUnlocked.Add(true);

            // PlayerPrefs �Ǵ� �ٸ� ���� �����Ϳ��� ��� ������ �������� Ȯ��
            for (int i = 1; i < 10; i++)
            {
                bool isUnlocked = PlayerPrefs.GetInt("Stage_" + i + "_Unlocked", 0) == 1;
                stageUnlocked.Add(isUnlocked);
            }

            // ���� ���������� ���� �ֱٿ� ��� ������ ���������� ����
            for (int i = stageUnlocked.Count - 1; i >= 0; i--)
            {
                if (stageUnlocked[i])
                {
                    currentStageIndex = i;
                    break;
                }
            }
        }

        public void EnterSelectedStage()
        {
            // ���������� ��� �����Ǿ����� Ȯ��
            if (currentStageIndex < stageUnlocked.Count && stageUnlocked[currentStageIndex])
            {
                // ���� �ִϸ��̼� ��� (�ִ� ���)
                if (portalAnimator != null)
                {
                    portalAnimator.SetTrigger("Enter");
                    StartCoroutine(LoadStageAfterDelay(1.0f)); // �ִϸ��̼� ����� ���� ����
                }
                else
                {
                    LoadSelectedStage();
                }
            }
            else
            {
                Debug.Log("���������� ��� �ֽ��ϴ�!");
                // ���⿡ UI �޽����� ǥ���� �� ����
            }
        }

        private IEnumerator LoadStageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            LoadSelectedStage();
        }

        private void LoadSelectedStage()
        {
            // �������� ���� "Stage1", "Stage2" ������ �̸��� ������
            //SceneManager.LoadScene("Stage" + (currentStageIndex + 1));
            //������ �ε��� ���
            SceneManager.LoadScene(currentStageIndex + 1);
        }
        public void OpenStageSelectScreen()
        {
            // Ȩ ȭ�� �����
            homeScreenUI.SetActive(false);

            // �������� ���� ȭ�� ǥ��
            if (stageSelectionManager != null)
            {
                stageSelectionManager.ShowStageSelection(currentStageIndex);
            }
        }

        public void ReturnFromStageSelection(int selectedStage)
        {
            // ��� ������ ��� ���� �������� ������Ʈ
            if (selectedStage < stageUnlocked.Count && stageUnlocked[selectedStage])
            {
                currentStageIndex = selectedStage;
            }

            // Ȩ ȭ�� ǥ��
            homeScreenUI.SetActive(true);

            // ���� ǥ�� ������Ʈ
            UpdatePortalDisplay();
        }

        private void UpdatePortalDisplay()
        {
            // StageDataManager �Ǵ� ������ �Ϳ��� �������� ������ ����
            StageData stageData = StageDataManager.GetStageData(currentStageIndex);

            if (stageData != null)
            {
                portalStageNameText.text = stageData.stageName;
                portalStageDescriptionText.text = stageData.stageDescription;

                // ���������� ���� ���� �ð� ȿ���� ������Ʈ�� �� ����
            }
        }

        public void StageCleared(int stageIndex)
        {
            // ���� �������� ��� ����
            if (stageIndex < stageUnlocked.Count - 1)
            {
                stageUnlocked[stageIndex + 1] = true;
                PlayerPrefs.SetInt("Stage_" + (stageIndex + 1) + "_Unlocked", 1);
                PlayerPrefs.Save();

                // ���� ���������� ���� ���������� ������Ʈ
                currentStageIndex = stageIndex + 1;
                UpdatePortalDisplay();
            }
        }
    }
}