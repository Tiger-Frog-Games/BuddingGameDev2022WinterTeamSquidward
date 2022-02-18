using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public enum TASKTYPE { SIZE, COLOR }

    public enum TASKRARITY { EASY,MEDIUM,HARD,PRISTINE}

    public class Task 
    {
        #region Variables

        public TASKTYPE taskType;
        public TASKRARITY taskRarity;
        private int timeLeft;

        public bool isSoldThisTurn = false;

        #endregion

        #region Methods
        public Task()
        {
            timeLeft = Random.Range(3, 5);

            System.Type type = typeof(TASKTYPE);
            System.Array values = type.GetEnumValues();

            taskType = (TASKTYPE)values.GetValue(Random.Range(0, values.Length));

            System.Type typeRare = typeof(TASKRARITY);
            System.Array valuesRare = type.GetEnumValues();

            taskRarity = (TASKRARITY)values.GetValue(Random.Range(0, values.Length));


        }

        public bool isTaskOver()
        {
            timeLeft--;

            if (timeLeft <= 0 )
            {
                return true;
            }
            return false;
        }

        public int getDaysLeft()
        {
            return timeLeft;
        }
        
        public string getDescription()
        {
            return "Look I am still makig this";
        }
        #endregion
    }
}