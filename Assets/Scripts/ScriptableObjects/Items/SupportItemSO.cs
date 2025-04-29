using UnityEngine;
using System.Collections.Generic; // List 사용을 위해 추가

// Player 스크립트의 enum을 사용하기 위해 추가 (Player.cs가 네임스페이스 밖에 있다고 가정)
// 만약 Player.cs가 특정 네임스페이스 안에 있다면 해당 네임스페이스를 using 해야 합니다.
// 예: using MyGame.Characters;

/// <summary>
/// 지원 아이템의 효과 타입을 정의합니다.
/// </summary>
public enum SupportItemEffectType
{
    StatBoost, // BonusStat 변경
    VisionChange // 플레이어 시야 또는 전역 밝기 변경
    // ... 기타 효과 타입 추가 가능
}

/// <summary>
/// 지원 아이템의 데이터를 정의하는 ScriptableObject 클래스입니다.
/// 레벨업 시 선택지로 제공되며, 영구적인 효과를 부여합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewSupportItem", menuName = "Scriptable Objects/Support Item")]
public class SupportItemSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("아이템의 이름 (UI 표시용)")]
    public string itemName = "새 지원 아이템";
    [Tooltip("아이템의 설명 (UI 표시용)")]
    [TextArea]
    public string description = "아이템 설명";
    [Tooltip("아이템 아이콘 (UI 표시용)")]
    public Sprite icon;
    [Tooltip("아이템의 최대 레벨")]
    public int maxLevel = 5;

    [Header("효과 타입")]
    [Tooltip("이 아이템이 어떤 종류의 효과를 주는지 선택")]
    public SupportItemEffectType effectType = SupportItemEffectType.StatBoost;

    [Header("스탯 효과 (StatBoost)")]
    [Tooltip("효과를 적용할 플레이어 스탯")]
    public Player.BonusStat targetStat = Player.BonusStat.MaxHealth;
    [Tooltip("레벨별 고정값 증가량 (Size는 maxLevel과 같아야 함, Index 0 = 레벨 1 효과)")]
    public List<float> fixedBonusPerLevel = new List<float>();
    [Tooltip("레벨별 비율값 증가량 (Size는 maxLevel과 같아야 함, Index 0 = 레벨 1 효과, 예: 1.1 = 10% 증가)")]
    public List<float> percentageBonusPerLevel = new List<float>();

    [Header("시야 효과 (VisionChange)")]
    [Tooltip("레벨별 시야/밝기 값 (Size는 maxLevel과 같아야 함, Index 0 = 레벨 1 효과)")]
    public List<float> visionValuePerLevel = new List<float>();


    /// <summary>
    /// 특정 레벨까지의 누적 스탯 보너스를 계산합니다. (GameManager의 GetPlayerStatValue에서 사용)
    /// </summary>
    /// <param name="statToGet">계산할 스탯 종류</param>
    /// <param name="typeToGet">계산할 보너스 타입 (Fixed 또는 Percentage)</param>
    /// <param name="currentLevel">현재 아이템 레벨</param>
    /// <returns>누적된 보너스 값 (비율은 곱연산, 고정은 합연산)</returns>
    public float GetCumulativeStatValue(Player.BonusStat statToGet, Player.BonusStatType typeToGet, int currentLevel)
    {
        // 효과 타입이 다르거나, 대상 스탯이 다르거나, 레벨이 0 이하면 기본값 반환
        if (effectType != SupportItemEffectType.StatBoost || targetStat != statToGet || currentLevel <= 0)
        {
            // 비율 보너스의 기본값은 1 (곱셈에 영향 없음), 고정 보너스는 0
            return (typeToGet == Player.BonusStatType.Percentage) ? 1.0f : 0.0f;
        }

        // 반환할 누적 값 초기화 (비율은 1, 고정은 0)
        float cumulativeValue = (typeToGet == Player.BonusStatType.Percentage) ? 1.0f : 0.0f;
        // 적용할 보너스 리스트 선택
        List<float> bonusList = (typeToGet == Player.BonusStatType.Fixed) ? fixedBonusPerLevel : percentageBonusPerLevel;

        // 보너스 리스트 유효성 검사
        if (bonusList == null || bonusList.Count == 0)
        {
             Debug.LogWarning($"{itemName} ({name}): {typeToGet} 보너스 리스트가 비어있거나 null입니다.");
             return cumulativeValue; // 기본값 반환
        }

        // 현재 레벨까지 순회하며 보너스 누적
        // 리스트 크기, maxLevel, currentLevel 중 가장 작은 값까지만 순회하여 오류 방지
        int loopCount = Mathf.Min(currentLevel, bonusList.Count, maxLevel);
        for (int i = 0; i < loopCount; i++)
        {
            if (typeToGet == Player.BonusStatType.Fixed)
            {
                // 고정값은 합산
                cumulativeValue += bonusList[i];
            }
            else // Percentage
            {
                // 비율값은 곱셈으로 누적
                float bonusValue = bonusList[i];
                // 0 또는 음수 값 처리: 곱셈 오류 방지 및 기획 의도 반영
                if (bonusValue > 0)
                {
                     cumulativeValue *= bonusValue;
                } else if (bonusValue == 0) {
                     // 0이면 1로 처리하여 곱셈에 영향 없도록 함 (기획에 따라 0 곱하기 가능)
                     cumulativeValue *= 1.0f;
                     // Debug.LogWarning($"{itemName} ({name}): 레벨 {i+1}의 비율 보너스가 0입니다.");
                } else {
                     // 음수 값 처리 (예: 곱셈에서 제외하고 경고 로그)
                     Debug.LogWarning($"{itemName} ({name}): 레벨 {i+1}의 비율 보너스({bonusValue})가 음수입니다. 곱셈에서 제외됩니다.");
                     // 또는 기획에 따라 다른 처리 (예: 절대값 적용 cumulativeValue *= Mathf.Abs(bonusValue);)
                }
            }
        }

        return cumulativeValue;
    }

    /// <summary>
    /// 특정 레벨의 시야/밝기 값을 가져옵니다. (GameManager의 ApplySupportItemEffect에서 사용)
    /// </summary>
    /// <param name="currentLevel">현재 아이템 레벨</param>
    /// <returns>해당 레벨에 정의된 시야/밝기 값</returns>
    public float GetVisionEffectValue(int currentLevel)
    {
         // 효과 타입이 다르거나 레벨이 0 이하면 기본값 반환
         if (effectType != SupportItemEffectType.VisionChange || currentLevel <= 0)
         {
              // 시야/밝기 효과가 없는 경우 기본값 반환 (GameManager에서 이 값을 어떻게 사용할지 결정 필요)
              // 예를 들어, 0을 반환하고 GameManager에서 기본 밝기/시야 값을 사용하도록 할 수 있음
              return 0f; // 또는 -1f 등 유효하지 않음을 나타내는 값
         }

         // 리스트 유효성 및 레벨 범위 검사
         if (visionValuePerLevel == null || visionValuePerLevel.Count == 0)
         {
              Debug.LogWarning($"{itemName} ({name}): 시야 값 리스트(visionValuePerLevel)가 비어있거나 null입니다.");
              return 0f; // 기본값 반환
         }

         // 요청된 레벨이 정의된 범위를 벗어나는 경우 처리
         if (currentLevel > visionValuePerLevel.Count || currentLevel > maxLevel)
         {
              Debug.LogWarning($"{itemName} ({name}): 요청된 레벨({currentLevel})이 정의된 시야 값 범위(1~{Mathf.Min(visionValuePerLevel.Count, maxLevel)})를 벗어났습니다. 마지막 유효 레벨 값으로 대체합니다.");
              // 마지막 유효 레벨의 값 반환 (리스트 인덱스는 0부터 시작)
              int lastValidIndex = Mathf.Min(visionValuePerLevel.Count - 1, maxLevel - 1);
              return visionValuePerLevel[lastValidIndex];
         }

         // 레벨은 1부터 시작하지만 리스트 인덱스는 0부터 시작하므로 -1
         return visionValuePerLevel[currentLevel - 1];
    }
}
