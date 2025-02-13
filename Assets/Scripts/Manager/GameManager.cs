using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : Singleton<GameManager>
{
     public enum GameState
     {
          Lobby,
          InGame,
          Pause,
          GameOver
     }


     public Player playerScript;
     public Light2D playerLight;
     public PlayerMove player;
     public PlayerAttack playerAttack;
     public GameState gameState;
     public List<Player.PlayerBuffEffect> playerBuffEffects = new();
     public List<Player.PlayerBonusStat> playerBonusStats = new();
     public long gameTimer;
     public int currentStage = 0;
     public float dragDistanceMultiplier = 1.0f;
     public float dragSpeedMultiplier = 1.0f;
     public float playerExperienceMultiplier = 1.0f;
     public float experienceToLevelUp = 100;
     public float playerExperience = 0;
     public long playerLevel = 1;
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
          bool result = DropItemManager.Instance.SetProbabilityCard(BackendDataManager.Instance.probabilityCardList.FirstOrDefault(x => x.probabilityName == $"Stage{stageNumber}_DropItemProb"));
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
