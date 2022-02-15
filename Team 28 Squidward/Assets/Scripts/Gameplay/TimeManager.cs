using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Ash
{
    public class TimeManager : MonoBehaviour
    {
        #region Variables
        [SerializeField]
        private EventChannelSOInt OnHourChange;
        [SerializeField]
        private EventChannelSOInt OnMinChange;
        public float currentTime;


        #endregion

        #region Unity Methods
        private void Start()
        {
            currentTime = Time.time;
        }
        private void Update()
        {
            currentTime += Time.deltaTime;
        }
        #endregion

        #region Methods

        #endregion
    }
}