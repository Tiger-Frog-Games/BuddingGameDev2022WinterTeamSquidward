using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class sheepPenAnimationHelper : MonoBehaviour
    {
        public void OnSpawnAnimationOver()
        {
            SheepPen.Instance.SpawnAnimationOver();
        }

        public void EndSellSheepAnimation()
        {
            SheepPen.Instance.sellSheepAnimationOver();
        }
    }

}
