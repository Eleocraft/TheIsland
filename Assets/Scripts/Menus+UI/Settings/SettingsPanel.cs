using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public enum SettingObjectType { Slider, Checkbox, Dropdown };
public class SettingsPanel : MonoBehaviour
{
    // Enum dictionaries
    [Header("--Panels")]
    [SerializeField] private EnumDictionary<SettingCategory, GameObject> panels;
    [Header("--PanelObjects")]
    [SerializeField] private EnumDictionary<SettingObjectType, SettingPanelObject> panelObjectPrefabs;

    [SerializeField] private int PanelObjectsStartPos;
    [SerializeField] private int additionalScrollLegth = 10;
    // Dictionary that get populated from the lists at start
    private Dictionary<SettingCategory, RectTransform> panelScrollRectContents;
    // Other stuff
    private Dictionary<SettingCategory, List<SettingPanelObject>> settingPanelObjects;
    private SettingCategory _settingPanel = SettingCategory.Display;
    private SettingCategory SettingPanel
    {
        get => _settingPanel;
        set
        {
            panels[_settingPanel].SetActive(false);
            panels[value].SetActive(true);
            _settingPanel = value;
        }
    }
    void Awake()
    {
        panels.Update();
        panelScrollRectContents = new();
        for (int i = 0; i < panels.Count; i++)
            panelScrollRectContents.Add(panels.ElementAt(i).Key, panels.ElementAt(i).Value.transform.GetChild(0).GetChild(0) as RectTransform);
    }
    void OnValidate()
    {
        panels.Update();
        panelObjectPrefabs.Update();
    }
    void Start()
    {
        CreatePanelObjects();
    }
    private void CreatePanelObjects()
    {
        settingPanelObjects = new();
        Dictionary<SettingCategory, Dictionary<string, string>> settings = SettingsManager.Settings;
        foreach (SettingCategory category in Utility.GetEnumValues<SettingCategory>())
        {
            settingPanelObjects.Add(category, new());
            int position = PanelObjectsStartPos;
            for (int i = 0; i < settings[category].Count; i++)
            {
                SettingAttribute objectSettings = SettingsManager.GetSettingAttribute(category, settings[category].ElementAt(i).Key);
                SettingPanelObject panelObject = Instantiate(panelObjectPrefabs[objectSettings.objectType], panels[category].transform);
                panelObject.transform.localPosition = new Vector2(0, position);
                panelObject.OnInitialisation(objectSettings.SettingsName, objectSettings.SettingsID, objectSettings);
                panelObject.ChangeValue(settings[category].ElementAt(i).Value);
                panelObject.SettingChanged += (value) => ChangeSetting(panelObject.Id, value);
                position -= panelObjectPrefabs[objectSettings.objectType].Size;
                settingPanelObjects[category].Add(panelObject);
            }
            panelScrollRectContents[category].sizeDelta = new Vector2(100, additionalScrollLegth + position);
        }
    }
    private void UpdatePanelObjects()
    {
        Dictionary<SettingCategory, Dictionary<string, string>> settings = SettingsManager.Settings;
        foreach (SettingCategory category in Utility.GetEnumValues<SettingCategory>())
            for (int i = 0; i < settings[category].Count; i++)
                settingPanelObjects[category][i].ChangeValue(settings[category].ElementAt(i).Value);
    }
    public void ChangeActivePanel(string newPanel) => SettingPanel = (SettingCategory)Enum.Parse(typeof(SettingCategory), newPanel);
    public void ChangeSetting(string setting, string value) => SettingsManager.ChangeSetting(SettingPanel, setting, value);
    public void ApplySettings() => SettingsManager.ApplySettings();
    public void RevertSettings() => Popup.Create(PopupType.YesNo, "Are you sure you want to undo your changes?", () => {
        SettingsManager.RevertSettings();
        UpdatePanelObjects();
    });
    public void Close()
    {
        if (SettingsManager.HasUnappliedSettings)
            Popup.Create(PopupType.YesNo, "You have unapplied settings, dou you want to apply them?", ApplySettings, RevertSettings);
    }
    public void ResetSettings() => Popup.Create(PopupType.YesNo, "Are you sure you want to reset the settings?", () => {
        SettingsManager.Reset();
        UpdatePanelObjects();
    });
    [Serializable]
    private class SettingPanelObjectPrefab
    {
        [HideInInspector] public string Name = "object";
        [HideInInspector] public SettingObjectType objectType;
        public SettingPanelObject Prefab;
        public SettingPanelObjectPrefab(SettingObjectType objectType)
        {
            this.objectType = objectType;
            Name = objectType.ToString();
        }
    }
}
public abstract class SettingPanelObject : MonoBehaviour
{
    public abstract string Id {get;}
    public abstract void ChangeValue(string newValue);
    public abstract void OnInitialisation(string name, string Id, SettingAttribute objectSetting);
    public abstract event Action<string> SettingChanged;
    public abstract int Size {get;}
}