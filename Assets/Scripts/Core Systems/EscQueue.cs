using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class EscQueue : MonoBehaviour
{
    private static List<Action> escQueue;
    public static Action pauseMenu;
    private void Awake()
    {
        escQueue = new();
    }
    private void Start()
    {
        GlobalData.controls.Menus.Escape.performed += Dequeue;
    }
    private void OnDestroy()
    {
        GlobalData.controls.Menus.Escape.performed -= Dequeue;
    }
    private static void Dequeue(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (escQueue.Count > 0)
            escQueue.Last()?.Invoke();
        else
            pauseMenu?.Invoke();
    }
    public static void Enqueue(Action callback)
    {
        escQueue.Add(callback);
    }
    public static void Remove(Action callback)
    {
        escQueue.Remove(callback);
    }
}
