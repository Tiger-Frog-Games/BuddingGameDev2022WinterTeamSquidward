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

        [SerializeField] private Rigidbody rb;
        private PlayerLogic farmerInRange;

        [SerializeField] private LayerMask MudLayer;

        [SerializeField] private RangeSensor farmerDetector;
        [SerializeField] private Animator animatorAnimal;
        [SerializeField] private SpriteRenderer bodySprite;
        [SerializeField] private SpriteRenderer bodyOutlineSprite;

        [SerializeField] private CinemachineVirtualCamera SheepCamera;

        [SerializeField] private SpriteRenderer SheepBodyTexture;
        [SerializeField] private SpriteRenderer expresionSpriteRenderer;
        [SerializeField] private SpriteRenderer hatSpriteRenderer;

        [SerializeField] private Sprite expressionNormal;
        [SerializeField] private Sprite expressionEating;
        [SerializeField] private Sprite expressionFrustrated;
        [SerializeField] private Sprite expressionHappy;

        [SerializeField] private GameObject SheepBody;
        [SerializeField] private GameObject SheepShadow;
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

        [SerializeField] private float sheepScaleSpeed = 2;
        private float currentFoodValue;
        [SerializeField] private float startingFoodValue = 1;
        [SerializeField] private float minPristineValue = 10;
        [SerializeField] private float numberOfTotalFoodTillPristineEligable = 30;
        [SerializeField] private float foodMultiplier = .1f;


        [SerializeField] private Color baseColor;
        [SerializeField] private List<foodColorData> foodColorValues;

        [SerializeField] private ParticleSystem[] tearParticlesLeft;
        private Vector3[] locationsOfLeftTears;
        [SerializeField] private ParticleSystem[] tearParticlesRight;
        private Vector3[] locationsOfRightTears;

        //private float tearEmmisionRate = 0;

        [SerializeField] private ParticleSystem mudParticles;
        [SerializeField] private ParticleSystem foodParticles;
        [SerializeField] private ParticleSystem upgradeParticles;
        [SerializeField] private ParticleSystem prestineParticles;
        private bool isInMudThisFrame = false;

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

            locationsOfLeftTears = new Vector3[tearParticlesLeft.Length];
            locationsOfRightTears = new Vector3[tearParticlesRight.Length];

            for (int i = 0; i < tearParticlesLeft.Length; i++)
            {
                var em = tearParticlesLeft[i].emission;
                em.rateOverTime = 0;

                locationsOfLeftTears[i] = tearParticlesLeft[i].transform.localPosition; 
            }

            for (int i = 0; i < tearParticlesRight.Length ; i++)
            {
                var em = tearParticlesLeft[i].emission;
                em.rateOverTime = 0;

                locationsOfRightTears[i] = tearParticlesRight[i].transform.localPosition;
            }

        }

        private void Update()
        {
            updateMudStatus();

            updateAnimator();
            updateExpressions();
            updateParticles();
            changeSize(Time.deltaTime);
        }

        private void OnEnable()
        {
            OnHourChange.OnEvent += OnHourChangeEvent;
            OnMinChange.OnEvent += OnMinChangeEvent;

            StressData.OnMaxValueEvent += OnStressEvent;
            ThirstData.OnMaxValueEvent += OnThirstEvent;
            MuddyData.OnMaxValueEvent += OnMudEvent;

            SheepCamera.enabled = true;
        }

        private void OnDisable()
        {
            OnHourChange.OnEvent -= OnHourChangeEvent;
            OnMinChange.OnEvent -= OnMinChangeEvent;

            StressData.OnMaxValueEvent -= OnStressEvent;
            ThirstData.OnMaxValueEvent -= OnThirstEvent;
            MuddyData.OnMaxValueEvent -= OnMudEvent;

            if (SheepCamera != null)
            {
                SheepCamera.enabled = false;
            }
        }

        private void Awake()
        {
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            OnNightTimeCleanUp.OnEvent += OnNightTimeCleanUpEvent;

            farmerDetector.OnLostDetection.AddListener(OnFarmerLeaveRange);

            lastAteTime = float.MinValue;
            lastExpressionWasSetToHappy = float.MinValue;
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            OnNightTimeCleanUp.OnEvent -= OnNightTimeCleanUpEvent;

            farmerDetector.OnLostDetection.RemoveListener(OnFarmerLeaveRange);

            if (SheepCamera != null)
            {
                Destroy(SheepCamera.gameObject);
            }

        }

        private float lastHitRockTime = 0;
        private void OnCollisionEnter(Collision collision)
        {
            if (isStressed == true && collision.gameObject.TryGetComponent<PlayerLogic>(out PlayerLogic pl))
            {
                rb.isKinematic = true;
            }

            if (lastHitRockTime + 5 < Time.time && collision.gameObject.TryGetComponent<Rockscript>(out Rockscript rock))
            {
                lastHitRockTime = Time.time;

                StressData.changeValue(stressRaisedOnRockHit);
            }
        }


        private void OnTriggerStay(Collider other)
        {
            if((MudLayer.value & (1 << other.gameObject.layer)) > 0)
            {
                isInMudThisFrame = true;
            }
        }



        private Vector3 velocityBeforePause;
        private Vector3 angluarVelocityBeforePause;

        private void OnGameStateChanged(GameState newGameState)
        {
            enabled = newGameState == GameState.Gameplay;
            if (newGameState == GameState.Gameplay)
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

            } else if (newGameState == GameState.Night)
            {
                rb.isKinematic = true;
                velocityBeforePause = Vector3.zero;
                angluarVelocityBeforePause = Vector3.zero;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else if (newGameState == GameState.Paused)
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

        private void updateMudStatus()
        {
            if(isInMudThisFrame == true)
            {
                MuddyData.changeValue(100 * Time.deltaTime);
            }
            else
            {
                MuddyData.changeValue(-(40 * Time.deltaTime));
            }
            isInMudThisFrame = false;

            if (MuddyData.getCurrentPercentage() > .8f )
            {
                rb.mass = 1.85f;
            }
            else
            {
                rb.mass = 1;
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
        
        public void updateAnimator()
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

                for (int i = 0; i < tearParticlesLeft.Length; i++)
                {
                    tearParticlesLeft[i].transform.localPosition = locationsOfLeftTears[i];
                    tearParticlesRight[i].transform.localPosition = locationsOfRightTears[i];
                }
            }
            if (rb.velocity.x < - .1)
            {
                Vector3 temp = visualGameObject.transform.localScale;
                temp.x = -1;
                visualGameObject.transform.localScale = temp;

                for (int i = 0; i < tearParticlesLeft.Length; i++)
                {
                    tearParticlesLeft[i].transform.localPosition = locationsOfRightTears[i];
                    tearParticlesRight[i].transform.localPosition = locationsOfLeftTears[i];
                }

            }
        }

        private float lastExpressionWasSetToHappy;
        private float lastExpresionChange;

        private void updateExpressions()
        {
            if (timeSinceBeingPet + 2f > Time.time)
            {
                expresionSpriteRenderer.sprite = expressionHappy;
                return;
            }

            if (lastExpresionChange + .05f > Time.time)
            {
                return;
            }

            //duration gottem from 
            if (  lastAteTime + 1.5f > Time.time && ( Mathf.Sin( Time.time * 15 )) > .8f )
            {
                expresionSpriteRenderer.sprite = expressionEating;
                lastExpresionChange = Time.time;
                return;
            }
            if ( StressData.getCurrentPercentage() > .8f || MuddyData.getCurrentPercentage() > .8f )
            {
                expresionSpriteRenderer.sprite = expressionFrustrated;
                lastExpresionChange = Time.time;
                return;
            }
         
            if ( lastExpressionWasSetToHappy + 3 > Time.time || Random.Range(0,100) < .7f )
            {
                expresionSpriteRenderer.sprite = expressionHappy;
                lastExpresionChange = Time.time;
                
                if (lastExpressionWasSetToHappy + 3.2 < Time.time)
                {
                    lastExpressionWasSetToHappy = Time.time;
                }
                return;
            }

            expresionSpriteRenderer.sprite = expressionNormal;
            lastExpresionChange = Time.time;
            
        }
        /// <summary>
        /// 
        /// </summary>
        private void updateParticles()
        {
            //print(StressData.getCurrentPercentage());
            if (MuddyData.getCurrentPercentage() == 1f)
            {
                  
                var em = mudParticles.emission;
                em.rateOverTime = 40;
                
            }
            else if (MuddyData.getCurrentPercentage() >= .6f) 
            {
                var em = mudParticles.emission;
                em.rateOverTime = 5;
            }
            else
            {
                var em = mudParticles.emission;
                em.rateOverTime = 0;
                if (mudParticles.isEmitting == true)
                {
                    mudParticles.Clear();
                }
            }

            if (StressData.getCurrentPercentage() == 1f)
            {
                foreach (ParticleSystem tears in tearParticlesLeft)
                {
                    var em = tears.emission;
                    em.rateOverTime = 20;
                }
                foreach (ParticleSystem tears in tearParticlesRight)
                {
                    var em = tears.emission;
                    em.rateOverTime = 20;
                }
            }
            else if (StressData.getCurrentPercentage() >= .8f)
            {
                foreach (ParticleSystem tears in tearParticlesLeft)
                {
                    var em = tears.emission;
                    em.rateOverTime = 4;
                }
                foreach (ParticleSystem tears in tearParticlesRight)
                {
                    var em = tears.emission;
                    em.rateOverTime = 4;
                }
            }
            else
            {
                foreach (ParticleSystem tears in tearParticlesLeft)
                {
                    var em = tears.emission;
                    em.rateOverTime = 0;
                }
                foreach (ParticleSystem tears in tearParticlesRight)
                {
                    var em = tears.emission;
                    em.rateOverTime = 0;
                }
            }



        }

        private float lastAteTime;
        private void eatFood( FoodPickup foodIn )
        {
            currentFoodValue += foodIn.getFoodValue() * foodMultiplier;

            foodParticles.Play();

            lastAteTime = Time.time;

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

            MuddyData.changeMax(foodIn.getFoodValue() * 2);

            foodIn.eatFood();
        }

        private void ChangeTargetSize()
        {
            targetSize.x = currentFoodValue + ((sizeForTasks - 1) * .25f);
            targetSize.y = currentFoodValue + ((sizeForTasks - 1) * .25f);
        }

       // private Vector3 ChangeSizeHelper = new Vector3();\
        private float currentSize;
        private void changeSize(float deltaTime)
        {       
            currentSize = Vector3.Lerp(SheepBody.gameObject.transform.localScale, targetSize, sheepScaleSpeed * deltaTime).x;
            
            Vector3 temp = Vector3.Lerp(SheepBody.gameObject.transform.localScale, targetSize, sheepScaleSpeed * deltaTime);

            SheepBody.gameObject.transform.localScale = temp;
            SheepShadow.gameObject.transform.localScale = temp;

             
            var em = mudParticles.shape;
            em.radius = 2 * currentSize;

            SheepCollider.gameObject.transform.localScale = Vector3.Lerp(SheepCollider.gameObject.transform.localScale, targetSize, sheepScaleSpeed * deltaTime);
            
            SheepCamera.m_Lens.OrthographicSize = 10 + (2 * SheepBody.gameObject.transform.localScale.x);

            //be more clever about this?
            farmerDetector.Sphere.Radius = targetSize.x * 3;

            

            changeBodySprite();
        }

        private void changeBodySprite()
        {
            
            if (sizeForTasks == 1 && currentFoodValue > foodValueForStageTwo)
            {
                //stage 2
                bodySprite.sprite = stageTwoSprite;
                bodyOutlineSprite.sprite = stageTwoOutlineSprite;
                sizeForTasks = 2;
                upgradeParticles.Play();
                ChangeTargetSize();
            }
            else if (sizeForTasks == 2 && currentFoodValue > foodValueForStageThree)
            {
                //stage 3
                bodySprite.sprite = stageThreeSprite;
                bodyOutlineSprite.sprite = stageThreeOutlineSprite;
                sizeForTasks = 3;
                upgradeParticles.Play();
                ChangeTargetSize();
            }
            else if(sizeForTasks == 3 && currentFoodValue > foodValueForStageFour)
            {
                //stage 4
                bodySprite.sprite = stageFourSprite;
                bodyOutlineSprite.sprite = stageFourOutlineSprite;
                SheepCollider.layer = 11;
                sizeForTasks = 4;
                upgradeParticles.Play();
                ChangeTargetSize();
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
                    if (isPristine == false)
                    {
                        //print("What a pristine color sheep");
                        prestineParticles.Play();
                        isPristine = true;
                    }
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
            removeActiveCamera();
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
            LaunchSheep(stressEventForce, true);
        }

        private Vector3 launchVector = new Vector3();
        private Vector2 launchVectorHelper = new Vector2();
        public void LaunchSheep(float power, bool fullRange = false)
        {
            if (fullRange == true)
            {
                launchVectorHelper.x = Random.Range(-1f, 1f);
                launchVectorHelper.y = Random.Range(-1f, 1f);
            }
            else
            {
                launchVectorHelper.x = Random.Range(-.75f, .75f);
                launchVectorHelper.y = Random.Range(0f, 1f);
            }
            
            
            launchVectorHelper = launchVectorHelper.normalized;

            launchVector.x = launchVectorHelper.x * power;
            launchVector.y = .5f;
            launchVector.z = launchVectorHelper.y * power;

            
            rb.AddForce(launchVector );
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
        
        private float timeSinceBeingPet;
        public void Pet()
        {
            //animatorAnimal.SetTrigger("PetEvent");
            timeSinceBeingPet = Time.time;
            if (isStressed)
            {
                StressData.reset();
                rb.isKinematic = false;

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

        public bool doesFullfillTask(Task taskToCheck )
        {
            if (sizeForTasks < (int)taskToCheck.taskSize  )
            {
                return false;
            }

            if(taskToCheck.taskQuality == TASKCOLORQUIALITY.PRISTINE)
            {
                if(isPristine == false)
                {
                    return false;
                }
                if (foodColorValues[0].type != taskToCheck.requiredFood)
                {
                    return false;
                }
            }
            if (taskToCheck.taskQuality == TASKCOLORQUIALITY.PRIME)
            {
                if (foodColorValues[0].type != taskToCheck.requiredFood)
                {
                    return false;
                }
            }
            if (taskToCheck.taskQuality == TASKCOLORQUIALITY.PUDDLE)
            {
                //not 100% sure this is working
                if (!(foodColorValues[0].type == taskToCheck.requiredFood ||
                    foodColorValues[1].type == taskToCheck.requiredFood ||
                    foodColorValues[2].type == taskToCheck.requiredFood))
                {
                    return false;
                }
            }

            return true;
        }

        public void setHat( Sprite hat )
        {
            if (hat != null)
            {
                hatSpriteRenderer.sprite = hat;
            }
        }

        #endregion
    }
}