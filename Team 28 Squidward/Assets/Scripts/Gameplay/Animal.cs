using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class Animal : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private CinemachineVirtualCamera SheepCamera;

        [SerializeField]
        private float foodEaten;

        [SerializeField]
        private float startingSize = 1;
        [SerializeField]
        private float foodMultiplier = .1f;


        private float currentSize;

        #endregion

        #region Unity Methods

        private void Start()
        {
            currentSize = startingSize;
            SheepCamera.gameObject.transform.SetParent(null);
            SheepCamera.transform.Rotate(90, 0, 0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if ( other.TryGetComponent<FoodPickup>(out FoodPickup eatingFood) )
            {
                eatFood(eatingFood);
            }
        }

        #endregion

        #region Methods

        private void eatFood( FoodPickup foodIn )
        {
            foodEaten += foodIn.getFoodValue();
            changeSize();
            foodIn.eatFood();
        }

        private void changeSize()
        {
            currentSize = startingSize + (foodEaten * foodMultiplier);

            SheepCamera.m_Lens.OrthographicSize = 10 + currentSize;

            this.transform.localScale = new Vector3(currentSize,currentSize,currentSize);
        }

        public void setActiveCamera()
        {
            SheepCamera.Priority = 10;
        }

        public void removeActiveCamera()
        {
            SheepCamera.Priority = 0;
        }


        #endregion
    }
}