using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TeamSquidward.Eric
{
    public class UIAnimator : MonoBehaviour
    {
        private static UIAnimator _instance;
        public static UIAnimator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UIAnimator();
                }
                return _instance;
            }
        }
        #region Variables
        [SerializeField]
        private InputActionAsset actions;
        private InputAction openMenuButton;

        [SerializeField]
        private Animator menuAnimator;
        

        [SerializeField] private GameObject SheepMenuHolder;
        [SerializeField] private GameObject ScreenGrayer;
        [SerializeField] private GameObject ScreenBlackout;
        [SerializeField] private GameObject LargeSheepImage;
        [SerializeField] private GameObject Timer;
        [SerializeField] private GameObject OptionsMenu;
        [SerializeField] private GameObject RequestPanel;
        [SerializeField] private GameObject SellingSheepPanel;
        [SerializeField] private GameObject sheepPenMenu;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject exitGameMenu;

        [SerializeField] private GameObject[] hatHolderPauseMenu;
        private int prevHat;

        [SerializeField] private EventChannelSO OnDayStart;
        [SerializeField] private EventChannelSO OnDayOver;
        [SerializeField] private EventChannelSO OnNightTimeCleanUp;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _instance = this;
            openMenuButton = actions.FindAction("Open Menu");
            if (openMenuButton != null)
            {
                openMenuButton.performed += OnOpenMenuButtonPress;
            }
            tutorialPanel.SetActive(true);
        }

        private void Start()
        {
            GameStateManager.Instance.SetState(GameState.Paused);
            Timer.SetActive(false);

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

            if (tutorialPanel.gameObject.activeSelf == true) 
            {
                hideTutorial();
                return;
            }

            if (sheepPenMenu.gameObject.activeSelf == true)
            {
                hideSheepPen();
                return;
            }

            if (RequestPanel.gameObject.activeSelf == true)
            {
                ScreenGrayer.SetActive(false);
                requestPannelButtonCloser.SetActive(false);
                RequestPanel.SetActive(false);

                GameStateManager.Instance.SetState(GameState.Gameplay);
                return;
            }

            if (SellingSheepPanel.gameObject.activeSelf == true)
            {
                SellingSheepPanel.SetActive(false);
                ScreenGrayer.SetActive(false);
                GameStateManager.Instance.SetState(GameState.Gameplay);
                return;
            }

            

            if (optionsPanel.gameObject.activeSelf == true)
            {
                optionsPanel.SetActive(false);
               
                return;
            }


            if (SheepMenuHolder.gameObject.activeSelf == false)
            {
                if ( unlockedHats > 1 )
                {
                    if (hatHolderPauseMenu[prevHat].activeSelf == true)
                    {
                        hatHolderPauseMenu[prevHat].SetActive(false);
                    }
                    prevHat = activeHat;
                    hatHolderPauseMenu[activeHat].SetActive(true);
                }
                menuAnimator.SetTrigger("OnMenuShow");
                menuAnimator.ResetTrigger("OnMenuHide");
                SheepMenuHolder.SetActive(true);
                ScreenGrayer.SetActive(true);
                GameStateManager.Instance.SetState(GameState.Paused);
            }
            else //if (1== 2)
            {
                menuAnimator.SetTrigger("OnMenuHide");
                menuAnimator.ResetTrigger("OnMenuShow");
            }

        }

        public void OnCloseOptionsMenu()
        {
            menuAnimator.SetTrigger("OnMenuHide");
            menuAnimator.ResetTrigger("OnMenuShow");
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
            ScreenGrayer.SetActive(false);
            GameStateManager.Instance.SetState(GameState.Gameplay);
        }


        [SerializeField] private Image hatImage;
        [SerializeField] private GameObject hatHolder;
        [SerializeField] private List<Sprite> hatSprites;
        private int activeHat;
        private int unlockedHats = 1;
        public void OnSheepPenOpen()
        {
            RefreshHatsAvailiable();
            ScreenGrayer.SetActive(true);
            sheepPenMenu.SetActive(true);
            GameStateManager.Instance.SetState(GameState.Paused);

        }

        public void hideSheepPen()
        {
            ScreenGrayer.SetActive(false);
            sheepPenMenu.SetActive(false);
            GameStateManager.Instance.SetState(GameState.Gameplay);
        }

        public void RefreshHatsAvailiable()
        {
            if (unlockedHats == 1)
            {
                hatHolder.SetActive(false);
            }
            else
            {
                changeHatSprite();
                hatHolder.SetActive(true);
            }
        }

        public void NextHat()
        {
            activeHat++;


            if (activeHat > hatSprites.Count - 1 || activeHat > unlockedHats - 1 )
            {
                activeHat = 0;
            }
            changeHatSprite();
        }

        public void PrevHat()
        {
            activeHat--;
            if (activeHat < 0)
            {
                if (unlockedHats > hatSprites.Count - 1)
                {
                    activeHat = hatSprites.Count - 1;
                }
                else
                {
                    activeHat = unlockedHats - 1;
                }
            }
            changeHatSprite();
        }

        private void changeHatSprite()
        {
            if (activeHat == 0)
            {
                hatImage.gameObject.SetActive(false);

            }
            else
            {
                hatImage.gameObject.SetActive(true);
                hatImage.sprite = hatSprites[activeHat];
            }
        }

        [ContextMenu("Do something")]
        public void unlockAHat()
        {
            unlockedHats++;
        }

        public Sprite getSheepHat()
        {
            if (activeHat == 0)
            {
                return null;
            }

            return hatSprites[activeHat];
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

            sheepPenMenu.SetActive(false);
            LargeSheepImage.SetActive(true);
            

        }
        [SerializeField] private GameObject requestPannelButtonCloser;
        [SerializeField] private GameObject NextDayButton;
        public void showRequestPanelFromInGame()
        {
            SheepTaskManager.Instance.checkSheepToSell();

            RequestPanel.SetActive(true);
            ScreenGrayer.SetActive(true);

            requestPannelButtonCloser.SetActive(true);
            NextDayButton.SetActive(false);

            GameStateManager.Instance.SetState(GameState.Paused);
        }

        public void hideRequestPanelFromInGame()
        {
            RequestPanel.SetActive(false);
            ScreenGrayer.SetActive(false);

            requestPannelButtonCloser.SetActive(false);
            //NextDayButton.SetActive(true);

            GameStateManager.Instance.SetState(GameState.Gameplay);
        }

        public void BigSheepDoneShowing()
        {
            RequestPanel.SetActive(true);
            NextDayButton.SetActive(true);
            OnNightTimeCleanUp.RaiseEvent();
        }

        public void showSheepSellScreen()
        {
            RequestPanel.SetActive(false);
            SellingSheepPanel.SetActive(true);
            if (GameStateManager.Instance.CurrentGameState == GameState.Paused)
            {
                ScreenGrayer.SetActive(true);
            }
        }

        public void hideSellSheep()
        {
            SellingSheepPanel.SetActive(false);
            if (GameStateManager.Instance.CurrentGameState == GameState.Night)
            {
                RequestPanel.SetActive(true);
            }
            else
            {
                RequestPanel.SetActive(true);
            }
        }

        public void showEndGameScreen()
        {
            ScreenBlackout.SetActive(true);

            RequestPanel.SetActive(false);
            LargeSheepImage.SetActive(false);
            Timer.SetActive(false);



            exitGameMenu.SetActive(true);
        }

        private bool isGrayScreenActive;
        public void SellSheepAnimationStart()
        {
            isGrayScreenActive = ScreenGrayer.activeInHierarchy;
            ScreenGrayer.SetActive(false);
            SellingSheepPanel.SetActive(false);
            if (GameStateManager.Instance.CurrentGameState == GameState.Night)
            {
                LargeSheepImage.SetActive(false);
            }

        }

        public void SellSheepAnimationOver()
        {
            ScreenGrayer.SetActive(isGrayScreenActive);
            RequestPanel.SetActive(true);
            if (GameStateManager.Instance.CurrentGameState == GameState.Night)
            {
                LargeSheepImage.SetActive(true);
            }
        }

        public void showTutorial()
        {
            tutorialPanel.gameObject.SetActive(true);
        }

        private bool firstTimeClosed = false;
        public void hideTutorial()
        {
            tutorialPanel.gameObject.SetActive(false);
            
            if (firstTimeClosed == false)
            {
                GameStateManager.Instance.SetState(GameState.Gameplay);
                Timer.SetActive(true);
                firstTimeClosed = true;
            }
        }

        public void showOptions()
        {
            optionsPanel.gameObject.SetActive(true);
        }

        public void hideOptions()
        {
            optionsPanel.gameObject.SetActive(false);
        }

        public void exitGame()
        {
            SceneManager.LoadScene("Main Menu");
        }

        #endregion
    }
}