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
            spawningSheep.enabled = true;
            spawningSheep.LaunchSheep();
        }

        public void spawnNewSheep()
        {
            spawningSheep = Instantiate(SheepPrefab, sheepSpawnLocation.transform.position, sheepSpawnLocation.gameObject.transform.rotation, null).GetComponent<Animal>();
            
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

           
            UIAnimator.Instance.CloseSheepPen();
            SpawnSheepAnimation.SetTrigger("SpawnSheep");
        }
        
        public void spawnExistingSheep(Animal animalIn)
        {
            spawningSheep = animalIn;
            ActiveSheep.Add(spawningSheep);

            UIAnimator.Instance.CloseSheepPen();
            SpawnSheepAnimation.SetTrigger("SpawnSheep");
        }

        #endregion
    }
}