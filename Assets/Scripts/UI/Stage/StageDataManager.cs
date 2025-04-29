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

        // �������� ������ ���
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

            // �����Ͱ� ��������� �ʱ�ȭ
            if (stages.Count == 0)
            {
                InitializeStageData();
            }
        }

        private void InitializeStageData()
        {
            // �������� ������ �������� �����ͷ� �ʱ�ȭ
            stages.Add(new StageData()
            {
                stageName = "������ ����ȸ��",
                stageDescription = "�Ѷ� ȭ���ߴ� ����ȸ�忡 ����Ը��� ���Ҵ�. ���� ���鸮�� �ֺ�, ���� �Ǹ� ������ ��������.",
                sceneName = "Stage1",
                difficulty = 1
            });

            stages.Add(new StageData()
            {
                stageName = "�ͽŵ鸰 �б�",
                stageDescription = "�ͼ������� ���� ���� �ӿ��� �̻��ϰ� ��Ʋ�� ���� �б��� �����Ѵ�.",
                sceneName = "Stage2",
                difficulty = 1
            });

            stages.Add(new StageData()
            {
                stageName = "������ ����",
                stageDescription = "���� ���밡 ������ ��⹰, �������� ����� ��� �� ������ �ʴ� �ü����� ���� ���Ѻ��� �ִ�.",
                sceneName = "Stage3",
                difficulty = 2
            });

            stages.Add(new StageData()
            {
                stageName = "������ �׸���",
                stageDescription = "����ϰ� �� ������ ����, �Ʒ��� ����������� £�� ����� ���� ��Ų��.",
                sceneName = "Stage4",
                difficulty = 2
            });

            stages.Add(new StageData()
            {
                stageName = "������ ��ö",
                stageDescription = "�޸��� ��ö ��, �������� ������ �ʴ´�. �������� �Ҿ���� ä ������ ���� �ӿ��� ���� ���ؾ� �� ���ΰ�.",
                sceneName = "Stage5",
                difficulty = 3
            });

            stages.Add(new StageData()
            {
                stageName = "������ ȣ��",
                stageDescription = "�߰��ϴ� ���͵��� ���� ������ ȣ���� �ǳʾ� �Ѵ�.",
                sceneName = "Stage6",
                difficulty = 3
            });

            stages.Add(new StageData()
            {
                stageName = "���� â��",
                stageDescription = "ģ���� �Բ� ���ٲ����� �ϸ� ��Ҵ� ���. �׷��� �̰��� �� �̻� ��ſ� ������ ������ �ƴϴ�.",
                sceneName = "Stage7",
                difficulty = 4
            });

            stages.Add(new StageData()
            {
                stageName = "���� ���� ��",
                stageDescription = "�����ϰ� ���� ���� �� �̷ο��� �������� ���� Ż���ض�.",
                sceneName = "Stage8",
                difficulty = 4
            });

            stages.Add(new StageData()
            {
                stageName = "������ �ڵ��� ����",
                stageDescription = "�������� ���� �տ� ��Ÿ�� �Ҿ���� ����� ã�ƾ� �Ѵ�.",
                sceneName = "Stage9",
                difficulty = 5
            });

            stages.Add(new StageData()
            {
                stageName = "���� �帣�� ���",
                stageDescription = "��ħ�� ã�Ե� ģ��, �׸��� ���ſ� �����ߴ� ��� ������� �ٽ� ��Ÿ�� ���� ���´�. ���� �Ʒ� �������� ������ ����, ���ΰ��� �ڽ��� �Ѿƿ� ������ �޾Ƶ��� ���ΰ�, �ƴϸ� �ٽ� ���� ���� ���ΰ�?",
                sceneName = "Stage10",
                difficulty = 5
            });
        }

        // �������� ������ �������� (���� �޼���)
        public static StageData GetStageData(int index)
        {
            if (Instance.stages.Count > index && index >= 0)
            {
                return Instance.stages[index];
            }
            return null;
        }

        // ��� �������� ������ �������� (���� �޼���)
        public static List<StageData> GetAllStageData()
        {
            return Instance.stages;
        }
    }
}