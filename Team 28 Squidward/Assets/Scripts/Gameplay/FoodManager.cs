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

        [SerializeField] private float holeRadius = 1.0f;
        [SerializeField] private float diskRadius = 2.5f;

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
        }

        private Vector3 posToSpawn = new Vector3();
        public Vector3 FindPosToSpawn()
        {
            posToSpawn.x = Random.Range(groundCollider.bounds.extents.x - 10, -groundCollider.bounds.extents.x +10);
            posToSpawn.z = Random.Range(groundCollider.bounds.extents.z - 10, -groundCollider.bounds.extents.z +10);

            return posToSpawn;
            //Vector3 pos = Random.insideUnitCircle;
            //pos = new Vector3(pos.x, 0, pos.y); // Lay the circle down flat on the ground
            //pos = pos.normalized * (holeRadius + pos.magnitude * (diskRadius - holeRadius));
            //return pos;
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