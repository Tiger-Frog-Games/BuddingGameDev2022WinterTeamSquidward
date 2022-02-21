using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class ParticleSystemAnimationHelper : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle_System;

        public void OnPlayFromAnimation()
        {
            particle_System.Play();
        }
    }
}