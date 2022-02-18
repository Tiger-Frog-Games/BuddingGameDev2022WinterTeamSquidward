using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace TeamSquidward.Eric
{
    public class TaskUIHelper : MonoBehaviour
    {
        #region Variables

        private Task task;
        [SerializeField] private TMP_Text textDaysLeft;
        [SerializeField] private TMP_Text textDescription;

        #endregion

        #region Methods

        public void populateUI( Task taskIn )
        {
            task = taskIn;

            textDaysLeft.text = task.getDaysLeft().ToString();
            textDescription.text = task.getDescription();

        }

     
        public void cleanUI()
        {
            task = null;

            textDaysLeft.text = "";
            textDescription.text = "";
        }

        public void refreshDaysLeft()
        {
            textDaysLeft.text = task.getDaysLeft().ToString();
        }

        #endregion
    }
}