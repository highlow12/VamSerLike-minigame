using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Stage
{
    public class StageDataManager : MonoBehaviour
    {
        private static StageDataManager _instance;

        public static StageDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<StageDataManager>();

                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("StageDataManager");
                        _instance = obj.AddComponent<StageDataManager>();
                        DontDestroyOnLoad(obj);
                    }
                }
                return _instance;
            }
        }

        // 스테이지 데이터 목록
        [SerializeField]
        private List<StageData> stages = new List<StageData>();

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 데이터가 비어있으면 초기화
            if (stages.Count == 0)
            {
                InitializeStageData();
            }
        }

        private void InitializeStageData()
        {
            // 문서에서 추출한 스테이지 데이터로 초기화
            stages.Add(new StageData()
            {
                stageName = "버려진 무도회장",
                stageDescription = "한때 화려했던 무도회장에 고요함만이 남았다. 낡은 샹들리에 주변, 춤의 악몽 속으로 끌려들어간다.",
                sceneName = "Stage1",
                difficulty = 1
            });

            stages.Add(new StageData()
            {
                stageName = "귀신들린 학교",
                stageDescription = "익숙하지만 낯선 교실 속에서 이상하게 뒤틀려 가는 학교를 마주한다.",
                sceneName = "Stage2",
                difficulty = 1
            });

            stages.Add(new StageData()
            {
                stageName = "부패의 둥지",
                stageDescription = "썩은 악취가 가득한 폐기물, 벌레들이 들끓는 어둠 속 보이지 않는 시선들이 나를 지켜보고 있다.",
                sceneName = "Stage3",
                difficulty = 2
            });

            stages.Add(new StageData()
            {
                stageName = "심해의 그림자",
                stageDescription = "고요하고 숨 막히는 심해, 아래로 가라앉을수록 짙은 어둠이 나를 삼킨다.",
                sceneName = "Stage4",
                difficulty = 2
            });

            stages.Add(new StageData()
            {
                stageName = "끝없는 전철",
                stageDescription = "달리는 전철 안, 도착지는 보이지 않는다. 목적지를 잃어버린 채 끝없는 여정 속에서 어디로 향해야 할 것인가.",
                sceneName = "Stage5",
                difficulty = 3
            });

            stages.Add(new StageData()
            {
                stageName = "얼어붙은 호수",
                stageDescription = "추격하는 몬스터들을 피해 얼어붙은 호수를 건너야 한다.",
                sceneName = "Stage6",
                difficulty = 3
            });

            stages.Add(new StageData()
            {
                stageName = "지하 창고",
                stageDescription = "친구와 함께 숨바꼭질을 하며 놀았던 장소. 그러나 이곳은 더 이상 즐거운 놀이의 공간이 아니다.",
                sceneName = "Stage7",
                difficulty = 4
            });

            stages.Add(new StageData()
            {
                stageName = "벽장 속의 나",
                stageDescription = "복집하게 얽힌 벽장 속 미로에서 괴물들을 피해 탈출해라.",
                sceneName = "Stage8",
                difficulty = 4
            });

            stages.Add(new StageData()
            {
                stageName = "잊혀진 자들의 정원",
                stageDescription = "누군가의 무덤 앞에 나타난 잃어버린 기억을 찾아야 한다.",
                sceneName = "Stage9",
                difficulty = 5
            });

            stages.Add(new StageData()
            {
                stageName = "별이 흐르는 언덕",
                stageDescription = "마침내 찾게된 친구, 그리고 과거에 마주했던 모든 존재들이 다시 나타나 길을 막는다. 별빛 아래 펼쳐지는 최후의 결전, 주인공은 자신이 쫓아온 진실을 받아들일 것인가, 아니면 다시 길을 잃을 것인가?",
                sceneName = "Stage10",
                difficulty = 5
            });
        }

        // 스테이지 데이터 가져오기 (정적 메서드)
        public static StageData GetStageData(int index)
        {
            if (Instance.stages.Count > index && index >= 0)
            {
                return Instance.stages[index];
            }
            return null;
        }

        // 모든 스테이지 데이터 가져오기 (정적 메서드)
        public static List<StageData> GetAllStageData()
        {
            return Instance.stages;
        }
    }
}