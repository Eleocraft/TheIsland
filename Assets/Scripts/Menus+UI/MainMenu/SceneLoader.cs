using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneLoader : MonoSingleton<SceneLoader>
{
    [SerializeField] private GameObject loadingPanel;
    public static void Load()
    {
        GlobalData.Override = true;
        GlobalData.loadMode = LoadMode.Load;

        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(GlobalData.mainSceneName);
        Instance.loadingPanel.SetActive(true);
    }
    public static void NewGame()
    {
        GlobalData.Override = true;
        GlobalData.loadMode = LoadMode.NewGame;

        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(GlobalData.mainSceneName);
        Instance.loadingPanel.SetActive(true);
    }
}
