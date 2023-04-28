using UnityEngine;

public class InputStateMachine : MonoBehaviour
{
    public static InstanceLocker locker = new();
    public static bool Active => !locker.Locked;
    private static InputMaster controls;
    void Start()
    {
        controls = GlobalData.controls;
    }
    void OnDestroy()
    {
        locker = new();
    }
    public static bool AlreadyLocked(Object lockObj) => locker.AlreadyLocked(lockObj);
    public static void ChangeInputState(bool active, Object lockObj)
    {
        if (!active)
            locker.AddLock(lockObj);
        else if (locker.LockedByObj(lockObj))
            locker.RemoveLock(lockObj);

        if (Active)
            controls.Player.Enable();
        else
            controls.Player.Disable();
    }
}
