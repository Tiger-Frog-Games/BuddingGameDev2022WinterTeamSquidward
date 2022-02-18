using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class SheepPen : MonoBehaviour
    {
        #region Variables
        [Header("Events")]

        [SerializeField]
        private EventChannelSO OnDayOverEvent;


        [Header("Inspector")]
        [SerializeField]
        private GameObject SheepPrefab;
        [SerializeField]
        private Animator SpawnSheepAnimation;

        [SerializeField]
        private GameObject sheepSpawnLocation;

        private List<Animal> AllTheSheeps = new List<Animal>();
        private List<Animal> ActiveSheep = new List<Animal>();
        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            OnDayOverEvent.OnEvent += OnDayOver;
        }

        private void OnDisable()
        {
            OnDayOverEvent.OnEvent -= OnDayOver;
        }

        private void OnCollisionEnter(Collision collision)
        {
            print(collision.gameObject.name);
        }

        #endregion

        #region Methods

        private void OnDayOver()
        {
            foreach(Animal animal in ActiveSheep)
            {
                animal.gameObject.SetActive(false);
            }
            ActiveSheep.Clear();
        }

        public void FarmerInRange()
        {
            SpawnSheepAnimation.SetTrigger("SpawnSheep");
            //later will spawn a specific sheep you want
            //for now just spawn the first sheep of AllSheep;
           
        }

        public void SpawnAnimationOver()
        {
            if (AllTheSheeps.Count == 0)
            {
                spawnSheep(-1);
            }
            else
            {
                spawnSheep(AllTheSheeps.Count);
            }
        }

        private void spawnSheep(int indexOfSheep = -1)
        {
            SpawnSheepAnimation.ResetTrigger("SpawnSheep");
            if (indexOfSheep == -1 || ActiveSheep.Count == AllTheSheeps.Count)
            {
                Animal temp = Instantiate(SheepPrefab, sheepSpawnLocation.transform.position,sheepSpawnLocation.gameObject.transform.rotation,null).GetComponent<Animal>();
                AllTheSheeps.Add(temp);
                ActiveSheep.Add(temp);
            }
            else
            {
                if (AllTheSheeps.Count > indexOfSheep && indexOfSheep > 0) {
                    Animal AnimalToSpawnFromPen = AllTheSheeps[indexOfSheep];
                    AnimalToSpawnFromPen.transform.position = this.transform.position;
                }
              
            }
        }

        #endregion
    }
}