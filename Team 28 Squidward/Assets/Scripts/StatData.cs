using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    [Serializable]
    public class StatData
    {
        [SerializeField] private float min;
        [SerializeField] private float max;//warning cant be zero or will crash all the things :D

        [SerializeField] private float start;
        [SerializeField] private float current;

        public event Action OnMaxValueEvent;
        /// <summary>
        ///  int is the current value
        ///  float is the percentage 0-1 of how muddy it is
        /// </summary>
        public event Action<float,float> OnValueChangeEvent;

        public void changeValue(float change)
        {
            if (max == 0)
            {
                max = 100;
            }

            current = Mathf.Clamp(current + change, min, max);

            if (OnValueChangeEvent != null)
            {
                OnValueChangeEvent.Invoke(current, (current / max));
            }
            
            if (OnMaxValueEvent!= null && current == max)
            {
                OnMaxValueEvent?.Invoke();
            }
        }

        public float getCurrentPercentage()
        {
            return (current/max);
        }

        public void reset()
        {
            current = start;
        }

        public void setRandomInRange()
        {
            current = UnityEngine.Random.Range(0, max);
        }

        internal void changeMax(float i)
        {
            max += i;
        }
    }


    [Serializable]
    public class foodColorData
    {
        public FOODTYPE type;
        public float Value;
        public Color color;
    }

}