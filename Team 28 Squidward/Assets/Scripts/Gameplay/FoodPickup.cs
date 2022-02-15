using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    
    public enum FOODTYPE { APPLE, CARROT, DADELION, GRAPES, OATS, STARBERRIES, BADPLANT, NONE }

    [SelectionBase]
    public class FoodPickup : MonoBehaviour
    {
        #region Variables

        //[SerializeField]
        //private SpriteRenderer sprite;

        /// <summary>
        /// How much it is worth for the sheep to eat the food
        /// </summary>
        [SerializeField] private FOODTYPE foodType;
        [SerializeField] private float FoodValue;

        public PooledObject pObj;

        #endregion

        #region Unity Methods

        #endregion

        #region Methods

        public void eatFood()
        {
            //sprite.enabled = false;
            if (pObj == null)
            {
                gameObject.SetActive(false);
                return;
            }
            pObj.returnToPool();
        }

        public FOODTYPE getFoodType()
        {
            return foodType;
        }

        public float getFoodValue()
        {
            return FoodValue;
        }
        #endregion
    }
}