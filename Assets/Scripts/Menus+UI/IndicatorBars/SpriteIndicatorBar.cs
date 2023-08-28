using UnityEngine;

public class SpriteIndicatorBar : IndicatorBar
{
    [SerializeField] private SpriteRenderer progressBar;
    [SerializeField] private SpriteRenderer animationBar;
    private float maxProgress;
    protected override float MaxProgress => maxProgress;

    private void Awake()
    {
        maxProgress = progressBar.size.x;
    }
    protected override void SetAnimatorBarSize(float progress)
    {
        animationBar.size = new Vector2(progress, animationBar.size.y);
    }
    protected override void SetProgressBarSize(float progress)
    {
        progressBar.size = new Vector2(progress, progressBar.size.y);
    }
}
