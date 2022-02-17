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
        [SerializeField] private SpriteRenderer bodySprite;
        [SerializeField] private SpriteRenderer bodyOutlineSprite;

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

        [SerializeField] private string sheepName;

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

        [SerializeField] private float foodValueForStageTwo = 10;
        [SerializeField] private float foodValueForStageThree = 20;
        [SerializeField] private float foodValueForStageFour = 30;
        [SerializeField] private Sprite stageTwoSprite;
        [SerializeField] private Sprite stageTwoOutlineSprite;
        [SerializeField] private Sprite stageThreeSprite;
        [SerializeField] private Sprite stageThreeOutlineSprite;
        [SerializeField] private Sprite stageFourSprite;
        [SerializeField] private Sprite stageFourOutlineSprite;

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
        private int sizeForTasks = 1;


        #endregion

        #region Unity Methods

        private void Start()
        {
            SheepCamera.gameObject.transform.SetParent(null);
            SheepCamera.transform.Rotate(90, 0, 0);

            resetStats();

            SheepBodyTexture.color = baseColor;

            currentFoodValue = startingFoodValue;
            targetSize = new Vector3(currentFoodValue, currentFoodValue, 1);
            changeBodySprite();


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

            rb = GetComponent<Rigidbody>();
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

            if (newGameState == GameState.Gameplay)
            {
                animatorAnimal.speed = 1;
            }
            else
            {
                animatorAnimal.speed = 0;
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
            if (currentSize < 5)
            {
                animatorAnimal.SetFloat("RollSpeed", 1);
            }
            else
            {
                animatorAnimal.SetFloat("RollSpeed", rb.velocity.magnitude / (2 * currentSize));
            }
            
            //print();

            if (rb.velocity.x > .1)
            {
                Vector3 temp = visualGameObject.transform.localScale;
                temp.x = 1;
                visualGameObject.transform.localScale = temp;
                
            }
            if (rb.velocity.x < - .1)
            {
                Vector3 temp = visualGameObject.transform.localScale;
                temp.x = -1;
                visualGameObject.transform.localScale = temp;
                
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

       // private Vector3 ChangeSizeHelper = new Vector3();\
        private float currentSize;
        private void changeSize(float deltaTime)
        {       
            currentSize = Vector3.Lerp(SheepBody.gameObject.transform.localScale, targetSize, sheepScaleSpeed * deltaTime).x;
            SheepBody.gameObject.transform.localScale = Vector3.Lerp(SheepBody.gameObject.transform.localScale, targetSize, sheepScaleSpeed * deltaTime);
            //ChangeSizeHelper.x = targetSize.x;
            //ChangeSizeHelper.z = targetSize.y;

            SheepCollider.gameObject.transform.localScale = Vector3.Lerp(SheepCollider.gameObject.transform.localScale, targetSize, sheepScaleSpeed * deltaTime);
            
            SheepCamera.m_Lens.OrthographicSize = 10 + (2 * SheepBody.gameObject.transform.localScale.x);

            //be more clever about this?
            farmerDetector.Sphere.Radius = targetSize.x * 3;

            changeBodySprite();
            

        }

        private void changeBodySprite()
        {
            if (currentFoodValue < foodValueForStageTwo)
            {
                //stage 1
                return;
            }

            if (currentFoodValue < foodValueForStageThree)
            {
                //stage 2
                bodySprite.sprite = stageTwoSprite;
                bodyOutlineSprite.sprite = stageTwoOutlineSprite;
            }
            else if (currentFoodValue < foodValueForStageFour)
            {
                //stage 3
                bodySprite.sprite = stageThreeSprite;
                bodyOutlineSprite.sprite = stageThreeOutlineSprite;
            }
            else
            {
                //stage 4
                bodySprite.sprite = stageFourSprite;
                bodyOutlineSprite.sprite = stageFourOutlineSprite;
            }
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
            LaunchSheep(true);
        }

        private Vector3 stressEventDirection = new Vector3();
        private Vector2 stressEventDirectionHelper = new Vector2();
        public void LaunchSheep(bool fullRange = false)
        {
            if (fullRange == true)
            {
                stressEventDirectionHelper.x = Random.Range(-1f, 1f);
                stressEventDirectionHelper.y = Random.Range(-1f, 1f);
            }
            else
            {
                stressEventDirectionHelper.x = Random.Range(-.75f, .75f);
                stressEventDirectionHelper.y = Random.Range(0f, 1f);
            }
            
            
            stressEventDirectionHelper = stressEventDirectionHelper.normalized;

            stressEventDirection.x = stressEventDirectionHelper.x * stressEventForce;
            stressEventDirection.y = .5f;
            stressEventDirection.z = stressEventDirectionHelper.y * stressEventForce;


            rb.AddForce(stressEventDirection );
        }

        /// <summary>
        ///  Called from rock adds stress to animal!
        /// </summary>
        public void addStress()
        {

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

        public void setName(string nameIn )
        {
            name = "Sheep - " + nameIn;
            sheepName = nameIn;
        }

        public string getSheepName()
        {
            return sheepName;
        }

        #endregion
    }
}