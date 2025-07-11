using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageManager : MonoBehaviour
{
    public GameObject homeScreen;
    public GameObject stageSelectScreen;

    public Button enterButton;
    public Button stageSelectButton;
    public Button selectStageButton;
    public Button leftArrowButton;
    public Button rightArrowButton;

    public GameObject[] portalImages; // 포털 이미지 (이미 있다고 하셨으니 활용)
    public TextMeshProUGUI stageTitle;
    public TextMeshProUGUI stageDescription;
    public TextMeshProUGUI stageDifficulty;

    private int currentStageIndex = 0;
    private int clearedStageIndex = 0; // 가장 최근에 클리어한 스테이지 인덱스

    // 스테이지 정보를 하드코딩
    private string[] stageTitles = {
        "1. 버려진 무도회장",
        "2. 귀신들린 학교",
        "3. 부패의 둥지",
        "4. 심해의 그림자",
        "5. 끝없는 전철",
        "6. 얼어붙은 호수",
        "7. 지하 창고",
        "8. 벽장 속의 나",
        "9. 잊혀진 자들의 정원",
        "10. 별이 흐르는 언덕"
    };

    private string[] stageDescriptions = {
        "한때 화려했던 무도회장에 고요함만이 남았다. 낡은 샹들리에 주변, 춤의 악몽 속으로 끌려들어간다.",
        "익숙하지만 낯선 교실 속에서 이상하게 뒤틀려 가는 학교를 마주한다.",
        "썩은 악취가 가득한 폐기물, 벌레들이 들끓는 어둠 속 보이지 않는 시선들이 나를 지켜보고 있다.",
        "고요하고 숨 막히는 심해, 아래로 가라앉을수록 짙은 어둠이 나를 삼킨다.",
        "달리는 전철 안, 도착지는 보이지 않는다. 목적지를 잃어버린 채 끝없는 여정 속에서 어디로 향해야 할 것인가.",
        "추격하는 몬스터들을 피해 얼어붙은 호수를 건너야 한다.",
        "친구와 함께 숨바꼭질을 하며 놀았던 장소. 그러나 이곳은 더 이상 즐거운 놀이의 공간이 아니다.",
        "복집하게 얽힌 벽장 속 미로에서 괴물들을 피해 탈출해라.",
        "누군가의 무덤 앞에 나타난 잃어버린 기억을 찾아야 한다.",
        "마침내 찾게된 친구, 그리고 과거에 마주했던 모든 존재들이 다시 나타나 길을 막는다. 별빛 아래 펼쳐지는 최후의 결전, 주인공은 자신이 쫓아온 진실을 받아들일 것인가, 아니면 다시 길을 잃을 것인가?"
    };

    private string[] stageDifficulties = {
        "난이도: ★☆☆☆☆",
        "난이도: ★☆☆☆☆",
        "난이도: ★★☆☆☆",
        "난이도: ★★☆☆☆",
        "난이도: ★★★☆☆",
        "난이도: ★★★☆☆",
        "난이도: ★★★★☆",
        "난이도: ★★★★☆",
        "난이도: ★★★★★",
        "난이도: ★★★★★"
    };

    void Start()
    {
        // PlayerPrefs로부터 클리어 정보 로드
        clearedStageIndex = PlayerPrefs.GetInt("ClearedStageIndex", 0);

        // 버튼 이벤트 설정
        enterButton.onClick.AddListener(EnterStage);
        stageSelectButton.onClick.AddListener(OpenStageSelect);
        selectStageButton.onClick.AddListener(ConfirmStageSelection);
        leftArrowButton.onClick.AddListener(NavigateToPreviousStage);
        rightArrowButton.onClick.AddListener(NavigateToNextStage);

        // 초기 화면 설정
        homeScreen.SetActive(true);
        stageSelectScreen.SetActive(false);

        // 현재 스테이지 표시 업데이트
        UpdateStageDisplay();
    }

    void EnterStage()
    {
        // 현재 선택된 스테이지로 게임 시작
        Debug.Log("스테이지 시작: " + stageTitles[currentStageIndex]);
        // 여기서 실제 스테이지 씬을 로드하거나 게임 오브젝트를 활성화
    }

    void OpenStageSelect()
    {
        homeScreen.SetActive(false);
        stageSelectScreen.SetActive(true);
        UpdateStageDisplay();
    }

    void ConfirmStageSelection()
    {
        homeScreen.SetActive(true);
        stageSelectScreen.SetActive(false);
    }

    void NavigateToPreviousStage()
    {
        if (currentStageIndex > 0)
        {
            currentStageIndex--;
            UpdateStageDisplay();
        }
    }

    void NavigateToNextStage()
    {
        // 클리어한 스테이지까지만 이동 가능
        if (currentStageIndex < clearedStageIndex && currentStageIndex < stageTitles.Length - 1)
        {
            currentStageIndex++;
            UpdateStageDisplay();
        }
    }

    void UpdateStageDisplay()
    {
        // 스테이지 정보 업데이트
        stageTitle.text = stageTitles[currentStageIndex];
        stageDescription.text = stageDescriptions[currentStageIndex];
        stageDifficulty.text = stageDifficulties[currentStageIndex];

        // 포털 이미지 업데이트 (가운데 포털은 현재 스테이지)
        for (int i = 0; i < portalImages.Length; i++)
        {
            if (i < stageTitles.Length) // 존재하는 스테이지만 처리
            {
                bool isVisible = (i == currentStageIndex) || // 현재 선택된 스테이지
                                 (i == currentStageIndex - 1 && currentStageIndex > 0) || // 이전 스테이지
                                 (i == currentStageIndex + 1 && i <= clearedStageIndex); // 다음 스테이지 (클리어한 경우만)

                portalImages[i].SetActive(isVisible);

                // 현재 선택된 스테이지는 더 크게 표시 (크기 조정)
                if (i == currentStageIndex)
                {
                    portalImages[i].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                }
                else
                {
                    portalImages[i].transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                }
            }
        }

        // 화살표 버튼 활성화/비활성화
        leftArrowButton.interactable = (currentStageIndex > 0);
        rightArrowButton.interactable = (currentStageIndex < clearedStageIndex && currentStageIndex < stageTitles.Length - 1);

        // 선택 버튼 활성화
        selectStageButton.interactable = true;
    }

    // 스테이지 클리어 시 호출할 함수
    public void StageClear()
    {
        // 현재 스테이지가 가장 높은 클리어 스테이지라면 업데이트
        if (currentStageIndex == clearedStageIndex)
        {
            clearedStageIndex++;
            PlayerPrefs.SetInt("ClearedStageIndex", clearedStageIndex);
            PlayerPrefs.Save();
        }

        // 홈 화면으로 돌아가면서 다음 스테이지로 설정
        if (currentStageIndex < stageTitles.Length - 1)
        {
            currentStageIndex++;
        }

        homeScreen.SetActive(true);
        stageSelectScreen.SetActive(false);
        UpdateStageDisplay();
    }
}