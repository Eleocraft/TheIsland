using UnityEngine;

public class PlayerAnimationManager : MonoSingleton<PlayerAnimationManager>
{
    [SerializeField] private Animator fullBodyAnimator;
    //[SerializeField] private Animator armAnimator;

    public static void Play(string name)
    {
        Instance.fullBodyAnimator.Play(name);
    }
    // public static void PlayStateDependent(string name)
    // {
    //     if (PlayerStateManager.State == PlayerState.FirstPerson)
    //         Instance.armAnimator.Play(name);
        
    //     else if (PlayerStateManager.State == PlayerState.ThirdPerson)
    //     {
    //         Instance.fullBodyAnimator.Play(name);
    //     }
    // }
    public static void SetFloat(string name, float value)
    {
        Instance.fullBodyAnimator.SetFloat(name, value);
    }
    public static void SetTrigger(string name)
    {
        Instance.fullBodyAnimator.SetTrigger(name);
    }
}
