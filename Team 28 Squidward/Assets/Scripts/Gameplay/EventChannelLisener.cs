using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TeamSquidward.Eric
{
    public class EventChannelLisener : MonoBehaviour
    {
        [SerializeField]
        private EventChannelSO eventChannel;
        [SerializeField]
        private UnityEvent unityEvent;

        private void OnEnable()
        {
            eventChannel.OnEvent += OnEventChannelTriggered;
        }

        private void OnDisable()
        {
            eventChannel.OnEvent -= OnEventChannelTriggered;
        }

        private void OnEventChannelTriggered()
        {
            unityEvent.Invoke();
        }

    }
}