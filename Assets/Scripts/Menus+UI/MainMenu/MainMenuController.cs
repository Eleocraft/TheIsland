using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class MainMenuController : MonoSingleton<MainMenuController>
{
    [Header("--Menus")]
    [SerializeField] private GameObject TopLVLMenu;
    [SerializeField] private GameObject HostClientSelect;
    [SerializeField] private GameObject MultiplayerHUB;
    [SerializeField] private GameObject ConnectingPanel;
    [Header("--Buttons")]
    [SerializeField] private GameObject MultiplayerStartButton;
    [Header("--Panels")]
    [SerializeField] private GameObject LoadMenu;
    [SerializeField] private GameObject NewGameMenu;
    [SerializeField] private GameObject SettingsMenu;
    [Header("--NewGame")]
    [SerializeField] private TMP_InputField WorldNameInputField;
    [SerializeField] private TMP_InputField SeedInputField;
    [Header("--Multiplayer")]
    [SerializeField] private TMP_InputField IPInputField;
    [SerializeField] private TMP_InputField UsernameInputField;
    [Header("--SaveFileButtons")]
    [SerializeField] private int SaveFileButtonsStartPos;
    [SerializeField] private int SaveFileButtonsDisplacement;
    [SerializeField] private SaveButton SaveFileButton;
    [SerializeField] private RectTransform ScrollRectContent;
    [SerializeField] private int additionalScrollLegth = 10;
    [SerializeField] private int maxSaveFiles = 10;
    [Header("--PlayerList")]
    [SerializeField] private GameObject PlayerListPanel;
    [SerializeField] private TMP_Text ConnectionPrefabs;
    [SerializeField] private int ConnectionListStartPos;
    [SerializeField] private int ConnectionListDisplacement;
    
    [Header("--General")]
    [SerializeField] [Range(30, 144)] private int TargetFrameRate;

    private static List<Savegame> savegames;
    private static List<string> savenames;
    private static List<GameObject> saveFileButtons;
    private static List<GameObject> clientConnections = new List<GameObject>();
    private static bool multiplayer;

    private const string connectionClosedErrorMessage = "The server closed the connection";
    private const string connectionErrorMessage = "The client could not connect to the server.\nThis could be due to a wrong IP-adress, or to the username already being taken up on the server.";

    private enum PanelType { None, Settings, LoadMenu, NewGameMenu }
    private PanelType currentPanel
    {
        set {
            switch (value)
            {
                case PanelType.Settings:
                    SettingsMenu.SetActive(true);
                    LoadMenu.SetActive(false);
                    NewGameMenu.SetActive(false);
                    break;
                case PanelType.LoadMenu:
                    SettingsMenu.SetActive(false);
                    LoadMenu.SetActive(true);
                    NewGameMenu.SetActive(false);
                    break;
                case PanelType.NewGameMenu:
                    SettingsMenu.SetActive(false);
                    LoadMenu.SetActive(false);
                    NewGameMenu.SetActive(true);
                    break;
                case PanelType.None:
                    SettingsMenu.SetActive(false);
                    LoadMenu.SetActive(false);
                    NewGameMenu.SetActive(false);
                    break;
            }
            if (value == PanelType.None)
                PlayerListPanel.SetActive(true);
            else
                PlayerListPanel.SetActive(false);
        }
    }
    private enum MenuType { TopLVL, HostClientSelect, MultiplayerHUB, ConnectingPanel }
    private MenuType currentMenu
    {
        set {
            switch (value)
            {
                case MenuType.TopLVL:
                    TopLVLMenu.SetActive(true);
                    HostClientSelect.SetActive(false);
                    MultiplayerHUB.SetActive(false);
                    ConnectingPanel.SetActive(false);
                    break;
                case MenuType.HostClientSelect:
                    TopLVLMenu.SetActive(false);
                    HostClientSelect.SetActive(true);
                    MultiplayerHUB.SetActive(false);
                    ConnectingPanel.SetActive(false);
                    break;
                case MenuType.MultiplayerHUB:
                    TopLVLMenu.SetActive(false);
                    HostClientSelect.SetActive(false);
                    MultiplayerHUB.SetActive(true);
                    ConnectingPanel.SetActive(false);
                    break;
                case MenuType.ConnectingPanel:
                    TopLVLMenu.SetActive(false);
                    HostClientSelect.SetActive(false);
                    MultiplayerHUB.SetActive(false);
                    ConnectingPanel.SetActive(true);
                    break;
            }
            // if (value == MenuType.TopLVL && NetworkManager.Connected)
            //     NetworkManager.Disconnect();
            currentPanel = PanelType.None;
        }
    }

    protected override void SingletonAwake()
    {
        Cursor.lockState = CursorLockMode.None;
        Application.targetFrameRate = TargetFrameRate;
    }
    void Start()
    {
        try { UsernameInputField.text = SaveAndLoad.Load<string>("Username", LoadCategory.Settings); }
        catch (noSaveFileFoundExeption) { }
        try { IPInputField.text = SaveAndLoad.Load<string>("IP", LoadCategory.Settings); }
        catch (noSaveFileFoundExeption) { }

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
        // NetworkManager.ConnectionEstablished += ConfirmConnection;
        // NetworkManager.ConnectionAborted += ConnectionAborted;
        // NetworkManager.ConnectionsAmountChanged += UpdateClientList;

        // if (NetworkManager.ConnectionError)
        //     Popup.Create(PopupType.OK, connectionClosedErrorMessage, () => {});

        CreateSaveFileButtons();
    }
    void OnDestroy()
    {
        // NetworkManager.ConnectionEstablished -= ConfirmConnection;
        // NetworkManager.ConnectionAborted -= ConnectionAborted;
        // NetworkManager.ConnectionsAmountChanged -= UpdateClientList;
    }
    // private void UpdateClientList()
    // {
    //     foreach (GameObject con in clientConnections)
    //         Destroy(con);
    //     clientConnections = new List<GameObject>();
    //     int position = ConnectionListStartPos;
    //     List<string> Usernames = NetworkManager.Usernames.Values.ToList();
    //     Usernames.Insert(0, NetworkManager.Username);
    //     for (int i = 0; i < Usernames.Count; i++)
    //     {
    //         TMP_Text connection = Instantiate(ConnectionPrefabs, PlayerListPanel.transform);
    //         connection.text = Usernames[i];
    //         connection.transform.localPosition = new Vector2(0, position);
    //         position -= ConnectionListDisplacement;
    //         clientConnections.Add(connection.gameObject);
    //     }
    // }
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
    public void Back() => currentMenu = MenuType.TopLVL;
    public void ConfirmConnection() => currentMenu = MenuType.MultiplayerHUB;
    private void ConnectionAborted()
    {
        Popup.Create(PopupType.OK, connectionErrorMessage, Back);
    }
    public void ConnectHost()
    {
        if (string.IsNullOrEmpty(UsernameInputField.text))
        {
            Popup.Create(PopupType.OK, "you need to input a username", () => {});
            return;
        }
        currentMenu = MenuType.MultiplayerHUB;
        MultiplayerStartButton.SetActive(true);
        SaveAndLoad.Save(UsernameInputField.text, "Username", LoadCategory.Settings, SerialisationType.Json);
        SaveAndLoad.Save(IPInputField.text, "IP", LoadCategory.Settings, SerialisationType.Json);
        // NetworkManager.Connect(NetworkMode.Host, IPInputField.text, UsernameInputField.text);
        // UpdateClientList();
    }
    public void ConnectClient()
    {
        if (string.IsNullOrEmpty(UsernameInputField.text))
        {
            Popup.Create(PopupType.OK, "you need to input a username", () => {});
            return;
        }
        currentMenu = MenuType.ConnectingPanel;
        MultiplayerStartButton.SetActive(false);
        SaveAndLoad.Save(UsernameInputField.text, "Username", LoadCategory.Settings, SerialisationType.Json);
        SaveAndLoad.Save(IPInputField.text, "IP", LoadCategory.Settings, SerialisationType.Json);
        // NetworkManager.Connect(NetworkMode.Client, IPInputField.text, UsernameInputField.text);
        // UpdateClientList();
    }
    public void SingleplayerInit()
    {
        currentPanel = PanelType.LoadMenu;
        multiplayer = false;
    }
    public void MultiplayerInit()
    {
        currentMenu = MenuType.HostClientSelect;
        multiplayer = true;
    }
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
        savegames[slot].UpdateData(DateTime.Now, multiplayer);

        SaveAndLoad.SlotName = savenames[slot];
        GlobalData.Seed = SaveAndLoad.Load<int>("MapSeed", LoadCategory.Slot);

        SaveAndLoad.Save(savegames[slot], "Savegamedata", LoadCategory.Slot, SerialisationType.Binary);
        SceneLoader.Load();
    }
    public void NewGame()
    {
        if (string.IsNullOrEmpty(WorldNameInputField.text))
            return;
        
        Savegame game = new Savegame(DateTime.Now, multiplayer);
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
    public bool multiplayer { get; private set; }
    public Savegame(DateTime date, bool multiplayer)
    {
        this.date = date;
        this.multiplayer = multiplayer;
    }
    public void UpdateData(DateTime date, bool multiplayer)
    {
        this.date = date;
        this.multiplayer = multiplayer;
    }
}