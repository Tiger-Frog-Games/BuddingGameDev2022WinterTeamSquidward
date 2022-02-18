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

        [SerializeField] private Transform startPosition;
        [SerializeField] private SquidwardMovement squidMovement;
        [SerializeField] private CharacterMovement charMovement;

        [SerializeField] private RangeSensor sheepDetector;

        [SerializeField] private EventChannelSO OnNightCleanUp;

        [SerializeField] private Animator farmerAnimation;
        
        private Animal currentAnimalPushing;
        private Animal lastAnimalPushed;
        


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
            //sheepDetector.OnLostDetection.AddListener(onSheepOutOfRange);
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
            squidMovement.enabled = newGameState == GameState.Gameplay;
            //squidMovement.enabled = newGameState == GameState.Gameplay;
            charMovement.Pause(!(newGameState == GameState.Gameplay));

            if (newGameState == GameState.Gameplay)
            {
                farmerAnimation.speed = 1;
            }
            else
            {
                farmerAnimation.speed = 0;
            }
            
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (currentAnimalPushing == null && collision.gameObject.TryGetComponent<Animal>(out Animal animal))
            {
                currentAnimalPushing = animal;
                currentAnimalPushing.setActiveCamera( this );
            }
        }

        #endregion

        #region Methods

        //called from animal when a sheep leaves the farmer's range
        public void onSheepOutOfRange(Animal obj)
        {
           
            if ( currentAnimalPushing != null && obj == currentAnimalPushing )
            {
                currentAnimalPushing.removeActiveCamera();
                lastAnimalPushed = currentAnimalPushing;
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

                //PET THE SHEEP
                farmerAnimation.SetTrigger("Petting");
                currentAnimalPushing.Pet();
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

        public void knockback(Vector3 sheepPosition)
        {
            squidMovement.knockBack(sheepPosition);
        }
        #endregion
    }
}