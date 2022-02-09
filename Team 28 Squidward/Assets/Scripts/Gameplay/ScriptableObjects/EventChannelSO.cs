using System;
using UnityEngine;

[CreateAssetMenu(menuName = "EventChannelBase/Base")]
public class EventChannelSO : ScriptableObject
{
    public event Action OnEvent;

    public void RaiseEvent()
    {
        OnEvent?.Invoke();
    }

    public int GetNumberOfLiseners()
    {
        return OnEvent.GetInvocationList().Length;
    }

}