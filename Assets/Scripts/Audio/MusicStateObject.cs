using UnityEngine;

public abstract class MusicStateObject : ScriptableObject
{
    public abstract bool StateChanged();
    public abstract float GetPauseTime();
    public abstract AudioClip GetClip();
}