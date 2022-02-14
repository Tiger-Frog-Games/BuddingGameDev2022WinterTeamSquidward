using ECM2.Components;
using Micosmo.SensorToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TeamSquidward.Eric
{
    public class PlayerLogic : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private InputActionAsset actions;
        private InputAction petButton;
        private InputAction moveMent;

        [SerializeField]
        private Transform startPosition;
        [SerializeField]
        private SquidwardMovement squidMovement;
        [SerializeField]
        private CharacterMovement charMovement;

        [SerializeField]
        private RangeSensor sheepDetector;

        [SerializeField]
        private EventChannelSO OnNightCleanUp;

        [SerializeField] private Animator farmerAnimation;
        private Animal currentAnimalPushing;

        private bool holdingBrush;


        #endregion

        #region Unity Methods

        private void Awake()
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            OnNightCleanUp.OnEvent += OnNightCleanUp_OnEvent;
            
            petButton = actions.FindAction("Pet");
            if (petButton != null)
            {
                petButton.performed += OnPetSheepButtonPress;
            }
            moveMent = actions.FindAction("moveMent");

        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            OnNightCleanUp.OnEvent -= OnNightCleanUp_OnEvent;

            if (petButton != null)
            {
                petButton.performed -= OnPetSheepButtonPress;
            }
        }

        private void Start()
        {
            sheepDetector.OnLostDetection.AddListener(onSheepOutOfRange);
        }

        private void OnEnable()
        {
            OnNightCleanUp.OnEvent += OnNightCleanUp_OnEvent;
            petButton.Enable();
        }

        private void OnDisable()
        {
            OnNightCleanUp.OnEvent -= OnNightCleanUp_OnEvent;
            petButton.Disable();
        }
        private void OnGameStateChanged( GameState newGameState )
        {
            enabled = newGameState == GameState.Gameplay;
            //squidMovement.enabled = newGameState == GameState.Gameplay;
            charMovement.Pause(!(newGameState == GameState.Gameplay));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (currentAnimalPushing == null && collision.gameObject.TryGetComponent<Animal>(out Animal animal))
            {
                currentAnimalPushing = animal;
                currentAnimalPushing.setActiveCamera();
            }
        }

        #endregion

        #region Methods

        private void onSheepOutOfRange(GameObject obj, Micosmo.SensorToolkit.Sensor sens)
        {
            if ( currentAnimalPushing != null && obj.transform.parent?.gameObject == currentAnimalPushing.gameObject )
            {
                currentAnimalPushing.removeActiveCamera();
                currentAnimalPushing = null;
            }            
        }

        private void OnNightCleanUp_OnEvent()
        {
            this.transform.position = startPosition.transform.position;
        }

        private void OnPetSheepButtonPress(InputAction.CallbackContext obj)
        {
            if (currentAnimalPushing != null)
            {
                moveMent.Disable();
                if (holdingBrush == false)
                {
                    //PET THE SHEEP
                    farmerAnimation.SetTrigger("Petting");

                }
                else
                {
                    //brush the sheep
                }
            }
        }

        public void DonePettingSheep()
        {
            moveMent.Enable();
        }

        public void DoneBrushingSheep()
        {
            moveMent.Enable();
        }

        #endregion
    }
}