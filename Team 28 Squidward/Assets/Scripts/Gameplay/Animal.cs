using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class Animal : MonoBehaviour
    {
        #region Variables

        private Rigidbody rb;

        [SerializeField]
        private CinemachineVirtualCamera SheepCamera;

        [SerializeField]
        private SpriteRenderer SheepSprite;

        [SerializeField]
        private EventChannelSOInt OnHourChange;
        [SerializeField]
        private EventChannelSOInt OnMinChange;

        [Header("Stats")]
        private bool isStressedOut;
        private float currentStress;
        private bool isTooMuddy;
        private float currentMud;
        private bool isThirsty;
        private float currentThirst;
        private float currentSize;
        private float currentFoodEaten;
        [Header("Tuning")]
        [SerializeField]
        private float startingStress = 0;
        [SerializeField ,Tooltip ("When current stress reaches max stress trigger stress event")]
        private float maxStress = 100;
        [SerializeField]
        private float chancePercentMinStressRaise = 5;
        [SerializeField]
        private float stressRaisePerMin = 10;
        [SerializeField]
        private float stressRaisedOnRockHit = 5;
        [SerializeField]
        private float startingMud = 0;
        [SerializeField, Tooltip("When current mud reaches max mud trigger mud event")]
        private float maxMud = 100;
        [SerializeField]
        private float startingThirst = 0;
        [SerializeField, Tooltip("When current thirst reaches max thirst trigger thirst event")]
        private float maxThirst = 100;
        [SerializeField]
        private float startingSize = 1;
        [SerializeField]
        private float startingFoodEaten = 0;
        [SerializeField]
        private float foodMultiplier = .1f;

        #endregion

        #region Unity Methods

        private void Start()
        {
            SheepCamera.gameObject.transform.SetParent(null);
            SheepCamera.transform.Rotate(90, 0, 0);

            rb = GetComponent<Rigidbody>();

            setUpInitialStats();
        }

        private void OnEnable()
        {
            OnHourChange.OnEvent += OnHourChangeEvent;
            OnMinChange.OnEvent += OnMinChangeEvent;
        }

        private void OnDisable()
        {
            OnHourChange.OnEvent -= OnHourChangeEvent;
            OnMinChange.OnEvent -= OnMinChangeEvent;
        }

        private void Awake()
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private Vector3 velocityBeforePause;
        private Vector3 angluarVelocityBeforePause;

        private void OnGameStateChanged(GameState newGameState)
        {
            enabled = newGameState == GameState.Gameplay;
            if(newGameState == GameState.Gameplay)
            {
                rb.isKinematic = false;
                if (velocityBeforePause != null)
                {
                    rb.velocity = velocityBeforePause;
                }
                else
                {
                    rb.velocity = Vector3.zero;
                }

                if (angluarVelocityBeforePause != null)
                {
                    rb.angularVelocity = angluarVelocityBeforePause;
                }
                else
                {
                    rb.angularVelocity = Vector3.zero;
                }

            }else if (newGameState == GameState.Night)
            {
                rb.isKinematic = true;
                velocityBeforePause = Vector3.zero;
                angluarVelocityBeforePause = Vector3.zero;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else if(newGameState == GameState.Paused)
            {
                velocityBeforePause = rb.velocity;
                angluarVelocityBeforePause = rb.angularVelocity;

                rb.isKinematic = true;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
        }

        private void setUpInitialStats()
        {
            currentStress = startingStress;
            currentMud = startingMud;
            currentSize = startingSize;
            this.transform.localScale = new Vector3(currentSize, currentSize, currentSize);
            currentFoodEaten = startingFoodEaten;
        }
   
        private void OnTriggerEnter(Collider other)
        {
            if ( other.TryGetComponent<FoodPickup>(out FoodPickup eatingFood) )
            {
                eatFood(eatingFood);
            }
        }

        #endregion

        #region Methods

        private void eatFood( FoodPickup foodIn )
        {
            currentFoodEaten += foodIn.getFoodValue();
            changeSize();

            changeColor(foodIn.getFoodColor());
            
            foodIn.eatFood();
        }

        private void changeSize()
        {
            //todo diminishing return on food eaten on size
            currentSize = startingSize + (currentFoodEaten * foodMultiplier);
            //todo smoother ratio 
            SheepCamera.m_Lens.OrthographicSize = 10 + (2*currentSize);

            this.transform.localScale = new Vector3(currentSize, 1, currentSize);
            //todo update player sheep detecotor?
        }

        private void changeColor(Color newColor)
        {
            SheepSprite.color = Color.Lerp(SheepSprite.color, newColor, .1f );
        }

        public void setActiveCamera()
        {
            SheepCamera.Priority = 10;
        }

        public void removeActiveCamera()
        {
            SheepCamera.Priority = 0;
        }

        private void OnHourChangeEvent(int h)
        {

        }

        private void OnMinChangeEvent(int m)
        {

        }

        #endregion
    }
}