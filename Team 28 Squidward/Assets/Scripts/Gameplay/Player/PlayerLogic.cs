using ECM2.Components;
using Micosmo.SensorToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class PlayerLogic : MonoBehaviour
    {
        #region Variables

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

        private Animal currentAnimalPushing;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            OnNightCleanUp.OnEvent += OnNightCleanUp_OnEvent;
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            OnNightCleanUp.OnEvent -= OnNightCleanUp_OnEvent;
        }

        private void Start()
        {
            sheepDetector.OnLostDetection.AddListener(onSheepOutOfRange);
        }

        private void OnEnable()
        {
            OnNightCleanUp.OnEvent += OnNightCleanUp_OnEvent;
        }

        private void OnDisable()
        {
            OnNightCleanUp.OnEvent -= OnNightCleanUp_OnEvent;
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

        private void onSheepOutOfRange(GameObject obj, Sensor sens)
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
        #endregion
    }
}