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
     public long gameTimer;
     public int currentStage = 0;
     public float dragDistanceMultiplier = 1.0f;
     public float dragSpeedMultiplier = 1.0f;
     public float playerExperienceMultiplier = 1.0f;
     public float experienceToLevelUp = 100;
     public float expereinceToLevelUpMultiplier = 1.05f;
     public float playerExperience = 0;
     public long playerLevel = 1;
     // expereince item values
     public ArrayList experienceValues = new() { 50, 100, 150, 200, 250 };

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
               DebugConsole.Line errorLog = new()
               {
                    text = $"[{gameTimer}] Failed to set probability card for stage {stageNumber}",
                    messageType = DebugConsole.MessageType.Local,
                    tick = gameTimer
               };
               DebugConsole.Instance.MergeLine(errorLog, "#FF0000");
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
               gameTimer += 1;
          }
     }

     public void ChangeValueForDuration(Action<float> setter, Func<float> getter, float changeValue, float duration)
     {
          StartCoroutine(ChangeValueForDurationCoroutine(setter, getter, changeValue, duration));
     }

     private IEnumerator ChangeValueForDurationCoroutine(Action<float> setter, Func<float> getter, float changeValue, float duration)
     {
          float prevValue = getter();
          setter(getter() + changeValue);
          DebugConsole.Line valueChangeLog = new()
          {
               text = $"[{gameTimer}] {setter.Method.Name} changed from {prevValue} to {getter()} for {duration} seconds",
               messageType = DebugConsole.MessageType.Local,
               tick = gameTimer
          };
          DebugConsole.Instance.MergeLine(valueChangeLog, "#00FF00");
          yield return new WaitForSeconds(duration);
          setter(getter() - changeValue);
          DebugConsole.Line valueRevertLog = new()
          {
               text = $"[{gameTimer}] {setter.Method.Name} reverted to {prevValue}",
               messageType = DebugConsole.MessageType.Local,
               tick = gameTimer
          };
          DebugConsole.Instance.MergeLine(valueRevertLog, "#00FF00");
     }

     public void AddExperience(float experience)
     {
          playerExperience += experience * playerExperienceMultiplier;
          // Leveling logic - needs to be edited
          if (playerExperience >= experienceToLevelUp)
          {
               playerExperience -= experienceToLevelUp;
               experienceToLevelUp *= expereinceToLevelUpMultiplier;
               LevelUp();
               DebugConsole.Line levelUpLog = new()
               {
                    text = $"[{gameTimer}] Player leveled up to {playerLevel}",
                    messageType = DebugConsole.MessageType.Local,
                    tick = gameTimer
               };
               DebugConsole.Instance.MergeLine(levelUpLog, "#00FF00");
          }
     }

     private void LevelUp()
     {
          playerLevel++;
     }


}
