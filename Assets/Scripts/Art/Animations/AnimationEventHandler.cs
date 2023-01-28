using UnityEngine;
using System.Collections.Generic;
using System;

public class AnimationEventHandler : MonoBehaviour
{
    private Dictionary<string, Action> _events;
    public Dictionary<string, Action> Events
    {
        get {
            if (_events == null)
                Awake();
            return _events;
        }
    }
    [SerializeField] private List<string> eventNames;
    void Awake()
    {
        if (_events != null)
            return;
        _events = new();
        foreach (string name in eventNames)
            _events.Add(name, null);
    }

    public void Event(string EventName) => Events[EventName]?.Invoke();
}
