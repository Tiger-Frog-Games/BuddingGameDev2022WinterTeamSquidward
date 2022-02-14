using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class DonePettingSheepHelper : MonoBehaviour
    {
        [SerializeField] private PlayerLogic pl;
        public void DonePettingSheep()
        {
            pl.DonePettingSheep();
        }

        public void DoneBrushingSheep()
        {
            pl.DoneBrushingSheep();
        }
    }
}
