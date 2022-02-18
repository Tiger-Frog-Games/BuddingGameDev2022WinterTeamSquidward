using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TeamSquidward.Eric
{
    public class FoodManager : MonoBehaviour
    {
        #region Variables

        [SerializeField] private int numOfFruitToSpawn;
        [SerializeField] private int numOfObsticlesToSpawn;

        [SerializeField] private PoolGameObject[] foodPools;
        [SerializeField] private PoolGameObject[] obsticalPools;

        [SerializeField] private EventChannelSO OnNightTimeCleanUp;

        private List<PooledObject> allFoodAndObsticals;

        [SerializeField] private GameObject startLocation;

        [SerializeField] private MeshCollider groundCollider;
        [SerializeField] private Tilemap tileMap;
        [SerializeField] private TileBase mudTile;
        [SerializeField] private float safeDistanceToStart = 20;
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

            //Food is spawned on the map
            for (int i = 0; i < numOfFruitToSpawn; i++)
            {
                int ran = Random.Range(0, foodPools.Length);

                GameObject foodToSpawn = foodPools[ran].getPoolObject();
                foodToSpawn.transform.position = FindPosToSpawn();
                allFoodAndObsticals.Add(foodToSpawn.GetComponent<PooledObject>());
            }

            //fill in this method
            for (int i = 0; i < numOfObsticlesToSpawn; i++)
            {
                int ran = Random.Range(0, obsticalPools.Length);

                GameObject obsticalToSpawn = obsticalPools[ran].getPoolObject();

                // I created a rock prefab for you. Right now all the rocks spawn at 0,0,0; 
                // I want you to spawn them in the correct spots. Similiar to how food is spawned. 
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
            } while ( vailidatePlaceMent() == false );

            return posToSpawn;
        }

        private bool vailidatePlaceMent()
        {
            
            //dont spawn objects next to the starting area
            if (startLocation != null && Vector3.Distance(posToSpawn, startLocation.transform.position) < safeDistanceToStart)
            {
                return false;
            }

            //Vector3Int helper = new Vector3Int((int)posToSpawn.x, (int)posToSpawn.z, 0);
            //print(helper);
            //Checks to see if it lands on a mud tile
            //print(tileMap.GetTile(helper));
            //if (tileMap.GetTile(helper ) == mudTile )
            //{
            //    return false;
            //}

            //passed all checks
            return true;
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