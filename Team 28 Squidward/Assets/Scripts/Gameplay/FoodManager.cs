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

           
            for (int i = 0; i < numOfObsticlesToSpawn; i++)
            {
                int ran = Random.Range(0, obsticalPools.Length);

                // I created a rock prefab for you. Right now all the rocks spawn at 0,0,0; 
                GameObject obsticalToSpawn = obsticalPools[ran].getPoolObject();
                obsticalToSpawn.transform.position = FindPosToSpawn();
                allFoodAndObsticals.Add(obsticalToSpawn.GetComponent<PooledObject>());
                //fill in this rest of this
                //method

                // I want you to spawn them in the correct spots. Similiar to how food is spawned. 
                //*Hint using FindPosToSpawn() will tell you where it goes*




                //end of fill area
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

        //[SerializeField] private LayerMask fruitandobstaclemask;
        private bool vailidatePlaceMent()
        {
            
            //dont spawn objects next to the starting area
            if (startLocation != null && Vector3.Distance(posToSpawn, startLocation.transform.position) < safeDistanceToStart)
            {
                return false;
            }

            /// don't spawn objects that overlap over each other

            // / print(posToSpawn);
            ///if (Physics.CheckSphere(posToSpawn, -200f, fruitandobstaclemask))
            //{
            //    return false;
            //}

            foreach (PooledObject pGO in allFoodAndObsticals)
            {
                if (Vector3.Distance(posToSpawn, pGO.gameObject.transform.position) < 10)
                {
                    return false;
                }
            }

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