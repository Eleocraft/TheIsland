using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneLoader : MonoSingleton<SceneLoader>
{
    [SerializeField] private GameObject loadingPanel;
    public static void Load()
    {
        // if (NetworkManager.networkMode == NetworkMode.Host)
        // {
        //     if (NetworkManager.Connected)
        //         SendSceneLoad(true);
        //     else
        //     {
        //         Debug.LogError("Server not connected");
        //         return;
        //     }
        // }
        GlobalData.Override = true;
        GlobalData.loadMode = LoadMode.Load;

        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(GlobalData.mainSceneName);
        Instance.loadingPanel.SetActive(true);
    }
    public static void NewGame()
    {
        // if (NetworkManager.networkMode == NetworkMode.Host)
        // {
        //     if (NetworkManager.Connected)
        //         SendSceneLoad(false);
        //     else
        //     {
        //         Debug.LogError("Server not connected");
        //         return;
        //     }
        // }
        GlobalData.Override = true;
        GlobalData.loadMode = LoadMode.NewGame;

        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(GlobalData.mainSceneName);
        Instance.loadingPanel.SetActive(true);
    }
    public static void SendSceneLoad(bool Load)
    {
        // Message SceneLoadMessage = Message.Create(MessageSendMode.reliable, (ushort)MessageId.initMessages);
        // SceneLoadMessage.AddBool(Load);
        // SceneLoadMessage.AddInt(GlobalData.Seed);
        // NetworkManager.Send(SceneLoadMessage);
    }
}
