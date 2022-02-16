using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Rat
{
    public class Rock : MonoBehaviour
    {
        [SerializeField] private float stress;

        private void OnCollisionEnter(Collision collision)
        {
            // check if its a sheep game object - look for animal
            // if it is a animal add stress 
        }
    }
}
