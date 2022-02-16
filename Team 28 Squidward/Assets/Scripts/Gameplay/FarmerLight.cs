using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TeamSquidward.Eric
{
    public class FarmerLight : MonoBehaviour
    {
        #region Variables
        [SerializeField] private EventChannelSOInt OnMinChange;
        [SerializeField] private Light2D farmerLight;

        [SerializeField] int turnOffMin;
        [SerializeField] int turnOffMax;
        #endregion

        #region Unity Methods

        private void Awake()
        {
            OnMinChange.OnEvent += OnMinChange_OnEvent;
        }

        private void OnDestroy()
        {
            OnMinChange.OnEvent -= OnMinChange_OnEvent;
        }

        #endregion

        #region Methods

        private void OnMinChange_OnEvent(int minChange)
        {
            //print($"{turnOffMin} - {minChange} - {turnOffMax}");

            if ( turnOffMin < minChange && minChange < turnOffMax )
            {
                farmerLight.enabled = false;
            }
            else
            {
                farmerLight.enabled = true;
            }
        }

        #endregion
    }
}