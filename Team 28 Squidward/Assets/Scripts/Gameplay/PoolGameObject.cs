using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Pool;

namespace TeamSquidward.Eric
{
    // This example spans a random number of ParticleSystems using a pool so that old systems can be reused.
    public class PoolGameObject : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        public enum PoolType
        {
            Stack,
            LinkedList
        }

        public PoolType poolType;

        // Collection checks will throw errors if we try to release an item that is already in the pool.
        public bool collectionChecks = true;
        public int maxPoolSize = 10;

        IObjectPool<GameObject> m_Pool;

        public IObjectPool<GameObject> Pool
        {
            get
            {
                if (m_Pool == null)
                {
                    if (poolType == PoolType.Stack)
                        m_Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, 10, maxPoolSize);
                    else
                        m_Pool = new LinkedPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionChecks, maxPoolSize);
                }
                return m_Pool;
            }
        }

        GameObject CreatePooledItem()
        {
            var go = Instantiate(prefab);// new GameObject("Pooled Particle System");
            go.transform.parent = this.transform;
            // This is used to return ParticleSystems to the pool when they have stopped.
            var returnToPool = go.AddComponent<PooledObject>();
            returnToPool.pool = Pool;

            return go;
        }

        // Called when an item is returned to the pool using Release
        void OnReturnedToPool(GameObject system)
        {
            system.gameObject.SetActive(false);
        }

        // Called when an item is taken from the pool using Get
        void OnTakeFromPool(GameObject system)
        {
            system.gameObject.SetActive(true);
        }

        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        void OnDestroyPoolObject(GameObject system)
        {
            Destroy(system.gameObject);
        }

        public GameObject getPoolObject()
        {
            return Pool.Get();
        }

        [ContextMenu("Do Something")]
        void spawnObject()
        {
            var amount = Random.Range(1, 10);
            for (int i = 0; i < amount; ++i)
            {
                var ps = Pool.Get();
                ps.transform.position = Random.insideUnitSphere * 10;
                
            }
        }
    }
}