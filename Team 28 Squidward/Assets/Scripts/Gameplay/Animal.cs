using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Micosmo.SensorToolkit;

namespace TeamSquidward.Eric
{
    enum AnimalState { NORMAL, STRESSED, THIRSTY, MUDDY }
    enum AnimalSize { NORMAL, BIG, BIGGER, CHONKY }

    public class Animal : MonoBehaviour
    {
        #region Variables

        private Rigidbody rb;
        private PlayerLogic farmerInRange;

        [SerializeField] private RangeSensor farmerDetector;
        [SerializeField] private Animator animatorAnimal;

        [SerializeField] private CinemachineVirtualCamera SheepCamera;

        [SerializeField] private SpriteRenderer SheepBodyTexture;
        [SerializeField] private GameObject SheepBody;
        [SerializeField] private GameObject SheepCollider;
        [SerializeField] private GameObject visualGameObject;

        [Header("Events")]

        [SerializeField] private EventChannelSO OnNightTimeCleanUp;
        [SerializeField] private EventChannelSOInt OnHourChange;
        [SerializeField] private EventChannelSOInt OnMinChange;

        [Header("Stats")]

        [SerializeField] private string Name;

        [SerializeField] public StatData StressData;
        [SerializeField] public StatData MuddyData;
        [SerializeField] public StatData ThirstData;
        

        [Header("Tuning")]

        //Stress variables
        private bool isStressed = false;
        [SerializeField] private float chancePercentMinStressRaise = .05f;
        [SerializeField] private float stressRaisePerMin = 10;
        [SerializeField] private float stressEventForce = 10;
        [SerializeField] private float stressRaisedOnRockHit = 5;
        [SerializeField] private float stressRemovedFromPetting = 20;


        private Vector3 targetSize;
        
        [SerializeField] private float sheepScaleSpeed =2;
        private float currentFoodValue;
        [SerializeField] private float startingFoodValue = 1;
        [SerializeField] private float minPristineValue = 10;
        [SerializeField] private float numberOfTotalFoodTillPristineEligable = 30;
        [SerializeField] private float foodMultiplier = .1f;


        [SerializeField] private Color baseColor;
        [SerializeField] private List<foodColorData> foodColorValues;

        //task varibales
        private bool isPristine = false;



        #endregion

        #region Unity Methods

        private void Start()
        {
            SheepCamera.gameObject.transform.SetParent(null);
            SheepCamera.transform.Rotate(90, 0, 0);

            rb = GetComponent<Rigidbody>();

            resetStats();

            SheepBodyTexture.color = baseColor;

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

            farmerDetector.OnLostDetection.AddListener(OnFarmerLeaveRange);
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            OnNightTimeCleanUp.OnEvent -= OnNightTimeCleanUpEvent;

            farmerDetector.OnLostDetection.RemoveListener(OnFarmerLeaveRange);
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

        private void OnCollisionEnter(Collision enter)
        {
            if (enter.gameObject.TryGetComponent<Rockscript>(out Rockscript rock) ) 
            {
                StressData.changeValue(stressRaisedOnRockHit);
            }
        }

        #endregion

        #region Methods
        private bool isFacingRight = false;
        private void updateAnimator()
        {
            
            animatorAnimal.SetFloat("Speed", rb.velocity.magnitude);

            //print();

            if (rb.velocity.x > .1)
            {
                Vector3 temp = visualGameObject.transform.localScale;
                temp.x = 1;
                visualGameObject.transform.localScale = temp;
                isFacingRight = true;
            }
            if (rb.velocity.x < - .1)
            {
                Vector3 temp = visualGameObject.transform.localScale;
                temp.x = -1;
                visualGameObject.transform.localScale = temp;
                isFacingRight = false;
            }
        }

        private void eatFood( FoodPickup foodIn )
        {
            currentFoodValue += foodIn.getFoodValue() * foodMultiplier;
            
            FOODTYPE toAdd = foodIn.getFoodType();
            foreach ( foodColorData foodData in foodColorValues)
            {
                if (foodData.type == toAdd)
                {
                    foodData.Value += foodIn.getFoodValue() * foodMultiplier;
                    break;
                }
            }

            changeColor();
            ChangeTargetSize();
            
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

            //be more clever about this?
            farmerDetector.Sphere.Radius = targetSize.x * 3;
            
        }

        private void changeColor()
        {
            foodColorValues = foodColorValues.OrderByDescending(x => x.Value).ToList<foodColorData>();

            //top three colors
            foodColorData first = foodColorValues[0];
            foodColorData second = foodColorValues[1];
            foodColorData third = foodColorValues[2];

            float totalColor = 0;
            if (first.Value != 0)
            {
                totalColor += first.Value;
            }
            if (second.Value != 0)
            {
                totalColor += second.Value;
            }
            if (third.Value != 0)
            {
                totalColor += third.Value;
            }

            if (totalColor == 0)
            {
                SheepBodyTexture.color = baseColor;
                return;
            }

            if (first.Value - second.Value >= minPristineValue)
            {
                SheepBodyTexture.color = first.color;
                if (currentFoodValue > numberOfTotalFoodTillPristineEligable)
                {
                    print("What a pristine color sheep");
                    isPristine = true;
                }
                else
                {
                    isPristine = false;
                }
            }
            else
            {
                //print($"{first.Value / totalColor}-{second.Value / totalColor}-{third.Value / totalColor}");
                SheepBodyTexture.color = baseColor;
                SheepBodyTexture.color = Color.Lerp(SheepBodyTexture.color, first.color, (first.Value / totalColor));
                SheepBodyTexture.color = Color.Lerp(SheepBodyTexture.color, second.color, (second.Value / totalColor));
                SheepBodyTexture.color = Color.Lerp(SheepBodyTexture.color, third.color, (third.Value / totalColor));

                isPristine = false;
            }


        }

        public void OnFarmerLeaveRange(GameObject obj, Micosmo.SensorToolkit.Sensor sens)
        {
            if (obj.TryGetComponent<PlayerLogic>(out PlayerLogic farmer))
            {
                farmer.onSheepOutOfRange(this);
            }
        }

        public void setActiveCamera(PlayerLogic farmerInRangeI)
        {
            SheepCamera.Priority = 10;
            farmerInRange = farmerInRangeI;
        }

        public void removeActiveCamera()
        {
            SheepCamera.Priority = 0;
            farmerInRange = null;
        }

        private void OnHourChangeEvent(int h)
        {

        }

        private void OnMinChangeEvent(int m)
        {
            float r = Random.Range(0f, 1f);
            if ( r < chancePercentMinStressRaise)
            {
                StressData.changeValue(stressRaisePerMin);
            }
        }

        private void OnNightTimeCleanUpEvent()
        {

        }

        private void OnStressEvent()
        {
            if (isStressed)
            {
                return;
            }
            animatorAnimal.SetTrigger("StressEvent");
            isStressed = true;
            if (farmerInRange != null)
            {
                farmerInRange.knockback(this.transform.position);
            }
            
        }


        public void OnStressEventRollAnimationDone()
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
        
        public void Pet()
        {
            animatorAnimal.SetTrigger("PetEvent");
            
            if (isStressed)
            {
                StressData.reset();
                isStressed = false;
            }
            else
            {
                StressData.changeValue(-stressRemovedFromPetting);
            }
        }

        #endregion
    }
}