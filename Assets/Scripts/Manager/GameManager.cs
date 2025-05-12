using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        InGame,
        Pause,
        GameOver
    }

    public delegate void OnPlayerLevelChanged();
    public event OnPlayerLevelChanged onPlayerLevelChanged;
    public delegate void OnGamePaused();
    public event OnGamePaused onGamePaused;

    public Player playerScript;
    public Light2D playerLight;
    public Light2D globalLight;
    public PlayerMove player;
    public PlayerAttack playerAttack;
    public GameState gameState
    {
        get => _GameState;
        set
        {
            _GameState = value;
        }
    }
    private GameState _GameState;
    public List<Player.PlayerBuffEffect> playerBuffEffects = new();
    public List<Player.PlayerBonusStat> playerBonusStats = new();
    public JsonData playerAssetData;
    public long gameTimer;
    public int currentStage = 0;
    public float dragDistanceMultiplier = 1.0f;
    public float dragSpeedMultiplier = 1.0f;
    public float playerExperienceMultiplier = 1.0f;
    public float experienceToLevelUp = 100;
    public float playerExperience = 0;
    public long playerLevel
    {
        get => _playerLevel;
        set
        {
            if (value > 1)
            {
                onPlayerLevelChanged?.Invoke();
            }
            _playerLevel = value;
        }
    }
    private long _playerLevel = 1;
    public static bool IsGamePaused
    {
        get { return Instance._isGamePaused; }
        set
        {
            Instance._isGamePaused = value;
            Instance.onGamePaused?.Invoke();
        }
    }
    private bool _isGamePaused = false;

    // 지원 아이템 레벨 관리용 Dictionary
    public Dictionary<SupportItemSO, int> supportItemLevels = new Dictionary<SupportItemSO, int>();

    public override void Awake()
    {
        base.Awake();
        onGamePaused += () =>
        {
            Time.timeScale = IsGamePaused ? 0 : 1;
        };

        SetGameState(GameState.InGame);
        playerAssetData = BackendDataManager.Instance.GetUserAssetData();

        InitializeSupportItems(); // 지원 아이템 초기화 (테스트용)

#if UNITY_EDITOR
        // Check if DebugGUI_GameManager exists on this object and add it if not
        //if (gameObject.GetComponent<DebugGUI_GameManager>() == null)
        //{
        //    gameObject.AddComponent<DebugGUI_GameManager>().enabled = true;
        //    Debug.Log("DebugGUI_GameManager was automatically added to GameManager object");
        //}
        SetStage(SceneManager.GetActiveScene().buildIndex); // 현재 씬의 빌드 인덱스를 사용하여 스테이지 설정
#endif
    }

    // 테스트용 지원 아이템 초기화 함수
    private void InitializeSupportItems()
    {
        // 예시: 모든 SupportItemSO를 로드하여 레벨 1로 초기화
        var allSupportItems = Resources.LoadAll<SupportItemSO>("ScriptableObjects/SupportItems");
        foreach (var item in allSupportItems)
        {
            if (!supportItemLevels.ContainsKey(item))
            {
                supportItemLevels.Add(item, 0); // 초기 레벨 0 또는 1로 설정
            }
        }
        Debug.Log($"Initialized {supportItemLevels.Count} support items.");
    }

    // 지원 아이템 레벨업 함수 (UI 버튼 클릭 시 호출될 함수)
    public bool LevelUpSupportItem(SupportItemSO itemToLevelUp)
    {
        if (itemToLevelUp == null) return false;

        if (!supportItemLevels.ContainsKey(itemToLevelUp))
        {
            // 아직 획득하지 않은 아이템이라면 레벨 1로 추가 (또는 다른 로직)
            supportItemLevels.Add(itemToLevelUp, 1);
            Debug.Log($"{itemToLevelUp.itemName} 획득! (Level 1)");
            ApplySupportItemEffect(itemToLevelUp, 1); // 효과 즉시 적용
            return true;
        }
        else
        {
            int currentLevel = supportItemLevels[itemToLevelUp];
            if (currentLevel < itemToLevelUp.maxLevel)
            {
                int newLevel = currentLevel + 1;
                supportItemLevels[itemToLevelUp] = newLevel;
                Debug.Log($"{itemToLevelUp.itemName} 레벨 업! (Level {newLevel})");
                ApplySupportItemEffect(itemToLevelUp, newLevel); // 효과 즉시 적용
                // TODO: 레벨업 시 필요한 추가 로직 (예: UI 업데이트 알림)
                return true;
            }
            else
            {
                Debug.Log($"{itemToLevelUp.itemName}은(는) 이미 최대 레벨입니다.");
                return false;
            }
        }
    }

    // 지원 아이템 효과 적용 함수 (레벨업 시 호출)
    private void ApplySupportItemEffect(SupportItemSO item, int newLevel)
    {
        if (item == null)
        {
            Debug.LogError("ApplySupportItemEffect: 아이템이 null입니다!");
            return;
        }

        Debug.Log($"[GameManager] {item.itemName} 효과 적용 시작 - 레벨: {newLevel}, 효과 타입: {item.effectType}");

        // 시야/밝기 아이템 효과 즉시 적용 (이름 기반 판단 대신 effectType 사용)
        if (item.effectType == SupportItemEffectType.VisionChange)
        {
            float visionValue = item.GetVisionEffectValue(newLevel);
            Debug.Log($"[GameManager] 비전 효과 값: {visionValue}");

            // 아이템 이름으로 구분하는 대신 특정 플래그를 사용하거나 아이템 설정을 통해 결정할 수 있습니다.
            if (item.itemName.Contains("손전등") || item.itemName.Contains("flashlight") || item.itemName.Contains("Flashlight"))
            {
                if (playerLight != null)
                {
                    Debug.Log($"[GameManager] playerLight 적용 전: {playerLight.pointLightOuterRadius} -> 적용 후: {visionValue}");
                    playerLight.pointLightOuterRadius = visionValue;
                }
                else
                {
                    Debug.LogError("[GameManager] playerLight가 null입니다! Inspector에서 할당해주세요.");
                }
            }
            else if (item.itemName.Contains("야간투시") || item.itemName.Contains("night") || item.itemName.Contains("Night"))
            {
                if (globalLight != null)
                {
                    Debug.Log($"[GameManager] globalLight 적용 전: {globalLight.intensity} -> 적용 후: {visionValue}");
                    globalLight.intensity = visionValue;
                    globalLight.gameObject.SetActive(true); // 확실하게 활성화
                }
                else
                {
                    Debug.LogError("[GameManager] globalLight가 null입니다! Inspector에서 할당해주세요.");
                }
            }
        }

        // 스탯 관련 효과는 GetPlayerStatValue에서 계산되므로 여기서는 별도 적용 불필요
    }

    // 특정 지원 아이템의 현재 레벨 가져오기
    public int GetSupportItemLevel(SupportItemSO item)
    {
        if (supportItemLevels.TryGetValue(item, out int level))
        {
            return level;
        }
        return 0; // 획득하지 않았으면 0 반환
    }

    // Set game state
    public void SetGameState(GameState newState)
    {
        gameState = newState;
        switch (gameState)
        {
            default:
                break;
        }
    }

    public bool SetStage(int stageNumber)
    {
        List<string> existingDropItems = DropItemManager.Instance.GetDropItems();
        if (existingDropItems.Count > 0)
        {
            foreach (string dropItem in existingDropItems)
            {
                ObjectPoolManager.Instance.UnregisterObjectPool(dropItem);
            }
        }
        bool result = DropItemManager.Instance.SetProbabilityTitle($"Stage{stageNumber}_DropItemProb");

        if (!result)
        {
#if UNITY_EDITOR
            DebugConsole.Line errorLog = new()
            {
                text = $"[{gameTimer}] Failed to set probability card for stage {stageNumber}",
                messageType = DebugConsole.MessageType.Local,
                tick = gameTimer
            };
            DebugConsole.Instance.MergeLine(errorLog, "#FF0000");
#endif
        }
        else
        {
            currentStage = stageNumber;
            List<string> dropItems = DropItemManager.Instance.GetDropItems();
            foreach (string dropItem in dropItems)
            {
                string path = $"Prefabs/In-game/DropItem/{dropItem}";
                GameObject dropItemPrefab = Resources.Load<GameObject>(path);
                if (dropItemPrefab == null)
                {
#if UNITY_EDITOR
                    DebugConsole.Line errorLog = new()
                    {
                        text = $"[{gameTimer}] Failed to load drop item prefab {dropItem}",
                        messageType = DebugConsole.MessageType.Local,
                        tick = gameTimer
                    };
                    DebugConsole.Instance.MergeLine(errorLog, "#FF0000");
#endif
                    continue;

                }
                else
                {
                    ObjectPoolManager.Instance.RegisterObjectPool(dropItem, dropItemPrefab, null, 10);
                }
            }
        }
        return result;
    }

    void FixedUpdate()
    {
        if (gameState == GameState.InGame)
        {
            foreach (Player.PlayerBuffEffect buffEffect in playerBuffEffects)
            {
                if (buffEffect.endTime <= gameTimer)
                {
                    RemoveBuffEffect(buffEffect);
                }
            }
            gameTimer += 1;
        }
    }

    public void AddBonusStat(Player.PlayerBonusStat bonusStat)
    {
        if (Equals(bonusStat.playerBuffEffect, default))
        {
            Debug.LogError("PlayerBuffEffect is null");
            return;
        }
        playerBonusStats.Add(bonusStat);
        playerBuffEffects.Add(bonusStat.playerBuffEffect);
    }

    // 스탯 계산 시 지원 아이템 레벨 반영
    public float GetPlayerStatValue(Player.BonusStat statEnum, float baseValue)
    {
        float result = baseValue;

        // 1. 기간제 버프/디버프 적용 (기존 playerBonusStats)
        foreach (Player.PlayerBonusStat bonusStat in playerBonusStats)
        {
            if (bonusStat.bonusStat == statEnum)
            {
                switch (bonusStat.bonusStatType)
                {
                    case Player.BonusStatType.Fixed:
                        result += bonusStat.value;
                        break;
                    case Player.BonusStatType.Percentage:
                        result *= bonusStat.value; // 곱연산 주의
                        break;
                }
            }
        }

        // 2. 지원 아이템 레벨 기반 영구 스탯 적용
        foreach (var kvp in supportItemLevels)
        {
            SupportItemSO item = kvp.Key;
            int level = kvp.Value;

            if (level > 0)
            {
                 // 고정값 효과 누적 적용
                 float fixedBonus = item.GetCumulativeStatValue(statEnum, Player.BonusStatType.Fixed, level);
                 result += fixedBonus;

                 // 비율값 효과 누적 적용 (곱연산)
                 float percentageBonus = item.GetCumulativeStatValue(statEnum, Player.BonusStatType.Percentage, level);
                 result *= percentageBonus;
            }
        }

        // 최종 값 반환 (필요시 최소/최대값 제한 추가)
        // 예: if (statEnum == Player.BonusStat.AttackSpeed) result = Mathf.Max(result, 0.1f);
        return result;
    }

    public void RemoveBuffEffect(Player.PlayerBuffEffect buffEffect)
    {
        playerBuffEffects.Remove(buffEffect);
        playerBonusStats.Remove(playerBonusStats.FirstOrDefault(x => x.playerBuffEffect.Equals(buffEffect)));
    }

    public void ChangeValueForDuration(Action<float> setter, Func<float> getter, float changeValue, float duration)
    {
        StartCoroutine(ChangeValueForDurationCoroutine(setter, getter, changeValue, duration));
    }

     private IEnumerator ChangeValueForDurationCoroutine(Action<float> setter, Func<float> getter, float changeValue, float duration)
     {
          setter(getter() + changeValue);
          yield return new WaitForSeconds(duration);
          setter(getter() - changeValue);
     }

     public void ChangeValueForDuration(Action<Enum> setter, Func<Enum> getter, Enum changeValue, float duration)
     {
          StartCoroutine(ChangeValueForDurationCoroutine(setter, getter, changeValue, duration));
     }

     // Dictionary to track active enum effect coroutines
     private Dictionary<Enum, Coroutine> activeEnumEffects = new Dictionary<Enum, Coroutine>();

     private IEnumerator ChangeValueForDurationCoroutine(Action<Enum> setter, Func<Enum> getter, Enum changeValue, float duration)
     {
          Enum currentValue = getter();
          bool alreadyHasFlag = false;

          // Check if the value already has the flag
          if (currentValue != null && changeValue != null &&
              Convert.ToInt32(currentValue) != 0 &&
              (Convert.ToInt32(currentValue) & Convert.ToInt32(changeValue)) == Convert.ToInt32(changeValue))
          {
               alreadyHasFlag = true;

               // 이미 실행 중인 코루틴이 있으면 중지
               if (activeEnumEffects.TryGetValue(changeValue, out Coroutine existingCoroutine))
               {
                    if (existingCoroutine != null)
                    {
                         StopCoroutine(existingCoroutine);
                    }
                    // 딕셔너리에서 이전 코루틴을 제거 (나중에 새 코루틴으로 다시 추가됨)
                    activeEnumEffects.Remove(changeValue);
               }
          }

          if (!alreadyHasFlag)
          {
               // 값이 없는 경우에만 추가
               setter((Enum)Enum.ToObject(getter().GetType(), Convert.ToInt32(getter()) | Convert.ToInt32(changeValue)));
          }

          // 현재 코루틴을 딕셔너리에 등록
          activeEnumEffects[changeValue] = StartCoroutine(RemoveValueAfterDelay(setter, getter, changeValue, duration));

          yield break; // 실제 대기는 RemoveValueAfterDelay에서 처리
     }

     private IEnumerator RemoveValueAfterDelay(Action<Enum> setter, Func<Enum> getter, Enum changeValue, float duration)
     {
          yield return new WaitForSeconds(duration);

          // 지정된 시간이 지나면 값을 제거
          setter((Enum)Enum.ToObject(getter().GetType(), Convert.ToInt32(getter()) & ~Convert.ToInt32(changeValue)));

          // 딕셔너리에서 코루틴 제거
          if (activeEnumEffects.ContainsKey(changeValue))
          {
               activeEnumEffects.Remove(changeValue);
          }
     }

    public void AddExperience(float experience)
    {
        playerExperience += experience * playerExperienceMultiplier;
        // Leveling logic - needs to be edited
        if (playerExperience >= experienceToLevelUp)
        {
            LevelUp();
#if UNITY_EDITOR
            DebugConsole.Line levelUpLog = new()
            {
                text = $"[{gameTimer}] Player leveled up to {playerLevel}",
                messageType = DebugConsole.MessageType.Local,
                tick = gameTimer
            };
            DebugConsole.Instance.MergeLine(levelUpLog, "#00FF00");
#endif
        }
    }

    private void LevelUp()
    {
        playerExperience -= experienceToLevelUp;
        playerLevel++;
        switch (playerLevel)
        {
            case 2:
                experienceToLevelUp = 300;
                break;
            case 3:
                experienceToLevelUp = 1000;
                break;
            default:
                experienceToLevelUp += 1000;
                break;
        }
        // TODO: 레벨업 시 LevelUpSelectManager를 통해 선택 UI 표시 로직 호출
        // LevelUpSelectManager.Instance.ShowLevelUpOptions();
    }
}
