using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Performance : MonoSingleton<Performance>
{
    [SerializeField] private bool active;
    [SerializeField] private TMP_Text text;
    private float timer;
    private float lastTime;
    private int frameCount;
    void Update()
    {
        if (!active)
            return;
        timer -= Time.deltaTime;
        frameCount++;
        if (timer < 0)
        {
            text.text = (1f / ((Time.time - lastTime) / frameCount)).ToString();
            timer = 5f;
            lastTime = Time.time;
            frameCount = 0;
        }
    }
    [Command]
    public static void showFPS(List<string> args)
    {
        Instance.active = !Instance.active;
        Instance.text.gameObject.SetActive(Instance.active);
    }
}
