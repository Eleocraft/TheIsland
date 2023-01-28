using UnityEngine;

public class CursorStateMachine : MonoBehaviour
{
    public static InstanceLocker locker = new();
    public static bool Locked => !locker.Locked;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void OnDestroy()
    {
        locker = new();
    }
    public static bool ExeptionLocked(Object lockObj) => locker.ExeptionLocked(lockObj);
    public static void ChangeCursorState(bool locking, Object lockObj)
    {
        if (!locking)
            locker.AddLock(lockObj);
        else if (locker.LockedByObj(lockObj))
            locker.RemoveLock(lockObj);

        if (Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            GlobalData.controls.Player.View.Enable();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            GlobalData.controls.Player.View.Disable();
        }
    }
}
