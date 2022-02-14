using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    enum AnimalState { NORMAL, STRESSED, THIRSTY, MUDDY }
    enum AnimalSize { NORMAL, BIG, BIGGER, CHONKY }

    public class Animal : MonoBehaviour
    {
        #region Variables

        private Rigidbody rb;
        [SerializeField] private Animator animatorAnimal;

        [SerializeField] private CinemachineVirtualCamera SheepCamera;

        [SerializeField] private SpriteRenderer SheepBodyTexture;
        [SerializeField] private GameObject SheepBody;
        [SerializeField] private GameObject SheepCollider;

        [Header("Events")]

        [SerializeField] private EventChannelSO OnNightTimeCleanUp;
        [SerializeField] private EventChannelSOInt OnHourChange;
        [SerializeField] private EventChannelSOInt OnMinChange;

        [Header("Stats")]

        [SerializeField] private string Name;

        [SerializeField] public StatData StressData;
        [SerializeField] public StatData MuddyData;
        [SerializeField] public StatData ThirstData;
        
        private float currentFoodValue;

        [Header("Tuning")]
        [SerializeField] private float chancePercentMinStressRaise = 5;
        [SerializeField] private float stressRaisePerMin = 10;
        [SerializeField] private float stressEventForce = 10;
        [SerializeField] private float stressRaisedOnRockHit = 5;
        [SerializeField] private float startingSize = 1;
        private Vector3 targetSize;
        
        [SerializeField] private float sheepScaleSpeed =2;

        [SerializeField] private float startingFoodValue = 1;
        private int numberOfFoodEaten = 0;
        [SerializeField] private float foodMultiplier = .1f;

        #endregion

        #region Unity Methods

        private void Start()
        {
            SheepCamera.gameObject.transform.SetParent(null);
            SheepCamera.transform.Rotate(90, 0, 0);

            rb = GetComponent<Rigidbody>();

            resetStats();

            currentFoodValue = startingFoodValue;
            targetSize = new Vector3(currentFoodValue, currentFoodValue, 1);

            
        }

        private void Update()
        {
            updateAnimator();

            changeSize(Time.deltaTime);
        }

        private void OnEnable()
        {
            OnHourChange.OnEvent += OnHourChangeEvent;
            OnMinChange.OnEvent += OnMinChangeEvent;

            StressData.OnMaxValueEvent += OnStressEvent;
            ThirstData.OnMaxValueEvent += OnThirstEvent;
            MuddyData.OnMaxValueEvent += OnMudEvent;
        }

        private void OnDisable()
        {
            OnHourChange.OnEvent -= OnHourChangeEvent;
            OnMinChange.OnEvent -= OnMinChangeEvent;

            StressData.OnMaxValueEvent -= OnStressEvent;
            ThirstData.OnMaxValueEvent -= OnThirstEvent;
            MuddyData.OnMaxValueEvent -= OnMudEvent;
        }

        private void Awake()
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            OnNightTimeCleanUp.OnEvent += OnNightTimeCleanUpEvent;
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            OnNightTimeCleanUp.OnEvent -= OnNightTimeCleanUpEvent;
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

        private void resetStats()
        {
            StressData.reset();
            MuddyData.reset();
            ThirstData.reset();
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

        private void updateAnimator()
        {
            animatorAnimal.SetFloat("Speed", rb.velocity.magnitude);
        }

        private void eatFood( FoodPickup foodIn )
        {
            currentFoodValue += foodIn.getFoodValue() * foodMultiplier;
            numberOfFoodEaten++;

            ChangeTargetSize();

            changeColor(foodIn.getFoodColor());
            
            foodIn.eatFood();
        }

        private void ChangeTargetSize()
        {
            targetSize.x = currentFoodValue;
            targetSize.y = currentFoodValue;
        }

        private Vector3 ChangeSizeHelper = new Vector3();
        private void changeSize(float deltaTime)
        {       
            SheepBody.gameObject.transform.localScale = Vector3.Lerp(SheepBody.gameObject.transform.localScale, targetSize, sheepScaleSpeed * deltaTime);
            ChangeSizeHelper.x = targetSize.x;
            ChangeSizeHelper.z = targetSize.y;

            SheepCollider.gameObject.transform.localScale = Vector3.Lerp(SheepCollider.gameObject.transform.localScale, targetSize, sheepScaleSpeed * deltaTime);
            
            SheepCamera.m_Lens.OrthographicSize = 10 + (2 * SheepBody.gameObject.transform.localScale.x);

            //todo update player sheep detecotor?
        }

        private void changeColor(Color newColor)
        {
            SheepBodyTexture.color = Color.Lerp(SheepBodyTexture.color, newColor, .1f );
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
            if ( 1 == 1)
            {
                StressData.changeValue(stressRaisePerMin);
            }
        }

        private void OnNightTimeCleanUpEvent()
        {

        }

        private void OnStressEvent()
        {
            animatorAnimal.SetTrigger("StressEvent");
        }

        public void OnDoneSpinningAndReadyToLaunch()
        {
            float x = Random.Range(-1, 1);
            float y = Random.Range(-1, 1);

            rb.AddForce(new Vector3(stressEventForce * x, 0, stressEventForce * y));
        }

        private void OnThirstEvent()
        {

        }

        private void OnMudEvent()
        {

        }

        #endregion
    }
}