using UnityEngine;
using Unity.Netcode;

public class PlayerAnimationManager : MonoSingleton<PlayerAnimationManager>
{
    [SerializeField] private Animator fullBodyAnimator;
    //[SerializeField] private Animator armAnimator;
    private Animator networkPlayerAnimator;

    public static void Play(string name)
    {
        Instance.fullBodyAnimator.Play(name);
        Instance.networkPlayerAnimator?.Play(name);
    }
    // public static void PlayStateDependent(string name)
    // {
    //     if (PlayerStateManager.State == PlayerState.FirstPerson)
    //         Instance.armAnimator.Play(name);
        
    //     else if (PlayerStateManager.State == PlayerState.ThirdPerson)
    //     {
    //         Instance.fullBodyAnimator.Play(name);
    //         Instance.networkPlayerAnimator?.Play(name);
    //     }
    // }
    public static void SetFloat(string name, float value)
    {
        Instance.fullBodyAnimator.SetFloat(name, value);
        Instance.networkPlayerAnimator?.SetFloat(name, value);
    }
    public static void SetTrigger(string name)
    {
        Instance.fullBodyAnimator.SetTrigger(name);
        Instance.networkPlayerAnimator?.SetTrigger(name);
    }

    public static void SetPlayerNetworkAnimator(Animator networkPlayerAnimator)
    {
        Instance.networkPlayerAnimator = networkPlayerAnimator;
    }
}
