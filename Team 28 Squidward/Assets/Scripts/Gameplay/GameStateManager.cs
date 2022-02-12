using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{

    public enum GameState
    {
        Gameplay,
        Night,
        Paused
    }

    public class GameStateManager 
    {
        private static GameStateManager _instance;
        public static GameStateManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new GameStateManager();
                }
                return _instance;
            }
        }

        private GameStateManager()
        {

        }

        public GameState CurrentGameState { get; private set; }

        public delegate void GameStateChangeHandler(GameState newGameState);
        public event GameStateChangeHandler OnGameStateChanged;

        #region Variables

        #endregion

        #region Unity Methods

        #endregion

        #region Methods

        public void SetState( GameState newGameState )
        {
            if (CurrentGameState == newGameState)
            {
                return;
            }

            CurrentGameState = newGameState;
            OnGameStateChanged?.Invoke(newGameState);
        }

        #endregion
    }
}