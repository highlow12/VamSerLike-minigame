using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Stage
{
    public class StageSelectionManager : MonoBehaviour
    {
        [Header("UI ���")]
        public GameObject stageSelectionScreen; // �������� ���� ȭ��
        public Button leftArrowButton; // ���� ȭ��ǥ ��ư
        public Button rightArrowButton; // ������ ȭ��ǥ ��ư
        public Button confirmButton; // Ȯ��(����) ��ư
        public Button backButton; // �ڷ� ���� ��ư

        [Header("�������� ǥ��")]
        public RectTransform stageContainer; // �������� �����̳�
        public Image stagePortalImage; // �������� ���� �̹���
        public Text stageTitleText; // �������� ���� �ؽ�Ʈ
        public Text stageDescriptionText; // �������� ���� �ؽ�Ʈ
        public CanvasGroup infoPanel; // ���� �г� ĵ���� �׷�

        [Header("�ִϸ��̼� ����")]
        public float slideSpeed = 0.5f; // �����̵� �ӵ�
        public float fadeDuration = 0.3f; // ���̵� ���� �ð�
        public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // �����̵� �

        // Ȩ ȭ�� �Ŵ��� ����
        private HomeScreenManager homeScreenManager;

        // ���� ǥ�õ� �������� �ε���
        private int currentStageIndex = 0;

        // �����̵� �ִϸ��̼� Ȱ�� �÷���
        private bool isSliding = false;

        // �������� ������ ���
        private List<StageData> stageDataList = new List<StageData>();

        // �������� ��� ����
        private List<bool> stageUnlocked = new List<bool>();

        void Awake()
        {
            homeScreenManager = FindObjectOfType<HomeScreenManager>();

            // �ʱ⿡ ���� ȭ�� �����
            if (stageSelectionScreen != null)
                stageSelectionScreen.SetActive(false);

            // ��ư ������ ����
            if (leftArrowButton != null)
                leftArrowButton.onClick.AddListener(SlideLeft);

            if (rightArrowButton != null)
                rightArrowButton.onClick.AddListener(SlideRight);

            if (confirmButton != null)
                confirmButton.onClick.AddListener(ConfirmStageSelection);

            if (backButton != null)
                backButton.onClick.AddListener(ReturnToHomeScreen);

            // �������� ������ �ε�
            stageDataList = StageDataManager.GetAllStageData();

            // ��� ���� ���� �ʱ�ȭ
            InitializeUnlockStatus();
        }

        private void InitializeUnlockStatus()
        {
            stageUnlocked = new List<bool>(10); // 10�� �������� ����

            // ù ��° ���������� �׻� �ر�
            stageUnlocked.Add(true);

            // ������ ���������� PlayerPrefs���� Ȯ��
            for (int i = 1; i < 10; i++)
            {
                bool isUnlocked = PlayerPrefs.GetInt("Stage_" + i + "_Unlocked", 0) == 1;
                stageUnlocked.Add(isUnlocked);
            }
        }

        public void ShowStageSelection(int initialStageIndex)
        {
            currentStageIndex = Mathf.Clamp(initialStageIndex, 0, stageDataList.Count - 1);

            // ���� ȭ�� ǥ��
            stageSelectionScreen.SetActive(true);

            // �������� ���� ������Ʈ
            UpdateStageDisplay();

            // �׺���̼� ��ư ������Ʈ
            UpdateNavigationButtons();
        }

        public void SlideLeft()
        {
            if (!isSliding && currentStageIndex > 0)
            {
                SlideToStage(currentStageIndex - 1);
            }
        }

        public void SlideRight()
        {
            if (!isSliding && currentStageIndex < stageDataList.Count - 1)
            {
                SlideToStage(currentStageIndex + 1);
            }
        }

        public void SlideToStage(int targetStageIndex)
        {
            // ��ȿ�� ������ ��� �ε��� ����
            targetStageIndex = Mathf.Clamp(targetStageIndex, 0, stageDataList.Count - 1);

            // ����� �����ϸ� �����̵� �ʿ� ����
            if (targetStageIndex == currentStageIndex) return;

            // �����̵� �ִϸ��̼� ����
            StartCoroutine(SlideAnimation(targetStageIndex));
        }

        private IEnumerator SlideAnimation(int targetStageIndex)
        {
            isSliding = true;

            // ���� �������� ���� ���̵� �ƿ�
            StartCoroutine(FadeStageInfo(false));

            // �̵� ���� ���� (-1: ����, 1: ������)
            int direction = (targetStageIndex > currentStageIndex) ? 1 : -1;

            // �̹��� �����̵� �ִϸ��̼�
            RectTransform imageRect = stagePortalImage.rectTransform;
            Vector2 startPos = imageRect.anchoredPosition;
            Vector2 exitPos = startPos + new Vector2(direction * 1000, 0); // ȭ�� ������ �̵�

            float elapsedTime = 0;

            // ���� �̹��� �����̵� �ƿ�
            while (elapsedTime < slideSpeed / 2)
            {
                float t = slideCurve.Evaluate(elapsedTime / (slideSpeed / 2));
                imageRect.anchoredPosition = Vector2.Lerp(startPos, exitPos, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // ���� �ε��� ������Ʈ
            currentStageIndex = targetStageIndex;

            // �̹��� ��ü �غ�
            UpdateStageImage();

            // �ݴ����� �����ϵ��� ��ġ ����
            Vector2 enterPos = startPos + new Vector2(-direction * 1000, 0);
            imageRect.anchoredPosition = enterPos;

            // �� �̹��� �����̵� ��
            elapsedTime = 0;
            while (elapsedTime < slideSpeed / 2)
            {
                float t = slideCurve.Evaluate(elapsedTime / (slideSpeed / 2));
                imageRect.anchoredPosition = Vector2.Lerp(enterPos, startPos, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // ��Ȯ�� ��ġ�� ����
            imageRect.anchoredPosition = startPos;

            // �������� ���� ������Ʈ �� ���̵� ��
            UpdateStageInfo();
            StartCoroutine(FadeStageInfo(true));

            // �׺���̼� ��ư ������Ʈ
            UpdateNavigationButtons();

            isSliding = false;
        }

        private IEnumerator FadeStageInfo(bool fadeIn)
        {
            float startAlpha = fadeIn ? 0 : 1;
            float targetAlpha = fadeIn ? 1 : 0;
            float elapsedTime = 0;

            infoPanel.alpha = startAlpha;

            while (elapsedTime < fadeDuration)
            {
                float t = elapsedTime / fadeDuration;
                infoPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            infoPanel.alpha = targetAlpha;
        }

        private void UpdateStageDisplay()
        {
            // �̹��� ������Ʈ
            UpdateStageImage();

            // �ؽ�Ʈ ���� ������Ʈ
            UpdateStageInfo();
        }

        private void UpdateStageImage()
        {
            // ���� �������� �̹��� ������Ʈ
            if (stagePortalImage != null && currentStageIndex < stageDataList.Count)
            {
                StageData stageData = stageDataList[currentStageIndex];
                if (stageData.portalImage != null)
                {
                    stagePortalImage.sprite = stageData.portalImage;
                }

                // ��� ���������� ��Ӱ� ǥ��
                bool isUnlocked = stageUnlocked[currentStageIndex];
                Color color = stagePortalImage.color;
                color.a = isUnlocked ? 1.0f : 0.5f;
                stagePortalImage.color = color;
            }
        }

        private void UpdateStageInfo()
        {
            // �������� ������ �Ŵ������� �������� ������ ȹ��
            if (currentStageIndex < stageDataList.Count)
            {
                StageData stageData = stageDataList[currentStageIndex];

                if (stageData != null)
                {
                    // �ؽ�Ʈ ������Ʈ
                    stageTitleText.text = stageData.stageName;
                    stageDescriptionText.text = stageData.stageDescription;
                }
            }
        }

        private void UpdateNavigationButtons()
        {
            // ���� ��ġ�� ���� �׺���̼� ��ư Ȱ��ȭ/��Ȱ��ȭ
            leftArrowButton.interactable = (currentStageIndex > 0);
            rightArrowButton.interactable = (currentStageIndex < stageDataList.Count - 1);

            // �������� ��� ���� ���¿� ���� Ȯ�� ��ư Ȱ��ȭ/��Ȱ��ȭ
            bool isUnlocked = stageUnlocked[currentStageIndex];
            confirmButton.interactable = isUnlocked;
        }

        private void ConfirmStageSelection()
        {
            // ���õ� ���������� Ȩ ȭ������ ���ư���
            ReturnToHomeScreen();
        }

        private void ReturnToHomeScreen()
        {
            // ���� ȭ�� �����
            stageSelectionScreen.SetActive(false);

            // Ȩ ȭ�鿡 ���� �˸�
            if (homeScreenManager != null)
            {
                homeScreenManager.ReturnFromStageSelection(currentStageIndex);
            }
        }
    }
}