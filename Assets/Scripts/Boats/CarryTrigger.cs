using UnityEngine;
using System;

public class CarryTrigger : MonoBehaviour
{
    public event Action PlayerEntered;
    public event Action PlayerExited;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "PlayerCarryTrigger")
            PlayerEntered?.Invoke();
    }
    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "PlayerCarryTrigger")
            PlayerExited?.Invoke();
    }
}
