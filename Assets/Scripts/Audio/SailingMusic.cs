using UnityEngine;

[RequireComponent(typeof(CarryPlayer))]
public class SailingMusic : MonoBehaviour
{
    [SerializeField] private MusicStateList musicStateManager;
    private void Start()
    {
        CarryPlayer cp = GetComponent<CarryPlayer>();
        cp.ChangeActivation += ChangeActivation;
    }
    private void ChangeActivation(bool active)
    {
        if (active)
            musicStateManager.SetMusicState(MusicState.Sailing, this);
        else
            musicStateManager.ResetMusicState(MusicState.Sailing, this);
    }
}
