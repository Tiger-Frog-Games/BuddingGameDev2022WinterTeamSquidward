using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamSquidward.Eric
{
    public class EventChannelBroadcaster : MonoBehaviour
    {
        [SerializeField]
        private EventChannelSO eventChannel;

        public void RaiseEvent()
        {
            eventChannel.RaiseEvent();
        }
    }
}