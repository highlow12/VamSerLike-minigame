using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Stage
{
    public class StageSelectionManager : MonoBehaviour
    {
        [Header("UI 요소")]
        public GameObject stageSelectionScreen; // 스테이지 선택 화면
        public Button leftArrowButton; // 왼쪽 화살표 버튼
        public Button rightArrowButton; // 오른쪽 화살표 버튼
        public Button confirmButton; // 확인(선택) 버튼
        public Button backButton; // 뒤로 가기 버튼

        [Header("스테이지 표시")]
        public RectTransform stageContainer; // 스테이지 컨테이너
        public Image stagePortalImage; // 스테이지 포털 이미지
        public Text stageTitleText; // 스테이지 제목 텍스트
        public Text stageDescriptionText; // 스테이지 설명 텍스트
        public CanvasGroup infoPanel; // 정보 패널 캔버스 그룹

        [Header("애니메이션 설정")]
        public float slideSpeed = 0.5f; // 슬라이드 속도
        public float fadeDuration = 0.3f; // 페이드 지속 시간
        public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 슬라이드 곡선

        // 홈 화면 매니저 참조
        private HomeScreenManager homeScreenManager;

        // 현재 표시된 스테이지 인덱스
        private int currentStageIndex = 0;

        // 슬라이딩 애니메이션 활성 플래그
        private bool isSliding = false;

        // 스테이지 데이터 목록
        private List<StageData> stageDataList = new List<StageData>();

        // 스테이지 잠금 상태
        private List<bool> stageUnlocked = new List<bool>();

        void Awake()
        {
            homeScreenManager = FindObjectOfType<HomeScreenManager>();

            // 초기에 선택 화면 숨기기
            if (stageSelectionScreen != null)
                stageSelectionScreen.SetActive(false);

            // 버튼 리스너 설정
            if (leftArrowButton != null)
                leftArrowButton.onClick.AddListener(SlideLeft);

            if (rightArrowButton != null)
                rightArrowButton.onClick.AddListener(SlideRight);

            if (confirmButton != null)
                confirmButton.onClick.AddListener(ConfirmStageSelection);

            if (backButton != null)
                backButton.onClick.AddListener(ReturnToHomeScreen);

            // 스테이지 데이터 로드
            stageDataList = StageDataManager.GetAllStageData();

            // 잠금 해제 상태 초기화
            InitializeUnlockStatus();
        }

        private void InitializeUnlockStatus()
        {
            stageUnlocked = new List<bool>(10); // 10개 스테이지 가정

            // 첫 번째 스테이지는 항상 해금
            stageUnlocked.Add(true);

            // 나머지 스테이지는 PlayerPrefs에서 확인
            for (int i = 1; i < 10; i++)
            {
                bool isUnlocked = PlayerPrefs.GetInt("Stage_" + i + "_Unlocked", 0) == 1;
                stageUnlocked.Add(isUnlocked);
            }
        }

        public void ShowStageSelection(int initialStageIndex)
        {
            currentStageIndex = Mathf.Clamp(initialStageIndex, 0, stageDataList.Count - 1);

            // 선택 화면 표시
            stageSelectionScreen.SetActive(true);

            // 스테이지 정보 업데이트
            UpdateStageDisplay();

            // 네비게이션 버튼 업데이트
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
            // 유효한 범위로 대상 인덱스 제한
            targetStageIndex = Mathf.Clamp(targetStageIndex, 0, stageDataList.Count - 1);

            // 현재와 동일하면 슬라이드 필요 없음
            if (targetStageIndex == currentStageIndex) return;

            // 슬라이딩 애니메이션 시작
            StartCoroutine(SlideAnimation(targetStageIndex));
        }

        private IEnumerator SlideAnimation(int targetStageIndex)
        {
            isSliding = true;

            // 현재 스테이지 정보 페이드 아웃
            StartCoroutine(FadeStageInfo(false));

            // 이동 방향 결정 (-1: 왼쪽, 1: 오른쪽)
            int direction = (targetStageIndex > currentStageIndex) ? 1 : -1;

            // 이미지 슬라이드 애니메이션
            RectTransform imageRect = stagePortalImage.rectTransform;
            Vector2 startPos = imageRect.anchoredPosition;
            Vector2 exitPos = startPos + new Vector2(direction * 1000, 0); // 화면 밖으로 이동

            float elapsedTime = 0;

            // 현재 이미지 슬라이드 아웃
            while (elapsedTime < slideSpeed / 2)
            {
                float t = slideCurve.Evaluate(elapsedTime / (slideSpeed / 2));
                imageRect.anchoredPosition = Vector2.Lerp(startPos, exitPos, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 현재 인덱스 업데이트
            currentStageIndex = targetStageIndex;

            // 이미지 교체 준비
            UpdateStageImage();

            // 반대편에서 시작하도록 위치 설정
            Vector2 enterPos = startPos + new Vector2(-direction * 1000, 0);
            imageRect.anchoredPosition = enterPos;

            // 새 이미지 슬라이드 인
            elapsedTime = 0;
            while (elapsedTime < slideSpeed / 2)
            {
                float t = slideCurve.Evaluate(elapsedTime / (slideSpeed / 2));
                imageRect.anchoredPosition = Vector2.Lerp(enterPos, startPos, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 정확한 위치로 보정
            imageRect.anchoredPosition = startPos;

            // 스테이지 정보 업데이트 및 페이드 인
            UpdateStageInfo();
            StartCoroutine(FadeStageInfo(true));

            // 네비게이션 버튼 업데이트
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
            // 이미지 업데이트
            UpdateStageImage();

            // 텍스트 정보 업데이트
            UpdateStageInfo();
        }

        private void UpdateStageImage()
        {
            // 현재 스테이지 이미지 업데이트
            if (stagePortalImage != null && currentStageIndex < stageDataList.Count)
            {
                StageData stageData = stageDataList[currentStageIndex];
                if (stageData.portalImage != null)
                {
                    stagePortalImage.sprite = stageData.portalImage;
                }

                // 잠긴 스테이지는 어둡게 표시
                bool isUnlocked = stageUnlocked[currentStageIndex];
                Color color = stagePortalImage.color;
                color.a = isUnlocked ? 1.0f : 0.5f;
                stagePortalImage.color = color;
            }
        }

        private void UpdateStageInfo()
        {
            // 스테이지 데이터 매니저에서 스테이지 데이터 획득
            if (currentStageIndex < stageDataList.Count)
            {
                StageData stageData = stageDataList[currentStageIndex];

                if (stageData != null)
                {
                    // 텍스트 업데이트
                    stageTitleText.text = stageData.stageName;
                    stageDescriptionText.text = stageData.stageDescription;
                }
            }
        }

        private void UpdateNavigationButtons()
        {
            // 현재 위치에 따라 네비게이션 버튼 활성화/비활성화
            leftArrowButton.interactable = (currentStageIndex > 0);
            rightArrowButton.interactable = (currentStageIndex < stageDataList.Count - 1);

            // 스테이지 잠금 해제 상태에 따라 확인 버튼 활성화/비활성화
            bool isUnlocked = stageUnlocked[currentStageIndex];
            confirmButton.interactable = isUnlocked;
        }

        private void ConfirmStageSelection()
        {
            // 선택된 스테이지로 홈 화면으로 돌아가기
            ReturnToHomeScreen();
        }

        private void ReturnToHomeScreen()
        {
            // 선택 화면 숨기기
            stageSelectionScreen.SetActive(false);

            // 홈 화면에 선택 알림
            if (homeScreenManager != null)
            {
                homeScreenManager.ReturnFromStageSelection(currentStageIndex);
            }
        }
    }
}