using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuController : MonoSingleton<MainMenuController>
{
    [Header("--Panels")]
    [SerializeField] private GameObject LoadMenu;
    [SerializeField] private GameObject NewGameMenu;
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject BackupMenu;
    [Header("--NewGame")]
    [SerializeField] private TMP_InputField WorldNameInputField;
    [SerializeField] private TMP_InputField SeedInputField;

    [Header("--SaveFileButtons")]
    [SerializeField] private int SaveFileButtonsStartPos;
    [SerializeField] private int SaveFileButtonsDisplacement;
    [SerializeField] private SaveButton SaveFileButton;
    [SerializeField] private RectTransform ScrollRectContent;
    [SerializeField] private int additionalScrollLegth = 10;
    [SerializeField] private int maxSaveFiles = 10;

    [Header("--Backups")]
    
    [SerializeField] private int BackupButtonsStartPos;
    [SerializeField] private int BackupButtonsDisplacement;
    [SerializeField] private BackupButton BackupButton;
    [SerializeField] private RectTransform BackupPanelScrollRectContent;

    [Header("--General")]
    [SerializeField] [Range(30, 144)] private int TargetFrameRate;
    [SerializeField] private GameObject ContinueButton;
    [SerializeField] private GameObject NewGameButton;

    private static List<Savegame> savegames;
    private static List<string> savenames;
    private static List<GameObject> saveFileButtons;
    private static List<GameObject> backupButtons;

    private enum PanelType { None, Settings, LoadMenu, NewGameMenu, BackupMenu }
    private PanelType currentPanel
    {
        set {
            switch (value)
            {
                case PanelType.Settings:
                    SettingsMenu.SetActive(true);
                    LoadMenu.SetActive(false);
                    NewGameMenu.SetActive(false);
                    BackupMenu.SetActive(false);
                    break;
                case PanelType.LoadMenu:
                    SettingsMenu.SetActive(false);
                    LoadMenu.SetActive(true);
                    NewGameMenu.SetActive(false);
                    BackupMenu.SetActive(false);
                    break;
                case PanelType.NewGameMenu:
                    SettingsMenu.SetActive(false);
                    LoadMenu.SetActive(false);
                    NewGameMenu.SetActive(true);
                    BackupMenu.SetActive(false);
                    break;
                case PanelType.BackupMenu:
                    SettingsMenu.SetActive(false);
                    LoadMenu.SetActive(false);
                    NewGameMenu.SetActive(false);
                    BackupMenu.SetActive(true);
                    break;
                case PanelType.None:
                    SettingsMenu.SetActive(false);
                    LoadMenu.SetActive(false);
                    NewGameMenu.SetActive(false);
                    BackupMenu.SetActive(false);
                    break;
            }
        }
    }
    
    protected override void SingletonAwake()
    {
        Cursor.lockState = CursorLockMode.None;
        Application.targetFrameRate = TargetFrameRate;
    }
    void Start()
    {
        savegames = new List<Savegame>();
        savenames = new List<string>();
        string[] dirs = SaveAndLoad.Savefiles;
        foreach (string dir in dirs)
        {
            savegames.Add(SaveAndLoad.Load<Savegame>("Savegamedata", LoadCategory.Slot, dir));
            savenames.Add(dir);
        }
        // Sorting savefiles by date
        for (int i = 0; i < savegames.Count - 1; i++)
        {
            if (savegames[i].date < savegames[i + 1].date)
            {
                savegames.Swap(i, i + 1);
                savenames.Swap(i, i + 1);
                i = -1; //reset i
            }
        }
        if (dirs.Length <= 0)
        {
            ContinueButton.SetActive(false);
            NewGameButton.SetActive(true);
        }

        CreateSaveFileButtons();

        backupButtons = new List<GameObject>();
    }
    private void CreateSaveFileButtons()
    {
        saveFileButtons = new List<GameObject>();
        int position = SaveFileButtonsStartPos;
        for (int i = 0; i < savegames.Count; i++)
        {
            SaveButton button = Instantiate(SaveFileButton, ScrollRectContent);
            button.transform.localPosition = new Vector2(0, position);
            Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            if(SaveAndLoad.TryLoadBytes(out byte[] bytes, "saveImage", LoadCategory.Slot, savenames[i]))
                tex.LoadImage(bytes);
            button.OnInitialisation(savenames[i], savegames[i], i, tex);
            position -= SaveFileButtonsDisplacement;
            saveFileButtons.Add(button.gameObject);
        }
        ScrollRectContent.sizeDelta = new Vector2(100, additionalScrollLegth + SaveFileButtonsStartPos * -1f + savegames.Count * SaveFileButtonsDisplacement);
    }
    public static void OpenBackups(int slot)
    {
        foreach (GameObject backupButton in backupButtons)
            Destroy(backupButton);
        int position = Instance.BackupButtonsStartPos;
        SaveAndLoad.SlotName = savenames[slot];
        string[] backups = SaveAndLoad.Backups;
        for (int i = 0; i < backups.Length; i++)
        {
            BackupButton button = Instantiate(Instance.BackupButton, Instance.BackupPanelScrollRectContent);
            button.transform.localPosition = new Vector2(0, position);
            button.OnInitialisation(backups[i], i);
            position -= Instance.BackupButtonsDisplacement;
            backupButtons.Add(button.gameObject);
        }
        Instance.BackupPanelScrollRectContent.sizeDelta = new Vector2(100, Instance.additionalScrollLegth + Instance.BackupButtonsStartPos * -1f + backups.Length * Instance.BackupButtonsDisplacement);
    }
    public static void LoadBackup(int backupID)
    {
        SaveAndLoad.LoadBackup(SaveAndLoad.Backups[backupID]);
        Instance.currentPanel = PanelType.LoadMenu;
    }
    public static void DeleteSaveFile(int slot)
    {
        SaveAndLoad.DeleteDirectory(LoadCategory.Slot, savenames[slot]);
        savegames.RemoveAt(slot);
        foreach (GameObject button in saveFileButtons)
            Destroy(button);
        Instance.CreateSaveFileButtons();
    }
    public void Quit() => Application.Quit();
    public void Settings() => currentPanel = PanelType.Settings;
    public void ClosePanel() => currentPanel = PanelType.None;
    public void OpenLoadMenu() => currentPanel = PanelType.LoadMenu;
    public void Continue() => Load(0);
    public void OpenNewGameMenu()
    {
        if (savegames.Count >= maxSaveFiles)
        {
            string maxSaveFileCountMessage = $"You can only have {maxSaveFiles} different saves at the same time.\nTo create another savefile first delete an existing one";
            Popup.Create(PopupType.OK, maxSaveFileCountMessage, () => {});
            return;
        }
        currentPanel = PanelType.NewGameMenu;
        SeedInputField.text = UnityEngine.Random.Range(0, 99999).ToString();
    }
    public static void Load(int slot)
    {
        savegames[slot].UpdateData();

        SaveAndLoad.SlotName = savenames[slot];
        GlobalData.Seed = SaveAndLoad.Load<int>("MapSeed", LoadCategory.Slot);

        SaveAndLoad.Save(savegames[slot], "Savegamedata", LoadCategory.Slot, SerialisationType.Binary);
        SceneLoader.Load();
    }
    public void NewGame()
    {
        if (string.IsNullOrEmpty(WorldNameInputField.text))
            return;
        
        Savegame game = new Savegame();
        savegames.Add(game);

        SaveAndLoad.SlotName = WorldNameInputField.text;
        GlobalData.Seed = int.Parse(SeedInputField.text);

        SaveAndLoad.DeleteDirectory(LoadCategory.Slot);
        SaveAndLoad.Save(GlobalData.Seed, "MapSeed", LoadCategory.Slot, SerialisationType.Binary);

        SaveAndLoad.Save(game, "Savegamedata", LoadCategory.Slot, SerialisationType.Binary);
        SceneLoader.NewGame();
    }
}
[Serializable]
public class Savegame
{
    public DateTime date { get; private set; }
    public Savegame()
    {
        this.date = DateTime.Now;
    }
    public void UpdateData()
    {
        this.date = DateTime.Now;
    }
}