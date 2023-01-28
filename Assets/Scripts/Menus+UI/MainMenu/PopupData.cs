using UnityEngine;

public class PopupData : MonoSingleton<PopupData>
{
    [SerializeField] private Popup popupPrefab;
    public static Popup PopupPrefab => Instance.popupPrefab;

    [SerializeField] private Transform canvasTransform;
    public static Transform CanvasTransform => Instance.canvasTransform;
}
