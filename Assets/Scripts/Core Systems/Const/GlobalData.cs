using UnityEngine;

public class GlobalData : MonoBehaviour
{
    public static InputMaster controls;
    // Game start Data
    public static int Seed = 0;
    public static LoadMode loadMode;
    public static bool Override;

    // Scenes
    public const string mainSceneName = "MainScene";
    public const string menuSceneName = "MainMenu";

    void Awake()
    {
        // Global InputMaster
        controls = new InputMaster();
        controls.Enable();
    }
}
