using UnityEngine;

public class CanvasIndicatorBar : IndicatorBar
{
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform animationBar;

    private float maxProgress;
    protected override float MaxProgress => maxProgress;

    private void Awake()
    {
        maxProgress = progressBar.sizeDelta.x;
    }
    protected override void SetAnimatorBarSize(float progress)
    {
        animationBar.sizeDelta = new Vector2(progress, animationBar.sizeDelta.y);
    }
    protected override void SetProgressBarSize(float progress)
    {
        progressBar.sizeDelta = new Vector2(progress, progressBar.sizeDelta.y);
    }
}
