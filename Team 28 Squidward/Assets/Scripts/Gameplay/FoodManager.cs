using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class FoodManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private int numOfFruitToSpawn;

        [SerializeField] private PoolGameObject[] foodPools;
        [SerializeField] private PoolGameObject[] obsticalPools;

        [SerializeField] private EventChannelSO OnNightTimeCleanUp;

        private List<PooledObject> allFoodAndObsticals;

        [SerializeField] private GameObject startLocation;

        [SerializeField] private MeshCollider groundCollider;
        #endregion


        #region Unity Methods

        private void Start()
        {
            allFoodAndObsticals = new List<PooledObject>();

            spawnFoodAndObsticals();

        }

        private void Awake()
        {
            OnNightTimeCleanUp.OnEvent += OnNightTimeCleanUpEvent;
        }

        private void OnDestroy()
        {
            OnNightTimeCleanUp.OnEvent -= OnNightTimeCleanUpEvent;
        }

        #endregion

        #region Methods

        private void OnNightTimeCleanUpEvent()
        {
            CleanUpAllFoodAndObsticals();
            spawnFoodAndObsticals();
        }

        private void spawnFoodAndObsticals()
        {
            for (int i = 0; i < numOfFruitToSpawn; i++)
            {
                int ran = Random.Range(0, foodPools.Length);

                GameObject foodToSpawn = foodPools[ran].getPoolObject();
                foodToSpawn.transform.position = FindPosToSpawn();
                allFoodAndObsticals.Add(foodToSpawn.GetComponent<PooledObject>());
            }

            for (int i = 0; i < numOfFruitToSpawn; i++)
            {
                // you will have to create a rock prefab then add a reference to the top of this script
                //fill in this method
                //instiate the rock prefab then place it somewhere 
                //*Hint using FindPosToSpawn() will tell you where it goes*

            }

        }

        private Vector3 posToSpawn = new Vector3();
        public Vector3 FindPosToSpawn()
        {
            do
            {
                posToSpawn.x = Random.Range(groundCollider.bounds.extents.x - 10, -groundCollider.bounds.extents.x + 10);
                posToSpawn.z = Random.Range(groundCollider.bounds.extents.z - 10, -groundCollider.bounds.extents.z + 10);
            } while (startLocation != null && Vector3.Distance(posToSpawn, startLocation.transform.position) < 20);

            return posToSpawn;
        }

        private void CleanUpAllFoodAndObsticals()
        {
            foreach (PooledObject pGO in allFoodAndObsticals)
            {
                pGO.returnToPool();
            }
            allFoodAndObsticals.Clear();
        }


        #endregion
    }
}