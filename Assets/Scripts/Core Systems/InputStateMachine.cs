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
    public static bool ExeptionLocked(Object lockObj) => locker.ExeptionLocked(lockObj);
    public static void ChangeInputState(bool active, Object lockObj)
    {
        if (!active)
            locker.AddLock(lockObj);
        else if (locker.LockedByObj(lockObj))
            locker.RemoveLock(lockObj);

        if (Active)
        {
            controls.Player.Enable();
            if (!CursorStateMachine.Locked)
                controls.Player.View.Disable();
            if (PlayerStateManager.State == PlayerState.ThirdPerson)
                controls.PlayerTP.Enable();
            else
                controls.PlayerFP.Enable();
        }
        else
        {
            controls.Player.Disable();
            controls.PlayerFP.Disable();
            controls.PlayerTP.Disable();
        }
    }
}
