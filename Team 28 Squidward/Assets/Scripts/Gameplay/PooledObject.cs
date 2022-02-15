using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TeamSquidward.Eric
{
    public class PooledObject : MonoBehaviour
    {
        public IObjectPool<GameObject> pool;

        void Start()
        {
            //system = GetComponent<ParticleSystem>();
            if (TryGetComponent<FoodPickup>(out FoodPickup food))
            {
                food.pObj = this;
            }
        }

        [ContextMenu("Do Something")]
        public void returnToPool()
        {
            if (pool == null)
            {
                gameObject.SetActive(false);
            }
            pool.Release(gameObject);
        }
    }
}