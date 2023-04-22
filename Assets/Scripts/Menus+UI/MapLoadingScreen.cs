using UnityEngine;

public class MapLoadingScreen : MonoSingleton<MapLoadingScreen>
{
    [SerializeField] private ImageFadeController loadingScreen;
    [SerializeField] private Rigidbody playerRB;
    [SerializeField] private float fadeTime;
    public static bool Loading { get; private set; }
    protected override void SingletonAwake()
    {
        Loading = true;
    }
    public static void Lock()
    {
        Loading = true;
        Instance.loadingScreen.SetVisible();
        Instance.playerRB.isKinematic = true;
        CursorStateMachine.ChangeCursorState(false, Instance);
        InputStateMachine.ChangeInputState(false, Instance);
    }
    public static void Unlock()
    {
        Loading = false;
        Instance.loadingScreen.StartTimer(0, Instance.fadeTime);
        Instance.playerRB.isKinematic = false;
        CursorStateMachine.ChangeCursorState(true, Instance);
        InputStateMachine.ChangeInputState(true, Instance);
    }
}
