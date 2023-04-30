using UnityEngine;

[CreateAssetMenu(fileName = "New Music State List", menuName = "CustomObjects/Audio/GroundLayerDependent")]
public class GroundLayerDependentList : SoundStateObject
{
    [SerializeField] private SerializableDictionary<LayerMask, SoundStateObject> sounds;
    private int currentGroundLayer;

    public override AudioClip GetClip() => GetSoundStateObject()?.GetClip();

    public override float GetPauseTime() => GetSoundStateObject().GetPauseTime();

    public override bool StateChanged()
    {
        if (currentGroundLayer != GetGroundLayer())
            return true;
        return GetSoundStateObject().StateChanged();
    }
    private void OnEnable()
    {
        sounds.Update();
    }
    private SoundStateObject GetSoundStateObject()
    {
        currentGroundLayer = GetGroundLayer();
        foreach (LayerMask key in sounds.Keys)
        {
            if (key.Contains(currentGroundLayer))
                return sounds[key];
        }
        return null;
    }
    private int GetGroundLayer()
    {
        GameObject groundObject = PlayerMovement.GroundHitData.collider?.gameObject;
        if (groundObject == null)
            return 0;
        else
            return groundObject.layer;
    }
}
