using UnityEngine;

public class StepSounds : MonoBehaviour
{
    [SerializeField] private AudioClip[] WalkingSounds;
    [SerializeField] private AudioClip[] RunningSounds;
    [SerializeField] private AnimationEventHandler animationEventHandler;
    private AudioSource StepsSource;
    void Start()
    {
        StepsSource = GetComponent<AudioSource>();
        animationEventHandler.Events["WalkingStep"] += PlayWalkingStep;
        animationEventHandler.Events["RunningStep"] += PlayRunningStep;
    }
    void OnDestroy()
    {
        animationEventHandler.Events["WalkingStep"] -= PlayWalkingStep;
        animationEventHandler.Events["RunningStep"] -= PlayRunningStep;
    }
    public void PlayWalkingStep() => StepsSource.PlayOneShot(WalkingSounds[Random.Range(0, WalkingSounds.Length - 1)]);
    public void PlayRunningStep() => StepsSource.PlayOneShot(RunningSounds[Random.Range(0, RunningSounds.Length - 1)]);
}
