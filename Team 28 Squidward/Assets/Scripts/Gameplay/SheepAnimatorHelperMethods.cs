using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class SheepAnimatorHelperMethods : MonoBehaviour
    {
        [SerializeField] private Animal ani;
        public void OnStessRollDone()
        {
            ani.OnStressEventRollAnimationDone();
        }
    }
}