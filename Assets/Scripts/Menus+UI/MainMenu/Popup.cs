using System;
using UnityEngine;
using TMPro;

public enum PopupType { OK, YesNo }
public class Popup : MonoBehaviour
{
    [SerializeField] private TMP_Text Message;
    [SerializeField] private GameObject YesNoButtons;
    [SerializeField] private GameObject OkButtons;
    private Action callback;
    private Action cancelCallback;
    public void OnInitialisation(PopupType type, string message, Action callback, Action cancelCallback)
    {
        Message.text = message;
        this.callback = callback;
        this.cancelCallback = cancelCallback;
        if (type == PopupType.OK)
            OkButtons.SetActive(true);
        if (type == PopupType.YesNo)
            YesNoButtons.SetActive(true);
    }
    public void Confirm()
    {
        callback?.Invoke();
        Destroy(this.gameObject);
    }
    public void Cancel()
    {
        cancelCallback?.Invoke();
        Destroy(this.gameObject);
    }
    public static void Create(PopupType type, string message, Action callback = default, Action cancelCallback = default)
    {
        Popup popup = Instantiate(PopupData.PopupPrefab, PopupData.CanvasTransform);
        popup.OnInitialisation(type, message, callback, cancelCallback);
    }
}