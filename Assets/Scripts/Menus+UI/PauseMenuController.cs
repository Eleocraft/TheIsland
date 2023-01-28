using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;

    void Start()
    {
        EscQueue.pauseMenu += ToggleVisibility;
    }
    void OnDestroy()
    {
        EscQueue.pauseMenu -= ToggleVisibility;
    }

    public void Save() => SavingGame.Save();

    public void ToggleVisibility()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
        if (menuPanel.activeSelf)
        {
            CursorStateMachine.ChangeCursorState(false, this);
            InputStateMachine.ChangeInputState(false, this);
        }
        else
        {
            if (settingsPanel.activeSelf)
                settingsPanel.SetActive(false);
            CursorStateMachine.ChangeCursorState(true, this);
            InputStateMachine.ChangeInputState(true, this);
        }
    }
    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void LoadMainMenu()
    {
        //NetworkManager.Disconnect();
        SceneManager.LoadScene(GlobalData.menuSceneName);
    }
}
