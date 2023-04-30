using UnityEngine;

public class StepSounds : MonoBehaviour
{
    [SerializeField] private SoundStateObject WalkingSounds;
    [SerializeField] private SoundStateObject RunningSounds;
    [SerializeField] private SoundStateObject LiftoffSounds;
    [SerializeField] private SoundStateObject LandingSounds;
    [SerializeField] private AnimationEventHandler animationEventHandler;
    private AudioSource StepsSource;
    void Start()
    {
        StepsSource = GetComponent<AudioSource>();
        animationEventHandler.Events["WalkingStep"] += PlayWalkingStep;
        animationEventHandler.Events["RunningStep"] += PlayRunningStep;
        animationEventHandler.Events["JumpStart"] += PlayLiftoff;
        animationEventHandler.Events["JumpEnd"] += PlayLanding;
    }
    void OnDestroy()
    {
        animationEventHandler.Events["WalkingStep"] -= PlayWalkingStep;
        animationEventHandler.Events["RunningStep"] -= PlayRunningStep;
        animationEventHandler.Events["JumpStart"] -= PlayLiftoff;
        animationEventHandler.Events["JumpEnd"] -= PlayLanding;
    }
    public void PlayWalkingStep()
    {
        AudioClip walkingStep = WalkingSounds.GetClip();
        if (walkingStep != null)
            StepsSource.PlayOneShot(walkingStep);
    }
    public void PlayRunningStep()
    {
        AudioClip runningStep = RunningSounds.GetClip();
        if (runningStep != null)
            StepsSource.PlayOneShot(runningStep);
    }
    public void PlayLiftoff()
    {
        AudioClip liftoffStep = LiftoffSounds.GetClip();
        if (liftoffStep != null)
            StepsSource.PlayOneShot(liftoffStep);
    }
    public void PlayLanding()
    {
        AudioClip landingStep = LandingSounds.GetClip();
        if (landingStep != null)
            StepsSource.PlayOneShot(landingStep);
    }
}
