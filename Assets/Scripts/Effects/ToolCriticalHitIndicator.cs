using UnityEngine;

public class ToolCriticalHitIndicator : MonoBehaviour
{
    private float timer;
    private float fullTimer;
    [SerializeField] private MeshRenderer IndicatorRenderer;

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            IndicatorRenderer.material.SetFloat("_t", timer / fullTimer);
            if (timer <= 0)
                IndicatorRenderer.gameObject.SetActive(false);
        }
    }
    public void CritChance(float timerLength)
    {
        fullTimer = timerLength;
        timer = timerLength;
        IndicatorRenderer.material.SetFloat("_t", 1);
        IndicatorRenderer.gameObject.SetActive(true);
    }
}
