using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TeamSquidward.Eric
{
    public class UIShower : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        private InputActionAsset actions;
        [SerializeField]
        private Animator menuAnimator;
        private InputAction openMenuButton;

        [SerializeField]
        private GameObject SheepMenuHolder;
        [SerializeField]
        private GameObject ScreenGrayer;

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
            
        }

        private void OnDisable()
        {
            openMenuButton?.Disable();
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



        #endregion
    }
}