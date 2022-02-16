using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class DayNightCycleLighting : MonoBehaviour
    {
        #region Variables

        [SerializeField] private EventChannelSOInt OnMinChange;
        [SerializeField] private EventChannelSO OnNightTimeCleanUp;
        [SerializeField] private UnityEngine.Rendering.Universal.Light2D globalLight;

        [SerializeField] private Color MorningColor;
        [SerializeField] private Color NoonColor;
        [SerializeField] private Color NightColor;

        private int startTime;
        private int noonTime;
        private int nightTime;
        #endregion

        private void Awake()
        {
            OnMinChange.OnEvent += OnMinuteChange;
            OnNightTimeCleanUp.OnEvent += OnNightTimeCleanUpEvent;
        }

        private void OnDestroy()
        {
            OnMinChange.OnEvent -= OnMinuteChange;
            OnNightTimeCleanUp.OnEvent -= OnNightTimeCleanUpEvent;
        }

        private void Start()
        {
            OnNightTimeCleanUpEvent();
        }

        #region Unity Methods

        #endregion

        #region Methods

        private void OnMinuteChange(int minInDay)
        {
            //before noon
            if (minInDay < noonTime)
            {
                //print($"{minInDay} - {minInDay - startTime} - {noonTime - startTime} - { (float) (minInDay - startTime) / (noonTime - startTime)} ");
                globalLight.color = Color.Lerp(MorningColor, NoonColor, (float) (minInDay - startTime) / (noonTime - startTime));
            }
            else
            {
                globalLight.color = Color.Lerp(NoonColor, NightColor, (float) (minInDay - noonTime) / (nightTime - noonTime));
            }
        }

        private void OnNightTimeCleanUpEvent()
        {
            globalLight.color = MorningColor;
            startTime = TeamSquidward.Rat.TimeManager.Instance.startingTime;
            noonTime = TeamSquidward.Rat.TimeManager.Instance.noonTime;
            nightTime = TeamSquidward.Rat.TimeManager.Instance.nightTime;

            print($"{startTime} - {noonTime} - {nightTime} ");

        }

        #endregion
    }
}