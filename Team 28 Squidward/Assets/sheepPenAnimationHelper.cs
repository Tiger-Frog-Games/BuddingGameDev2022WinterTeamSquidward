using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class sheepPenAnimationHelper : MonoBehaviour
    {
        public void OnAnimationOver()
        {
            SheepPen.Instance.SpawnAnimationOver();
        }
    }

}
