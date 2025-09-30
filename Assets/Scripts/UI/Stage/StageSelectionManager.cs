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
        public List<Image> stagePortalImageList; // 스테이지 포털 이미지 리스트 //코드가 빵꾸 나있어서 일단 임시로 코드 짜봄
        public Image stagePortalImage; // 스테이지 포털 이미지
        public Text stageTitleText; // 스테이지 제목 텍스트
        public Text stageDescriptionText; // 스테이지 설명 텍스트
        public CanvasGroup infoPanel; // 정보 패널 캔버스 그룹

        [Header("애니메이션 설정")]
        public float slideSpeed = 0.5f; // 슬라이드 속도
        public float fadeDuration = 0.3f; // 페이드 지속 시간
        public float StageImagesOffset = 510f; // 스테이지 이미지 간격
        //public GameObject stagesParent; // 스테이지 이미지들의 부모 오브젝트
        public float normalWidthHeight = 400f;
        public float selectedWidthHeight = 500f;
        public float sizeUpSpeed = 3f; // 크기 증가 속도
        public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 슬라이드 곡선

        private Vector3[] targetPositions;

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
            Debug.Log("StageSelectionManager: Awake called");
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
            if (stageDataList.Count <= 0)
            {
                Debug.LogError("StageSelectionManager: No stage data available!");
            }

            // 잠금 해제 상태 초기화
            InitializeUnlockStatus();

            targetPositions = new Vector3[stagePortalImageList.Count];
            UpdateTargetPositions();
        }

        private void InitializeUnlockStatus()
        {
            //stageUnlocked = new List<bool>(10); // 10개 스테이지 가정//add로 구현할꺼면 10개 미리 생성하면 안됨

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

            // 선택 화면 표시
            gameObject.SetActive(true);//맨 처음에 활성화 시켜면서 Awake로 초기화를 해줘야함..? 왜 Awake가 작동 안하지?=>오 현재 코드상 작동 하는듯

            //Awake가 작동 안하는건지, stageDataList가 비어있는 현상 발생.
            stageDataList = StageDataManager.GetAllStageData();

            Debug.Log("StageSelectionManager: ShowStageSelection called with index " + initialStageIndex);
            if (stageDataList.Count <= 0)
            {
                Debug.LogError("StageSelectionManager: No stage data available!");
                return;
            }
            currentStageIndex = Mathf.Clamp(initialStageIndex, 0, stageDataList.Count - 1);//여기서 0값을 -1로 변환해줌. 버그=>count가 0이라 -1이 되는듯
            Debug.Log("StageSelectionManager: currentStageIndex set to " + currentStageIndex);
            Debug.Log("StageSelectionManager: stageDataList count is " + stageDataList.Count);


            // 스테이지 정보 업데이트
            //UpdateStageDisplay();
            SlideToStage(currentStageIndex);//현재 스테이지의 크기를 키워놓음//또한 현재 스테이지 인덱스에 맞게 위치도 조정됨

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

            // 현재와 동일하면 슬라이드 필요 없음// 초기에 사이즈 조정해놓으려면 필요해짐
            //if (targetStageIndex == currentStageIndex) return;

            // 슬라이딩 애니메이션 시작
            StartCoroutine(SlideAnimation(targetStageIndex));
        }

        void UpdateTargetPositions()
        {
            //초기 index가 0이라고 했을 때, 0번째는 좌표값이 0임
            for (int i = 0; i < stagePortalImageList.Count; i++)
            {
                // Calculate the target position based on currentStageIndex
                float xPos = 250 + (i - currentStageIndex) * StageImagesOffset;//250은 0번째 이미지의 초기 위치
                targetPositions[i] = new Vector3(xPos, stagePortalImageList[i].rectTransform.anchoredPosition.y, 0);
            }
        }

        private IEnumerator SlideAnimation(int targetStageIndex)
        {
            currentStageIndex = targetStageIndex;
            UpdateTargetPositions();//index 변경 후에 위치 업데이트

            

            Debug.Log("StageSelectionManager: Starting slide animation to index " + targetStageIndex);
            isSliding = true;

            // 현재 스테이지 정보 페이드 아웃
            StartCoroutine(FadeStageInfo(false));

            // 이동 방향 결정 (-1: 왼쪽, 1: 오른쪽)
            int direction = (targetStageIndex > currentStageIndex) ? 1 : -1;

            /* // 이미지 슬라이드 애니메이션
             RectTransform imageRect = stagePortalImage.rectTransform;//현재 선택된 스테이지의 이미지의 transform을 가져와서 움직인다고 생각하면 되겠네//초기화 안됨..
             Vector2 startPos = imageRect.anchoredPosition;
             Vector2 exitPos = startPos + new Vector2(direction * 1000, 0); // 화면 밖으로 이동

             float elapsedTime = 0;

             // 현재 이미지 슬라이드 아웃
             while (elapsedTime < slideSpeed / 2)
             {
                 Debug.Log("StageSelectionManager: Sliding out, elapsedTime = " + elapsedTime);
                 float t = slideCurve.Evaluate(elapsedTime / (slideSpeed / 2));
                 imageRect.anchoredPosition = Vector2.Lerp(startPos, exitPos, t);
                 Debug.Log("StageSelectionManager: Sliding out, imageRect.anchoredPosition = " + imageRect.anchoredPosition + "");

                 elapsedTime += Time.deltaTime;
                 yield return null;
             }

             */


            //HorizontalLayoutGroup layoutGroup = stagesParent.GetComponent<HorizontalLayoutGroup>();

            // 끄기
            //layoutGroup.enabled = false;//끄지 않으면 크기와 위치를 변경할 수 없음.

            // 대수술 진행중. StageSelectManager의 코드를 바탕으로 SlideAnimation 구현을 변경. 그 이후에 stagePortalImage 변수 필요 없어질듯
            float elapsedTime = 0;

            // 현재 이미지 슬라이드 아웃
            while (elapsedTime < slideSpeed)
            {
                //이미지 크기 수정 코드
                stagePortalImage.rectTransform.sizeDelta = Vector2.Lerp(
                    new Vector2(selectedWidthHeight, selectedWidthHeight),
                    new Vector2(normalWidthHeight, normalWidthHeight),
                    elapsedTime * sizeUpSpeed
                );

                stagePortalImageList[currentStageIndex].rectTransform.sizeDelta = Vector2.Lerp(
                    new Vector2(normalWidthHeight, normalWidthHeight),
                    new Vector2(selectedWidthHeight, selectedWidthHeight),
                    elapsedTime * sizeUpSpeed
                );






                for (int i = 0; i < stagePortalImageList.Count; i++)
                {
                    // Check the distance to the target position
                    if (Vector3.Distance(stagePortalImageList[i].rectTransform.anchoredPosition, targetPositions[i]) < 0.01f)
                    {
                        // Snap to the target position if close enough
                        stagePortalImageList[i].rectTransform.anchoredPosition = targetPositions[i];
                    }
                    else
                    {
                        // Otherwise, interpolate smoothly
                        stagePortalImageList[i].rectTransform.anchoredPosition = Vector3.Lerp(
                            stagePortalImageList[i].rectTransform.anchoredPosition,
                            targetPositions[i],
                            elapsedTime
                        );
                    }
                }
                elapsedTime += Time.deltaTime;
                //Debug.Log("StageSelectionManager: Sliding out, elapsedTime = " + elapsedTime);
                yield return null;
            }

            for (int i = 0; i < stagePortalImageList.Count; i++)
            {
                Debug.Log("StageSelectionManager: Target position for image " + i + " = " + targetPositions[i]);
                Debug.Log("StageSelectionManager: Sliding, " + i + ": imageRect.rectTransform.anchoredPosition = " + stagePortalImageList[i].rectTransform.anchoredPosition);
                stagePortalImageList[i].rectTransform.anchoredPosition = targetPositions[i];
            }

            // 다시 켜기
            //layoutGroup.enabled = true;

            // 현재 인덱스 업데이트

            // 이미지 교체 준비
            //UpdateStageImage();

            /* // 반대편에서 시작하도록 위치 설정
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
 */


            // 스테이지 정보 업데이트 및 페이드 인
            //UpdateStageInfo();
            UpdateStageDisplay();//현재 코드상 통합해서 display 관리 가능해짐
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
        if (currentStageIndex < stageDataList.Count)//왜 stagePortalImage가 null이 아닐때 작동해야함?=>일단 빼봄
            {
                StageData stageData = stageDataList[currentStageIndex];
                /*if (stageData.portalImage != null)
                {
                    stagePortalImage.sprite = stageData.portalImage;
                }*/
                //현재 구조상 stageData에서 이미지 받아올 필요 없음.//크기 조정하려면 이제 필요함
                stagePortalImage = stagePortalImageList[currentStageIndex];

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
                Debug.Log("StageSelectionManager: currentStageIndex == "+currentStageIndex);
                StageData stageData = stageDataList[currentStageIndex];//임시 오류 체크용 주석. stageDataList[0]에서 오류가 발생한듯

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
            //leftArrowButton.interactable = (currentStageIndex > 0);
            leftArrowButton.gameObject.SetActive(currentStageIndex > 0);//기획서상 화살표 자체가 사라져야함
            //rightArrowButton.interactable = (currentStageIndex < stageDataList.Count - 1);
            rightArrowButton.gameObject.SetActive(currentStageIndex < stageDataList.Count - 1);//기획서상 화살표 자체가 사라져야함

            // 스테이지 잠금 해제 상태에 따라 확인 버튼 활성화/비활성화
            bool isUnlocked = stageUnlocked[currentStageIndex];
            confirmButton.interactable = isUnlocked;
            //rightArrowButton.interactable = isUnlocked;//현재 스테이지가 비활성화 상태이면 왼쪽은 활성화 상태여야하고, 오른쪽으로 못가야함.
            rightArrowButton.gameObject.SetActive(isUnlocked);//기획서상 화살표 자체가 사라져야함
        }

        private void ConfirmStageSelection()
        {
            // 선택된 스테이지로 홈 화면으로 돌아가기
            ReturnToHomeScreen();
        }

        public void ReturnToHomeScreen()
        {
            Debug.Log("StageSelectionManager: Returning to home screen with selected stage " + currentStageIndex);
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