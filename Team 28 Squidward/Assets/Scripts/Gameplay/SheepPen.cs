using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class SheepPen : MonoBehaviour
    {
        private static SheepPen _instance;
        public static SheepPen Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SheepPen();
                }
                return _instance;
            }
        }


        #region Variables
        [Header("Events")]

        [SerializeField]
        private EventChannelSO NightTimeCleanUp;


        [Header("Inspector")]
        [SerializeField] private GameObject SheepPrefab;
        [SerializeField] private Animator SpawnSheepAnimation;
        [SerializeField] private Transform hookPosition;
        [SerializeField] private CinemachineVirtualCamera penCamera;

        [SerializeField] private GameObject sheepSpawnLocation;

        [SerializeField] private TMP_InputField sheepNameText;
        [SerializeField] private string[] DefaultSheepName;

        [SerializeField] private Transform SheepUIContainer;
        [SerializeField] private GameObject sheepUIPrefab;

        private List<Animal> AllTheSheeps = new List<Animal>();
        private List<Animal> ActiveSheep = new List<Animal>();
        private Dictionary<Animal,GameObject> sheepUIBlocks = new Dictionary<Animal,GameObject>();

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _instance = this; 
        }

        private void OnEnable()
        {
            NightTimeCleanUp.OnEvent += OnNightTimeCleanUP_Event;
        }

        private void OnDisable()
        {
            NightTimeCleanUp.OnEvent -= OnNightTimeCleanUP_Event;
        }

        #endregion

        #region Methods

        private void OnNightTimeCleanUP_Event()
        {
            foreach (Animal animal in ActiveSheep)
            {
                animal.gameObject.SetActive(false);
                if (sheepUIBlocks.TryGetValue(animal,out GameObject obj))
                {
                    obj.SetActive(true);
                }
            }
            ActiveSheep.Clear();
        }

        public void FarmerInRange()
        {
            UIAnimator.Instance.OnSheepPenOpen();
        }

        private Animal spawningSheep;
        public void SpawnAnimationOver()
        {
            spawningSheep.transform.position = sheepSpawnLocation.transform.position;
            spawningSheep.gameObject.SetActive(true);
            spawningSheep.LaunchSheep(500);
        }

        public void spawnNewSheep()
        {
            spawningSheep = Instantiate(SheepPrefab, sheepSpawnLocation.transform.position, sheepSpawnLocation.gameObject.transform.rotation, null).GetComponent<Animal>();
            spawningSheep.setHat( UIAnimator.Instance.getSheepHat() );

            if (string.Compare(sheepNameText.text, "") == 0)
            {
                spawningSheep.setName( DefaultSheepName[ Random.Range( 0, DefaultSheepName.Length-1) ] );
            }
            else
            {
                spawningSheep.setName(sheepNameText.text);
            }
            sheepNameText.text = "";
            //spawningSheep.GetComponent<Rigidbody>().AddForce(Vector3.up * 100);

            AllTheSheeps.Add(spawningSheep);
            ActiveSheep.Add(spawningSheep);
            
            //create a ui method

            GameObject uiBlock = Instantiate(sheepUIPrefab,SheepUIContainer);
            uiBlock.GetComponent<SheepUIBlockHelper>().setUp(spawningSheep);
            uiBlock.SetActive(false);

            sheepUIBlocks.Add(spawningSheep, uiBlock);

           
            UIAnimator.Instance.hideSheepPen();
            SpawnSheepAnimation.SetTrigger("SpawnSheep");
        }
        
        public void spawnExistingSheep(Animal animalIn)
        {
            spawningSheep = animalIn;
            ActiveSheep.Add(spawningSheep);

            UIAnimator.Instance.hideSheepPen();
            SpawnSheepAnimation.SetTrigger("SpawnSheep");
        }

        public List<Animal> CheckForValidSheep( Task task  )
        {
            List<Animal> animalsThatFulfillTheTask = new List<Animal>();

            foreach(Animal animal in AllTheSheeps)
            {
                if (animal.doesFullfillTask(task))
                {
                    animalsThatFulfillTheTask.Add(animal);
                    if (sheepUIBlocks.TryGetValue(animal, out GameObject uiBlock) )
                    {
                        sheepUIBlocks.Remove(animal);
                        Destroy(uiBlock);
                    }
                }
            }
            return animalsThatFulfillTheTask;
        }

        private Animal animalToSell;
        public void sellSheep( Animal animalToSellIN )
        {
            
            animalToSell = animalToSellIN;
            penCamera.Priority = 10;

            UIAnimator.Instance.SellSheepAnimationStart();

            SpawnSheepAnimation.SetTrigger("SellSheep");
            
            animalToSell.transform.parent = hookPosition;

            animalToSell.StressData.setRandomInRange();
            animalToSell.updateAnimator();

            animalToSell.enabled = false;
            animalToSell.gameObject.SetActive(true);
            animalToSell.gameObject.transform.localPosition = Vector3.zero;

            if (AllTheSheeps.Contains(animalToSell))
            {
                AllTheSheeps.Remove(animalToSell);
                
            }
            
        }

        public void sellSheepAnimationOver()
        {
            animalToSell.gameObject.SetActive(false);
            animalToSell.transform.parent = null;
            penCamera.Priority = 0;
            UIAnimator.Instance.SellSheepAnimationOver();
            //hookPosition.gameObject.SetActive(false);
            //grand a boon

        }
        
        #endregion
    }
}