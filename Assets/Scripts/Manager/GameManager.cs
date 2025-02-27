using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : Singleton<GameManager>
{
     public enum GameState
     {
          InGame,
          Pause,
          GameOver
     }

    // 레벨업 이벤트 수정 - 이벤트를 null 체크 없이 호출할 수 있도록 함
    // 델리게이트 정의
    public delegate void OnPlayerLevelChanged();
    // 이벤트 초기화하여 null이 되지 않도록 함
    public event OnPlayerLevelChanged onPlayerLevelChanged = delegate { };
    public delegate void OnGamePaused();
     public event OnGamePaused onGamePaused;

     public Player playerScript;
     public Light2D playerLight;
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

    // playerLevel 프로퍼티 수정
    public long playerLevel
    {
        get => _playerLevel;
        set
        {
            Debug.Log($"[GameManager] 레벨 변경: {_playerLevel} -> {value}");
            _playerLevel = value;

            // value > 1 체크는 제거하고 항상 이벤트 발생
            Debug.Log($"[GameManager] onPlayerLevelChanged 이벤트 호출");
            // null 체크가 필요 없음 - 빈 델리게이트로 초기화했기 때문
            onPlayerLevelChanged.Invoke();
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


    public override void Awake()
    {
        base.Awake();
        Debug.Log("[GameManager] Awake 호출됨");

        // 테스트를 위해 레벨업 이벤트 기본 구독자 추가
        onPlayerLevelChanged += () => { Debug.Log("[GameManager] 레벨업 이벤트 기본 처리기 호출됨"); };

        onGamePaused += () =>
        {
            Time.timeScale = IsGamePaused ? 0 : 1;
            Debug.Log($"[GameManager] Time.timeScale 설정됨: {Time.timeScale}");
        };

        SetGameState(GameState.InGame);
        playerAssetData = BackendDataManager.Instance.GetUserAssetData();
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

     public float GetPlayerStatValue(Player.BonusStat statEnum, float value)
     {
          float result = value;
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
                              result *= bonusStat.value;
                              break;
                    }
               }
          }
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
        Debug.Log($"[GameManager] LevelUp 시작: 현재 경험치={playerExperience}/{experienceToLevelUp}, 현재 레벨={playerLevel}");

        playerExperience -= experienceToLevelUp;
        playerLevel = playerLevel + 1;

        // 이벤트 로그 추가
        Debug.Log($"[GameManager] onPlayerLevelChanged 이벤트 구독자 수: {onPlayerLevelChanged.GetInvocationList().Length}");


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

        Debug.Log($"[GameManager] 다음 레벨업까지 필요 경험치: {experienceToLevelUp}");
    }


}
