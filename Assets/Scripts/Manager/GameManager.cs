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

     public override void Awake()
     {
          base.Awake();
          onGamePaused += () =>
          {
               Time.timeScale = IsGamePaused ? 0 : 1;
          };

          SetGameState(GameState.InGame);
          playerAssetData = BackendDataManager.Instance.GetUserAssetData();

#if UNITY_EDITOR
          // Check if DebugGUI_GameManager exists on this object and add it if not
          if (gameObject.GetComponent<DebugGUI_GameManager>() == null)
          {
               gameObject.AddComponent<DebugGUI_GameManager>().enabled = true;
               Debug.Log("DebugGUI_GameManager was automatically added to GameManager object");
          }
#endif
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
     }


}
