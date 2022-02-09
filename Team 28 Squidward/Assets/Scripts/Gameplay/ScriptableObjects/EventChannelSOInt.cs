using System;
using UnityEngine;

[CreateAssetMenu(menuName = "EventChannelBase/Int")]
public class EventChannelSOInt : ScriptableObject
{
    public event Action<int> OnEvent;

    public void RaiseEvent(int i)
    {
        OnEvent?.Invoke(i);
    }

    public int GetNumberOfLiseners()
    {
        return OnEvent.GetInvocationList().Length;
    }

}
