using Micosmo.SensorToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class PlayerLogic : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private RangeSensor sheepDetector;

        private Animal currentAnimalPushing;

        #endregion

        #region Unity Methods

        private void Start()
        {
            sheepDetector.OnLostDetection.AddListener(onSheepOutOfRange);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (currentAnimalPushing == null && collision.gameObject.TryGetComponent<Animal>(out Animal animal))
            {
                currentAnimalPushing = animal;
                currentAnimalPushing.setActiveCamera();
            }
        }

        #endregion

        #region Methods

        private void onSheepOutOfRange(GameObject obj, Sensor sens)
        {
            if ( currentAnimalPushing != null && obj.gameObject == currentAnimalPushing.gameObject )
            {
                currentAnimalPushing.removeActiveCamera();
                currentAnimalPushing = null;
            }            
        }

        #endregion
    }
}