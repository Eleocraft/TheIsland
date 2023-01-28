using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

public class SavingGame : MonoSingleton<SavingGame>
{
    [SerializeField] private LoadMode loadMode;
    [SerializeField] private int Seed;

    [SerializeField] private string currentSaveName;

    private static Dictionary<string, PropertyAction<object>> savablePlayerProperties = new();
    private static Dictionary<string, PropertyAction<object>> savableWorldProperties = new();

    protected override void SingletonAwake()
    {
        if (GlobalData.Override == true)
        {
            loadMode = GlobalData.loadMode;
            Seed = GlobalData.Seed;
        }
        else
        {
            SaveAndLoad.SlotName = currentSaveName;

            if (loadMode == LoadMode.Load && !SaveAndLoad.DirectoryExists(LoadCategory.Slot)) // Checking if savefile exists
                loadMode = LoadMode.NewGame;
            
            if (loadMode == LoadMode.NewGame) // Delete The Old saveFile
                SaveAndLoad.DeleteDirectory(LoadCategory.Slot);

            GlobalData.loadMode = loadMode;
            
            SaveAndLoad.Save(new Savegame(DateTime.Now, false), "Savegamedata", LoadCategory.Slot, SerialisationType.Binary);
            if (loadMode == LoadMode.Load)
                GlobalData.Seed = SaveAndLoad.Load<int>("MapSeed", LoadCategory.Slot);
            else
            {
                GlobalData.Seed = Seed;
                SaveAndLoad.Save(Seed, "MapSeed", LoadCategory.Slot, SerialisationType.Binary);
            }
        }
        savablePlayerProperties = new();
        savableWorldProperties = new();
        PropertyInfo[] properties = Assembly.GetExecutingAssembly().GetTypes()
                       .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) // Include instance methods in the search so we can show the developer an error instead of silently not adding instance methods to the dictionary
                       .Where(m => m.GetCustomAttributes(typeof(SaveAttribute), false).Length > 0)
                       .ToArray();
        
        foreach (PropertyInfo property in properties)
        {
            if (!property.SetMethod.IsStatic || !property.GetMethod.IsStatic)
                throw new Exception($"savable properties should be static, but '{property.DeclaringType}.{property.Name}' is an instance method!");
            
            SaveAttribute attribute = property.GetCustomAttribute<SaveAttribute>();
            if (attribute == null)
                continue;
            if (property.PropertyType == typeof(object))
            {
                if (attribute.SaveType == SaveType.player)
                    savablePlayerProperties.Add(string.IsNullOrEmpty(attribute.Name) ? property.Name : attribute.Name, new(property));
                else
                    savableWorldProperties.Add(string.IsNullOrEmpty(attribute.Name) ? property.Name : attribute.Name, new(property));
            }
            else
                throw new Exception($"'{property.DeclaringType}.{property.Name}' is not of type byte[]");
        }
    }
    void Start()
    {
        if (loadMode == LoadMode.Load)
            Load();
        else if (loadMode == LoadMode.NewGame)
            Save();
    }
    // Commands
    [Command("save")]
    public static void SaveCommand(List<string> parameters) => Save();

    public static void Load()
    {
        SaveAndLoad.Load<SaveData>("localPlayer", LoadCategory.Slot).LoadData(SaveType.player);
        SaveData worldData = SaveAndLoad.Load<SaveData>("worldData", LoadCategory.Slot);
        worldData.LoadData(SaveType.world);
    }
    public static void Save()
    {
        ScreenshotTaker.TakeScreenshot((bytes) => SaveAndLoad.SaveBytes(bytes, "saveImage", LoadCategory.Slot));
        SaveAndLoad.Save(new SaveData(SaveType.player), "localPlayer", LoadCategory.Slot, SerialisationType.Binary);
        SaveAndLoad.Save(new SaveData(SaveType.world), "worldData", LoadCategory.Slot, SerialisationType.Binary);
    }

    [Serializable]
    public class SaveData
    {
        private List<string> keys;
        private List<object> data;
        public SaveData(SaveType saveType)
        {
            data = new();
            keys = new();
            foreach (KeyValuePair<string, PropertyAction<object>> savableProperty in (saveType == SaveType.player) ? savablePlayerProperties : savableWorldProperties)
            {
                data.Add(savableProperty.Value.Get());
                keys.Add(savableProperty.Key);
            }
        }

        public void LoadData(SaveType saveType)
        {
            Dictionary<string, PropertyAction<object>> savableProperties = (saveType == SaveType.player) ? savablePlayerProperties : savableWorldProperties;
            for (int i = 0; i < data.Count; i++)
                savableProperties[keys[i]].Set(data[i]);
        }
    }
}

public enum SaveType { player, world }
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class SaveAttribute : Attribute
{
    public readonly SaveType SaveType;
    public readonly string Name;
    public SaveAttribute(SaveType SaveType, string Name = "")
    {
        this.SaveType = SaveType;
        this.Name = Name;
    }
}

[Serializable]
public class SerializableVector2
{
    public float x;
    public float y;
    public SerializableVector2(Vector2 vector)
    {
        x = vector.x;
        y = vector.y;
    }
    public Vector2 getVec() => new(x, y);
}

[Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;
    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    public Vector3 getVec() => new(x, y, z);
}

[Serializable]
public class SerializableQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;
    public SerializableQuaternion(Quaternion quaternion)
    {
        x = quaternion.x;
        y = quaternion.y;
        z = quaternion.z;
        w = quaternion.w;
    }
    public Quaternion getQuat() => new(x, y, z, w);
}