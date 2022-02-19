using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class BackgroundSoundManager : MonoBehaviour
    {
        private static BackgroundSoundManager _instance;
        public static BackgroundSoundManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BackgroundSoundManager();
                }
                return _instance;
            }
        }

        #region Variables

        [SerializeField]
        private AudioSource gameTheme;
        [SerializeField]
        private AudioSource nightTheme;

        #endregion

        #region Unity Methods
        private void Awake()
        {
            _instance = this;
            //DontDestroyOnLoad(this);

            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;

            //gameTheme.Play();
        }
        private void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
        #endregion

        #region Methods

        private GameState prevGameState;
        private void OnGameStateChanged(GameState newGameState)
        {
            if (newGameState == GameState.Night)
            {
                gameTheme.Stop();
                nightTheme.Play();
            }
            if (newGameState == GameState.Gameplay && prevGameState == GameState.Night)
            {
                gameTheme.Stop();
                nightTheme.Play();
            }
            prevGameState = newGameState;

        }
        #endregion
    }
}