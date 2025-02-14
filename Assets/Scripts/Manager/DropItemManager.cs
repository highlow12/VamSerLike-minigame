using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class DropItemManager : Singleton<DropItemManager>
{
    // <summary>
    // 뒤끝 백엔드에 업로드되어 있는 확률 차트를 클라이언트에서 사용하기 위한 변수
    // 해당 변수 값이 변경되면 해당 확률 차트에 맞는 드랍 아이템 데이터를 불러옴
    // </summary>
    private BackendDataManager.ProbabilityCardV2 _probabilityCard;

    public BackendDataManager.ProbabilityCardV2 probabilityCard
    {
        get
        {
            return _probabilityCard;
        }
        set
        {
            if (value != null)
            {
                _probabilityCard = value;
                currentStageDropItemData = BackendDataManager.Instance.GetProbabilityData(value.selectedProbabilityFileId);
            }
        }
    }
    // <summary>
    // 현재 스테이지의 드랍 아이템 데이터
    // </summary>
    private LitJson.JsonData currentStageDropItemData;

    // <summary>
    // 기본 경험치 드랍 확률
    // </summary>
    public static ProbabilityList defaultExpProbs = new(new List<float> { 0.8f, 0.15f, 0.05f });

    public override void Awake()
    {
        base.Awake();
    }

    // <summary>
    // 해당 클래스 외부에서 확률 차트를 설정하는 함수
    // </summary>
    public bool SetProbabilityCard(BackendDataManager.ProbabilityCardV2 probabilityCard)
    {
        if (probabilityCard == null || Equals(probabilityCard, default))
        {
            return false;
        }
        this.probabilityCard = probabilityCard;
        return true;
    }

    // <summary>
    // 설정된 확률 데이터를 기반으로 아이템을 드랍하는 함수
    // </summary>
    public string DropItem(Vector3 position)
    {
        float random = UnityEngine.Random.Range(0.0f, 1.0f);
        float accumulatedProbability = 0.0f;

        for (int i = 0; i < currentStageDropItemData.Count; i++)
        {
            float dropRate = float.Parse(currentStageDropItemData[i]["percent"].ToString()) / 100f;
            accumulatedProbability += dropRate;

            if (random <= accumulatedProbability)
            {
                string itemId = currentStageDropItemData[i]["itemId"].ToString();
                Item.DropItemType parsedItemId = (Item.DropItemType)Enum.Parse(typeof(Item.DropItemType), itemId);
                string itemName = Enum.GetName(typeof(Item.DropItemType), parsedItemId);
                // 경험치일 경우 추가 로직 실행
                if (parsedItemId == Item.DropItemType.Experience)
                {
                    DropExperience(position);
                }
                else
                {
                    GameObject dropItem = ObjectPoolManager.Instance.GetGo(itemName);
                    dropItem.transform.position = position;
                    // 아이템일 경우에는 경험치도 추가 생성
                    DropExperience(position);
                }
                return itemName;
            }
        }
        return "Void";
    }
    // <summary>
    // 지정된 아이템을 드랍하는 함수
    // </summary>
    public void DropItem(Vector3 position, Item.DropItemType itemType)
    {
        string itemName = Enum.GetName(typeof(Item.DropItemType), itemType);
        // 경험치일 경우 추가 로직 실행
        if (itemType == Item.DropItemType.Experience)
        {
            DropExperience(position);
        }
        else
        {
            GameObject dropItem = ObjectPoolManager.Instance.GetGo(itemName);
            dropItem.transform.position = position;
        }
    }

    // <summary>
    // 확률에 따라 경험치를 생성하는 함수
    // 확률 값을 ProbabilityList로 전달하여 defaultExpProbs를 대체할 수 있음
    // 경험치의 종류의 개수와 ProbabilityList의 크기가 일치해야 함
    // probs의 원소의 값은 자동으로 정규화되어 사용됨
    // ex) DropExperience(Vector3 position, new ProbabilityList(new List<float> { 0.5f, 0.7f, 0.3f }));
    // 위 예시와 같이 사용하여 1레벨 경험치를 33.3%, 2레벨 경험치를 46.7%, 3레벨 경험치를 20% 확률로 생성
    // </summary>
    public void DropExperience(Vector3 position, ProbabilityList probs = null)
    {
        // probs가 null이면 defaultExpProbs로 초기화
        probs ??= defaultExpProbs;
#if UNITY_EDITOR
        DebugConsole.Line line = new()
        {
            text = $"{GameManager.Instance.gameTimer} - Normalized exp probs: {probs.GetElements()}",
            messageType = DebugConsole.MessageType.Local,
            tick = GameManager.Instance.gameTimer
        };
        DebugConsole.Instance.MergeLine(line, "#00FF00");
#endif

        float expRandom = UnityEngine.Random.Range(0.0f, 1.0f);
        float accumulatedProbability = 0.0f;
        Experience experience = ObjectPoolManager.Instance.GetGo("Experience").GetComponent<Experience>();
        for (int i = 0; i < probs.Count; i++)
        {
            float prob = (float)probs[i];
            accumulatedProbability += prob;
            if (expRandom <= accumulatedProbability)
            {
                experience.SetExperienceItemLevel(i + 1);
                experience.transform.position = position;
                return;
            }
        }
    }

    // <summary>
    // 지정된 레벨의 경험치를 생성하는 함수
    // </summary>
    public void DropExperience(Vector3 position, int level)
    {
        Experience experience = ObjectPoolManager.Instance.GetGo("Experience").GetComponent<Experience>();
        experience.SetExperienceItemLevel(level);
        experience.transform.position = position;
    }

    // <summary>
    // 현재 스테이지의 모든 드랍 아이템들을 리턴하는 함수
    // </summary>
    public List<string> GetDropItems()
    {
        if (currentStageDropItemData == null)
        {
            return new List<string>();
        }
        List<string> dropItems = new();
        for (int i = 0; i < currentStageDropItemData.Count; i++)
        {
            string itemId = currentStageDropItemData[i]["itemId"].ToString();
            Item.DropItemType parsedItemId = (Item.DropItemType)Enum.Parse(typeof(Item.DropItemType), itemId);
            string itemName = Enum.GetName(typeof(Item.DropItemType), parsedItemId);
            dropItems.Add(itemName);
        }
        return dropItems;
    }
}
