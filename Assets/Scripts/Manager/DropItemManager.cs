using UnityEngine;
using System.Collections.Generic;

public class DropItemManager : Singleton<DropItemManager>
{
    public BackendDataManager.ProbabilityCardV2 probabilityCard
    {
        get
        {
            return probabilityCard;
        }
        set
        {
            if (value != null)
            {
                currentStageDropItemData = BackendDataManager.Instance.GetProbabilityData(value.selectedProbabilityFileId);
            }
        }
    }
    private LitJson.JsonData currentStageDropItemData;

    public bool SetProbabilityCard(BackendDataManager.ProbabilityCardV2 probabilityCard)
    {
        if (probabilityCard == null || Equals(probabilityCard, default))
        {
            return false;
        }
        this.probabilityCard = probabilityCard;
        return true;
    }

    public string DropItem(Vector3 position)
    {
        float random = Random.Range(0.0f, 1.0f);
        float accumulatedProbability = 0.0f;

        for (int i = 0; i < currentStageDropItemData.Count; i++)
        {
            float dropRate = float.Parse(currentStageDropItemData[i]["percent"].ToString()) / 100f;
            accumulatedProbability += dropRate;

            if (random <= accumulatedProbability)
            {
                // 여기서 아이템 생성
                string itemName = currentStageDropItemData[i]["itemName"].ToString();
                if (itemName == "Void")
                {
                }
                else
                {
                    Instantiate(Resources.Load<GameObject>($"Prefabs/In-game/DropItem/{itemName}")).TryGetComponent<Item.DropItem>(out var dropItem);
                    dropItem.transform.position = position;
                }
                return itemName;
            }
        }
        return "Void";
    }

}
