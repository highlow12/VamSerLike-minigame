using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UI.Stage
{
    public class HomeScreenManager : MonoBehaviour
    {
        [Header("홈 화면 UI 요소")]
        public Button enterButton; // 입장 버튼
        public Button stageSelectButton; // 스테이지 선택 버튼
        public GameObject homeScreenUI; // 홈 화면 UI 그룹

        [Header("스테이지 포털")]
        public GameObject portalVisual; // 포털 시각 효과
        public Text portalStageNameText; // 포털 스테이지 이름
        public Text portalStageDescriptionText; // 포털 스테이지 설명

        [Header("애니메이션")]
        public float fadeSpeed = 1.0f; // 페이드 속도
        public Animator portalAnimator; // 포털 애니메이터

        // 스테이지 선택 매니저 참조
        private StageSelectionManager stageSelectionManager;

        // 현재 선택된 스테이지
        private int currentStageIndex = 0;

        // 잠금 해제된 스테이지
        private List<bool> stageUnlocked = new List<bool>();

        void Start()
        {
            stageSelectionManager = FindObjectOfType<StageSelectionManager>();

            // 버튼 리스너 설정
            if (enterButton != null)
                enterButton.onClick.AddListener(EnterSelectedStage);

            if (stageSelectButton != null)
                stageSelectButton.onClick.AddListener(OpenStageSelectScreen);

            // 스테이지 초기화
            InitializeStages();

            // 포털 표시 업데이트
            UpdatePortalDisplay();
        }

        private void InitializeStages()
        {
            // 실제 게임에서는 저장 데이터에서 로드할 것
            stageUnlocked = new List<bool>(10); // 총 10개 스테이지 가정

            // 최소한 스테이지 1은 항상 잠금 해제
            stageUnlocked.Add(true);

            // PlayerPrefs 또는 다른 저장 데이터에서 잠금 해제된 스테이지 확인
            for (int i = 1; i < 10; i++)
            {
                bool isUnlocked = PlayerPrefs.GetInt("Stage_" + i + "_Unlocked", 0) == 1;
                stageUnlocked.Add(isUnlocked);
            }

            // 현재 스테이지를 가장 최근에 잠금 해제된 스테이지로 설정
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
            // 스테이지가 잠금 해제되었는지 확인
            if (currentStageIndex < stageUnlocked.Count && stageUnlocked[currentStageIndex])
            {
                // 포털 애니메이션 재생 (있는 경우)
                if (portalAnimator != null)
                {
                    portalAnimator.SetTrigger("Enter");
                    StartCoroutine(LoadStageAfterDelay(1.0f)); // 애니메이션 재생을 위한 지연
                }
                else
                {
                    LoadSelectedStage();
                }
            }
            else
            {
                Debug.Log("스테이지가 잠겨 있습니다!");
                // 여기에 UI 메시지를 표시할 수 있음
            }
        }

        private IEnumerator LoadStageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            LoadSelectedStage();
        }

        private void LoadSelectedStage()
        {
            // 스테이지 씬은 "Stage1", "Stage2" 등으로 이름이 지정됨
            //SceneManager.LoadScene("Stage" + (currentStageIndex + 1));
            //씬빌드 인덱스 사용
            SceneManager.LoadScene(currentStageIndex + 1);
        }
        public void OpenStageSelectScreen()
        {
            // 홈 화면 숨기기
            homeScreenUI.SetActive(false);

            // 스테이지 선택 화면 표시
            if (stageSelectionManager != null)
            {
                stageSelectionManager.ShowStageSelection(currentStageIndex);
            }
        }

        public void ReturnFromStageSelection(int selectedStage)
        {
            // 잠금 해제된 경우 현재 스테이지 업데이트
            if (selectedStage < stageUnlocked.Count && stageUnlocked[selectedStage])
            {
                currentStageIndex = selectedStage;
            }

            // 홈 화면 표시
            homeScreenUI.SetActive(true);

            // 포털 표시 업데이트
            UpdatePortalDisplay();
        }

        private void UpdatePortalDisplay()
        {
            // StageDataManager 또는 유사한 것에서 스테이지 정보를 가정
            StageData stageData = StageDataManager.GetStageData(currentStageIndex);

            if (stageData != null)
            {
                portalStageNameText.text = stageData.stageName;
                portalStageDescriptionText.text = stageData.stageDescription;

                // 스테이지에 따라 포털 시각 효과도 업데이트할 수 있음
            }
        }

        public void StageCleared(int stageIndex)
        {
            // 다음 스테이지 잠금 해제
            if (stageIndex < stageUnlocked.Count - 1)
            {
                stageUnlocked[stageIndex + 1] = true;
                PlayerPrefs.SetInt("Stage_" + (stageIndex + 1) + "_Unlocked", 1);
                PlayerPrefs.Save();

                // 현재 스테이지를 다음 스테이지로 업데이트
                currentStageIndex = stageIndex + 1;
                UpdatePortalDisplay();
            }
        }
    }
}