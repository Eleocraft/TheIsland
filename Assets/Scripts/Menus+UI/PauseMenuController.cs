using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;

    const string QuitMessage = "Are you sure that you want to quit?\nAll unsaved progress will be lost!";

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
    public void ToggleSettings() => settingsPanel.SetActive(!settingsPanel.activeSelf);

    public void Quit() => Popup.Create(PopupType.YesNo, QuitMessage, () => SceneManager.LoadScene(GlobalData.menuSceneName));
}
