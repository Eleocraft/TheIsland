using UnityEngine;

[CreateAssetMenu(fileName = "New Music List", menuName = "CustomObjects/Audio/MusicList")]
public class MusicList : MusicStateObject
{
    [MinMaxRange(0, 500)] public RangeSlider clipTimer;
    [SerializeField] private AudioClip[] music;
    public override AudioClip GetClip()
    {
        return music[Random.Range(0, music.Length)];
    }
    public override float GetPauseTime()
    {
        return clipTimer.RandomValue();
    }
    public override bool StateChanged() => false;
}