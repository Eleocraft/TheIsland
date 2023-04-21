using UnityEngine;

public class MapLoadingScreen : MonoSingleton<MapLoadingScreen>
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Rigidbody playerRB;
    public static bool Loading { get; private set; }
    public static void Lock()
    {
        Loading = true;
        Instance.loadingScreen.SetActive(true);
        Instance.playerRB.isKinematic = true;
        CursorStateMachine.ChangeCursorState(false, Instance);
        InputStateMachine.ChangeInputState(false, Instance);
    }
    public static void Unlock()
    {
        Loading = false;
        Instance.loadingScreen.SetActive(false);
        Instance.playerRB.isKinematic = false;
        CursorStateMachine.ChangeCursorState(true, Instance);
        InputStateMachine.ChangeInputState(true, Instance);
    }
}
