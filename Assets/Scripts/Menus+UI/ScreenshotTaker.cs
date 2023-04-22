using System;
using UnityEngine;
using UnityEngine.Rendering;

public class ScreenshotTaker : MonoSingleton<ScreenshotTaker>
{
    private Action<byte[]> OnScreenshotTaken;
    private int width;
    private int height;
    private void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += (arg1, arg2) => OnAfterCameraRender();
    }
    private void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= (arg1, arg2) => OnAfterCameraRender();
    }

    private void Start()
    {
        width = Screen.width;
        height = Screen.height;
    }

    private void OnAfterCameraRender()
    {
        if (OnScreenshotTaken == null)
            return;

        Texture2D screenshotTexture = new(width, height, TextureFormat.RGB24, false);
        Rect rect = new(0, 0, width, height);
        screenshotTexture.ReadPixels(rect, 0, 0);
        screenshotTexture.Apply();

        OnScreenshotTaken(screenshotTexture.EncodeToPNG());
        OnScreenshotTaken = null;
    }
    public static void TakeScreenshot(Action<byte[]> callback) => Instance.OnScreenshotTaken += callback;
}
