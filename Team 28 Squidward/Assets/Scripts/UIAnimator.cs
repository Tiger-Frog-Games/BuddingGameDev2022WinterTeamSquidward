using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TeamSquidward.Eric
{
    public class UIAnimator : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        private InputActionAsset actions;
        private InputAction openMenuButton;

        [SerializeField]
        private Animator menuAnimator;
        

        [SerializeField] private GameObject SheepMenuHolder;
        [SerializeField] private GameObject ScreenGrayer;
        [SerializeField] private GameObject LargeSheepImage;
        [SerializeField] private GameObject Timer;
        [SerializeField] private GameObject OptionsMenu;
        [SerializeField] private GameObject RequestPanel;

        [SerializeField] private EventChannelSO OnDayStart;
        [SerializeField] private EventChannelSO OnDayOver;
        [SerializeField] private EventChannelSO OnNightTimeCleanUp;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            openMenuButton = actions.FindAction("Open Menu");
            if (openMenuButton != null)
            {
                openMenuButton.performed += OnOpenMenuButtonPress;
            }
        }

        private void OnDestroy()
        {
            if (openMenuButton != null)
            {
                openMenuButton.performed -= OnOpenMenuButtonPress;
            }
        }

        private void OnEnable()
        {
            openMenuButton?.Enable();
            OnDayOver.OnEvent += OnDayOver_OnEvent;


        }

        private void OnDisable()
        {
            openMenuButton?.Disable();
            OnDayOver.OnEvent -= OnDayOver_OnEvent;
        }

        #endregion

        #region Methods

        private void OnOpenMenuButtonPress(InputAction.CallbackContext obj)
        {
            if (GameStateManager.Instance.CurrentGameState == GameState.Night)
            {
                return;
            }

            if (SheepMenuHolder.gameObject.activeSelf == false)
            {
                menuAnimator.SetTrigger("OnMenuShow");
                SheepMenuHolder.SetActive(true);
                ScreenGrayer.SetActive(true);
                GameStateManager.Instance.SetState(GameState.Paused);
            }
            else //if (1== 2)
            {
                menuAnimator.SetTrigger("OnMenuShow");
            }

        }

        private void OnMenuHidingAndDoneAnimating()
        {
            SheepMenuHolder.SetActive(false);
            ScreenGrayer.SetActive(false);
            if (GameStateManager.Instance.CurrentGameState == GameState.Paused)
            {
                GameStateManager.Instance.SetState(GameState.Gameplay);
            }
        }

        private void OnNightTimeMenusDoneAnimating()
        {
            OnDayStart.RaiseEvent();
            LargeSheepImage.SetActive(false);
            RequestPanel.SetActive(false);
            GameStateManager.Instance.SetState(GameState.Gameplay);
        }

        private void OnDayOver_OnEvent()
        {
            if (menuAnimator.GetCurrentAnimatorStateInfo(0).IsName("IdleInGamePauseMenu") )
            {
                menuAnimator.SetTrigger("OnDayOverFromMenu");
            }
            else
            {
                menuAnimator.SetTrigger("OnDayOver");
            }
            LargeSheepImage.SetActive(true);
            RequestPanel.SetActive(true);

        }

        public void BroadCastNightTimeCleanUp()
        {
            OnNightTimeCleanUp.RaiseEvent();
        }

        

        #endregion
    }
}