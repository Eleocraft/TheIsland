using UnityEngine;

public abstract class IndicatorBar : MonoBehaviour
{
    [SerializeField] private float animationSpeed;
    [SerializeField] private float timeToUpdate;
    protected abstract float MaxProgress { get; }
    private float lastProgress;
    private float animationProgress;
    private float actualProgress;
    private float timer;
    private void Start()
    {
        actualProgress = MaxProgress;
        animationProgress = MaxProgress;
    }
    private void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;
        if (animationProgress > actualProgress && timer <= 0)
        {
            animationProgress = Mathf.MoveTowards(animationProgress, actualProgress, animationSpeed * Time.deltaTime * Mathf.Clamp01((animationProgress - actualProgress) / (lastProgress - actualProgress)));
            SetAnimatorBarSize(animationProgress);
        }
    }
    protected abstract void SetAnimatorBarSize(float progress);
    protected abstract void SetProgressBarSize(float progress);
    public void AnimateProgress(float relProgress)
    {
        relProgress = Mathf.Clamp01(relProgress);
        if (relProgress * MaxProgress > actualProgress)
        {
            SetProgress(relProgress);
            return;
        }
        lastProgress = actualProgress;
        actualProgress = relProgress * MaxProgress;
        SetProgressBarSize(actualProgress);
        timer = timeToUpdate;
    }
    public void SetProgress(float relProgress)
    {
        timer = 0;
        relProgress = Mathf.Clamp01(relProgress);
        actualProgress = relProgress * MaxProgress;
        animationProgress = actualProgress;
        lastProgress = actualProgress;
        SetProgressBarSize(actualProgress);
        SetAnimatorBarSize(animationProgress);
    }
}
