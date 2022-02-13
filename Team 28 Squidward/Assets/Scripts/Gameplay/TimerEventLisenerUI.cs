using TMPro;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class TimerEventLisenerUI : MonoBehaviour
    {
        #region Variables

        [SerializeField] private TMP_Text TimerText;

        [SerializeField] private EventChannelSOInt HourEvent;
        [SerializeField] private EventChannelSOInt MinEvent;

        private int currentHour;
        private int currentMin;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            HourEvent.OnEvent += hourChange;
            MinEvent.OnEvent += minChange;
        }

        private void OnDisable()
        {
            HourEvent.OnEvent -= hourChange;
            MinEvent.OnEvent -= minChange;
        }

        #endregion

        #region Methods

        private void hourChange(int i)
        {
            currentHour = i;
            RefreshText();
        }

        private void minChange(int i)
        {
            currentMin = i;
            RefreshText();
        }

        private void RefreshText()
        {
            int displayMin = currentMin;
            displayMin = displayMin % 60 ;

            int displayHour = currentHour;
            displayHour = displayHour % 12;

            if (displayHour == 0)
            {
                displayHour = 12;
            }

            if (displayMin < 10)
            {
                TimerText.text = $"{displayHour}:0{displayMin}";
            }
            else
            {
                TimerText.text = $"{displayHour}:{displayMin}";
            }
        }

        #endregion
    }
}