using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UI.Stage
{
    public class HomeScreenManager : MonoBehaviour
    {
        public static HomeScreenManager Instance { get; private set; }

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
        public Animator portalAnimator; // 포털 애니메이터 (있는 경우): 포털 선택해서 들어갈때 지연시키면서 애니메이션 효과 넣어줌


        // 스테이지 선택 매니저 참조
        [Header("Stage Selection Manager")]
        [SerializeField] private StageSelectionManager stageSelectionManager;

        [Header("stageUnlocked 현황")]
        [SerializeField] private List<bool> stageUnlocked = new List<bool>();



        // 현재 선택된 스테이지
        //wallpaper 씬에서는 이 값만을 사용. stage 로드하면 그때 playerpref에 저장
        //start에서 가장 최근 클리어한 스테이지 인덱스로 초기화됨
        public int currentStageIndex { get; private set; } = 0;
        private int highestClearedStageIndex = -1; // 클리어한 스테이지중 가장 높은 인덱스

        // 잠금 해제된 스테이지


        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.D))
            {
                PlayerPrefs.DeleteAll();
                Debug.Log("StageSelectionManager: PlayerPrefs deleted");
            }
#endif
        }

        void Awake()
        {

            //PlayerPrefs.DeleteAll();
            //Debug.Log("PlayerPrefs has been reset.");

            // 싱글톤 패턴 구현
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeStages();
        }

        void Start()
        {
            //stageSelectionManager = FindAnyObjectByType<StageSelectionManager>();

            // 버튼 리스너 설정
            //if (enterButton != null)
            //    enterButton.onClick.AddListener(EnterSelectedStage);

            //if (stageSelectButton != null)
            //   stageSelectButton.onClick.AddListener(OpenStageSelectScreen);

            // 스테이지 초기화


            // 포털 표시 업데이트
            UpdatePortalDisplay();
        }

        private void InitializeStages()
        {
            // 실제 게임에서는 저장 데이터에서 로드할 것
            //stageUnlocked = new List<bool>(10); // 총 10개 스테이지 가정

            // 최소한 스테이지 1은 항상 잠금 해제
            stageUnlocked[0] = true;

            // PlayerPrefs 또는 다른 저장 데이터에서 잠금 해제된 스테이지 확인
            for (int i = 1; i < 10; i++)
            {
                bool isUnlocked = PlayerPrefs.GetInt("Stage_" + i + "_Unlocked", 0) == 1;
                stageUnlocked[i] = isUnlocked;
                //Debug.Log("HomeScreenManager: stage index " + i + ": isUnlocked=>" + stageUnlocked[i]);
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
            //SceneManager.LoadScene(currentStageIndex + 1);
            StageLoadManager.Instance.LoadSceneAsync("Stage " + currentStageIndex);//로딩창과 함께 이동
        }
        public void OpenStageSelectScreen()
        {
            // 홈 화면 숨기기
            //homeScreenUI.SetActive(false);//현재 코드상 이러면 캔버스 자체가 비활성화돼서 ui가 아무것도 안보임;;

            // 스테이지 선택 화면 표시
            if (stageSelectionManager != null)
            {
                stageSelectionManager.ShowStageSelection(currentStageIndex);
            }
            else
            {
                Debug.LogError("StageSelectionManager not found!");
            }
        }

        public void ReturnFromStageSelection(int selectedStage)
        {
            Debug.Log((selectedStage < stageUnlocked.Count && stageUnlocked[selectedStage]) + " is unlocked.");
            // 잠금 해제된 경우 현재 스테이지 업데이트
            if (selectedStage < stageUnlocked.Count && stageUnlocked[selectedStage])
            {
                currentStageIndex = selectedStage;
                //SaveCurrentStageIndex(currentStageIndex);
                Debug.Log("HomeScreenManager: returned from stage selection. currentStageIndex updated to: " + currentStageIndex);
            }

            // 홈 화면 표시
            //homeScreenUI.SetActive(true);

            // 포털 표시 업데이트
            UpdatePortalDisplay();
        }

        private void UpdatePortalDisplay()
        {
            // StageDataManager 또는 유사한 것에서 스테이지 정보를 가정
            StageData stageData = StageDataManager.GetStageData(currentStageIndex);

            if (stageData != null)
            {
                //portalStageNameText.text = stageData.stageName;//portalStageNameText가 null인 상태로 시작하는듯. 근데 왜 게임 시작할때 이 코드가 실행되지?=>처음부터 스테이지 정보 받아와서 넣어주려고
                //portalStageDescriptionText.text = stageData.stageDescription;

                // 스테이지에 따라 포털 시각 효과도 업데이트할 수 있음

                stageSelectButton.image.sprite = stageData.portalImage;
                Debug.Log("Updated portal visuals for stage: " + stageData.portalImage);
            }
        }

        public void StageCleared(int stageIndex)
        {
            Debug.Log("HomeScreenManager: StageCelared called");
            Debug.Log("HomeScreenManager: stageUnlocked.Count: " + stageUnlocked.Count);
            UpdatePortalDisplay();
            if (stageIndex <= highestClearedStageIndex)//이미 클리어했던 스테이지면 무시
            {
                Debug.Log("HomeScreenManager: already cleared. " + stageIndex + " <= " + highestClearedStageIndex);
                return;
            }
            highestClearedStageIndex = stageIndex;
            // 다음 스테이지 잠금 해제
            if (stageIndex < stageUnlocked.Count - 1)
            {
                stageUnlocked[stageIndex + 1] = true;
                Debug.Log("HomeScreenManager: stage celared=>" + stageIndex +
                 ", next stage unlocked=>" + stageUnlocked[stageIndex + 1]);
                PlayerPrefs.SetInt("Stage_" + (stageIndex + 1) + "_Unlocked", 1);
                PlayerPrefs.Save();

                // 현재 스테이지를 다음 스테이지로 업데이트
                currentStageIndex = stageIndex + 1;
                //SaveCurrentStageIndex(currentStageIndex);
                Debug.Log("Stage " + (stageIndex + 1) + " unlocked!");

            }
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void SaveCurrentStageIndex(int cur_value)
        {
            PlayerPrefs.SetInt("CurrentStageIndex", cur_value);
            PlayerPrefs.Save();
        }
        int LoadCurrentStageIndex()
        {
            return PlayerPrefs.GetInt("CurrentStageIndex", 0); // 기본값 0
        }
    }
}