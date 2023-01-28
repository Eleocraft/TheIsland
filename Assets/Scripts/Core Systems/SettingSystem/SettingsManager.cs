using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;

public enum SettingCategory { Display, Input }
public class SettingsManager : MonoSingleton<SettingsManager>
{
    private static Dictionary<SettingCategory, Dictionary<string, string>> settings;
    public static Dictionary<SettingCategory, Dictionary<string, string>> Settings => settings;

    private static Dictionary<SettingCategory, Dictionary<string, SettingAccessor>> settingAccessors;

    private static HashSet<SettingID> changedSettings;
    private static HashSet<SettingCategory> changedCategories
    {
        get
        {
            HashSet<SettingCategory> categories = new();
            foreach (SettingID changedSetting in changedSettings)
                categories.Add(changedSetting.Category);
            return categories;
        }
    }
    public static bool HasUnappliedSettings => changedSettings.Count > 0;
    public static SettingAttribute GetSettingAttribute(SettingCategory settingCategory, string setting)
    {
        return settingAccessors[settingCategory][setting].Set.GetMethodInfo().GetCustomAttribute<SettingAttribute>();
    }
    protected override void SingletonAwake()
    {
        DontDestroyOnLoad(this);
        Setup();
        SceneManager.sceneLoaded += SceneSwitch;
    }
    private void Setup()
    {
        settings = new();
        settingAccessors = new();
        changedSettings = new();
        List<SettingCategory> emptySettings = new();
        // Setting up settings array
        foreach (SettingCategory settingCategory in Utility.GetEnumValues<SettingCategory>())
        {
            Dictionary<string, string> setting = new();
            if (SaveAndLoad.TryLoad(out SerializableList<string> settingList, settingCategory.ToString(), LoadCategory.Settings))
                setting = settingList.AsDict();
            else
                emptySettings.Add(settingCategory);
            settings.Add(settingCategory, setting);
            settingAccessors.Add(settingCategory, new());
        }
        // Finding all methods
        MethodInfo[] methods = Assembly.GetExecutingAssembly().GetTypes()
                               .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) // Include instance methods in the search so we can show the developer an error instead of silently not adding instance methods to the dictionary
                               .Where(m => m.GetCustomAttributes(typeof(SettingAttribute), false).Length > 0)
                               .ToArray();
        
        foreach (MethodInfo method in methods)
        {
            if (!method.IsStatic)
                throw new Exception($"Message handler methods should be static, but '{method.DeclaringType}.{method.Name}' is an instance method!");
            
            SettingAttribute attribute = method.GetCustomAttribute<SettingAttribute>();
            if (attribute == null)
                continue;
            SettingCategory settingCategory = attribute.settingCategory;
            ParameterInfo[] parameters = method.GetParameters();
            Action<string> action;
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                action = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), method);
            else
                throw new Exception("Parameter missmatch: setting methods can only have one parameter of type string");
            settingAccessors[settingCategory].Add(attribute.SettingsID, new(action, attribute.loadTime));
            if (emptySettings.Contains(settingCategory))
                settings[settingCategory].Add(attribute.SettingsID, attribute.DefaultValue);
        }
        foreach (SettingCategory settingCategory in Utility.GetEnumValues<SettingCategory>())
            if (emptySettings.Contains(settingCategory))
                SaveAndLoad.Save(settings[settingCategory].AsList(), settingCategory.ToString(), LoadCategory.Settings, SerialisationType.Json);
        
        // Failsave for outdated savefiles
        foreach (SettingCategory settingCategory in Utility.GetEnumValues<SettingCategory>())
        {
            if (!settingAccessors[settingCategory].Keys.ToList().SequenceEqual(settings[settingCategory].Keys.ToList()))
            {
                Debug.LogError("Settings file contents dont match list of settings! reverting to default.");
                Reset();
                return;
            }
        }
        SetAllSettings();
    }
    void SceneSwitch(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GlobalData.mainSceneName)
            SetAllSettings();
        if (HasUnappliedSettings)
            RevertSettings();
    }
    private static void SetAllSettings()
    {
        foreach (SettingCategory settingCategory in Utility.GetEnumValues<SettingCategory>())
            for (int i = 0; i < settingAccessors[settingCategory].Count; i++)
                if (settingAccessors[settingCategory].ElementAt(i).Value.LoadTime == SettingLoadTime.Start || SceneManager.GetActiveScene().name == GlobalData.mainSceneName)
                    settingAccessors[settingCategory].ElementAt(i).Value.Set?.Invoke(settings[settingCategory][settingAccessors[settingCategory].ElementAt(i).Key]);
    }
    public static void ChangeSetting(SettingCategory settingCategory, string setting, string value)
    {
        if (!settings[settingCategory].ContainsKey(setting))
            throw new Exception($"There is no setting with the name {setting} in the category {settingCategory}, check for spelling mistakes!");
        changedSettings.Add(new SettingID(settingCategory, setting));
        settings[settingCategory][setting] = value;
    }
    
    public static void RevertSettings()
    {
        foreach (SettingCategory settingCategory in changedCategories)
            settings[settingCategory] = SaveAndLoad.Load<SerializableList<string>>(settingCategory.ToString(), LoadCategory.Settings).AsDict();
        changedSettings = new();
    }
    public static void ApplySettings()
    {
        SaveSettings();
        foreach (SettingID setting in changedSettings)
            if (settingAccessors[setting.Category][setting.Id].LoadTime == SettingLoadTime.Start || SceneManager.GetActiveScene().name == GlobalData.mainSceneName)
                settingAccessors[setting.Category][setting.Id].Set?.Invoke(settings[setting.Category][setting.Id]);
        changedSettings = new();
        
        static void SaveSettings()
        {
            foreach (SettingCategory settingCategory in changedCategories)
                SaveAndLoad.Save(Settings[settingCategory].AsList(), settingCategory.ToString(), LoadCategory.Settings, SerialisationType.Json);
        }
    }
    public static void Reset()
    {
        changedSettings = new();
        foreach (SettingCategory settingCategory in Utility.GetEnumValues<SettingCategory>())
            SaveAndLoad.DeleteFile(settingCategory.ToString(), LoadCategory.Settings);
        Instance.Setup();
    }
    private struct SettingID
    {
        public readonly SettingCategory Category;
        public readonly string Id;
        public SettingID(SettingCategory Category, string Id)
        {
            this.Category = Category;
            this.Id = Id;
        }
    }
    private struct SettingAccessor
    {
        public readonly Action<string> Set;
        public readonly SettingLoadTime LoadTime;
        public SettingAccessor(Action<string> setDelegate, SettingLoadTime loadTime)
        {
            Set = setDelegate;
            LoadTime = loadTime;
        }
    }
}
public static class SettingsSaveExtensions
{
    // Extension methods for converting Dictionarys
    const string seperator = ">>";
    public static SerializableList<string> AsList(this Dictionary<string, string> dict)
    {
        SerializableList<string> list = new();
        foreach (KeyValuePair<string, string> entry in dict)
            list.Add($"{entry.Key} {seperator} {entry.Value}");
        return list;
    }
    public static Dictionary<string, string> AsDict(this SerializableList<string> list)
    {
        Dictionary<string, string> dict = new();
        foreach (string entry in list.list)
        {
            string trimedEntry = entry.Replace(" ", "");
            string[] keyValuePair = trimedEntry.Split(seperator);
            dict.Add(keyValuePair[0], keyValuePair[1]);
        }
        return dict;
    }
}

public enum SettingLoadTime { Start, MainSceneLoad }
[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public abstract class SettingAttribute : Attribute
{
    readonly private string settingsName;
    readonly private string settingsID;
    readonly private string defaultValue;
    readonly public SettingCategory settingCategory;
    readonly public SettingLoadTime loadTime;
    public abstract SettingObjectType objectType {get;}

    public string SettingsName => settingsName;
    public string DefaultValue => defaultValue;
    public string SettingsID => settingsID;

    public SettingAttribute(SettingCategory settingCategory, SettingLoadTime loadTime, string settingsName, string defaultValue)
    {
        this.settingCategory = settingCategory;
        this.settingsName = settingsName;
        this.defaultValue = defaultValue;
        this.loadTime = loadTime;
        settingsID = Utility.CreateID(settingsName);
    }
}