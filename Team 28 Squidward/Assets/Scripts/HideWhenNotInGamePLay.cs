using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class HideWhenNotInGamePLay : MonoBehaviour
    {
        #region Variables

        #endregion

        #region Unity Methods

        private void Awake()
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            if (GameStateManager.Instance.CurrentGameState != GameState.Gameplay)
            {
                this.gameObject.SetActive(false);
            }
            
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
        #endregion

        #region Methods
        private void OnGameStateChanged(GameState newGameState)
        {
            this.gameObject.SetActive( newGameState == GameState.Gameplay );
        }
            #endregion
        }
}