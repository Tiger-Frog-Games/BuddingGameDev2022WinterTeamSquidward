using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public enum TASKSIZE { BIG = 2, LARGE = 3, GARGANTUAN = 4}

    public enum TASKCOLORQUIALITY { PUDDLE, PRIME, PRISTINE}

    public class Task 
    {
        #region Variables

        public TASKSIZE taskSize;
        public TASKCOLORQUIALITY taskQuality;
        public FOODTYPE requiredFood;

        private int timeLeft;

        public bool isSoldThisTurn = false;

        #endregion

        #region Methods
        public Task()
        {
            timeLeft = Random.Range(3, 5);

            System.Type type = typeof(TASKSIZE);
            System.Array values = type.GetEnumValues();

            taskSize = (TASKSIZE)values.GetValue(Random.Range(0, values.Length));

            System.Type typeRare = typeof(TASKCOLORQUIALITY);
            System.Array valuesRare = typeRare.GetEnumValues();

            taskQuality = (TASKCOLORQUIALITY)valuesRare.GetValue(Random.Range(0, valuesRare.Length));

            System.Type typeFood = typeof(FOODTYPE);
            System.Array valuesFood = typeFood.GetEnumValues();

            requiredFood = (FOODTYPE)valuesFood.GetValue(Random.Range(0, valuesFood.Length -1 ));

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
            return $"I am looking for a {taskSize}, {taskQuality} sheep, fed with {requiredFood}!";
        }
        #endregion
    }
}