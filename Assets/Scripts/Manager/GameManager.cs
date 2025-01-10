using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
     public PlayerMove player;
     public GameState gameState;
     public long gameTimer;

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

     void FixedUpdate()
     {
          if (gameState == GameState.InGame)
          {
               gameTimer += 1;
          }
     }





}
