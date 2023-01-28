using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public enum LoadMode { NewGame, Load, TestMap }
public enum LoadCategory { Slot, Map, Settings }
public enum SerialisationType { Binary, Json }
public static class SaveAndLoad
{
    private static string pathRoot = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/TheIsland";
    private static string _overrideSlotName;
    public static string SlotName
    {
        get => _overrideSlotName;
        set {
            if (string.IsNullOrEmpty(value))
                value = "default";
            _overrideSlotName = value;
        }
    }
    // Constant variables
    private const string mapSaveExtension = ".mapdata";
    private const string settingsSaveExtension = ".settings";
    private const string JsonSaveExtension = ".Rsavegame";
    private const string BinarySaveExtension = ".savegame";

    public static string[] Savefiles
    {
        get {
            string[] dirs = Directory.GetDirectories($"{pathRoot}/Savegames/");
            for (int i = 0; i < dirs.Length; i++)
                dirs[i] = new DirectoryInfo(dirs[i]).Name;
            
            return dirs;
        }
    }

    public static void DeleteDirectory(LoadCategory category, string overrideSlotName = "")
    {
        string path = "";
        if (category == LoadCategory.Slot)
            path = $"{pathRoot}/Savegames/{(string.IsNullOrEmpty(overrideSlotName) ? SlotName : overrideSlotName)}/";
        else if (category == LoadCategory.Map)
            path = $"{pathRoot}/Maps/{GlobalData.Seed}/";
        else if (category == LoadCategory.Settings)
            path = $"{pathRoot}/Settings/";
        
        if (Directory.Exists(Path.GetDirectoryName(path)))
            Directory.Delete(Path.GetDirectoryName(path), true);
    }
    public static void DeleteFile(string SaveName, LoadCategory category, string overrideSlotName = "")
    {
        File.Delete(GetPath(SaveName, category, overrideSlotName));
    }

    public static bool DirectoryExists(LoadCategory category, string overrideSlotName = "")
    {
        string path = "";
        if (category == LoadCategory.Slot)
            path = $"{pathRoot}/Savegames/{(string.IsNullOrEmpty(overrideSlotName) ? SlotName : overrideSlotName)}/";
        else if (category == LoadCategory.Map)
            path = $"{pathRoot}/Maps/{GlobalData.Seed}/";
        else if (category == LoadCategory.Settings)
            path = $"{pathRoot}/Settings/";
        
        return Directory.Exists(Path.GetDirectoryName(path));
    }
    public static bool FileExists(string SaveName, LoadCategory category, string overrideSlotName="")
    {
        try 
        {
            GetPath(SaveName, category, overrideSlotName);
            return true;
        }
        catch (noSaveFileFoundExeption)
        {
            return false;
        }
    }

    public static byte[] SerializeBinary(object data)
    {
        BinaryFormatter formatter = new();
        MemoryStream stream = new();
        formatter.Serialize(stream, data);
        return stream.ToArray();
    }

    public static object DeserializeBinary(byte[] bytes)
    {
        BinaryFormatter formatter = new();
        MemoryStream stream = new();
        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
        return formatter.Deserialize(stream);
    }

    // Method that gets the Path where to save the Data
    static string GetPath(string SaveName, LoadCategory category, string overrideSlotName = "")
    {
        string path = "";
        if (category == LoadCategory.Slot)
        {
            if (File.Exists($"{pathRoot}/Savegames/{(string.IsNullOrEmpty(overrideSlotName) ? SlotName : overrideSlotName)}/{SaveName}{JsonSaveExtension}")) // Check for a Json File
                path = String.Concat($"{pathRoot}/Savegames/{(string.IsNullOrEmpty(overrideSlotName) ? SlotName : overrideSlotName)}/{SaveName}{JsonSaveExtension}");
            
            else if (File.Exists($"{pathRoot}/Savegames/{(string.IsNullOrEmpty(overrideSlotName) ? SlotName : overrideSlotName)}/{SaveName}{BinarySaveExtension}")) // Check for a Binary File
                path = $"{pathRoot}/Savegames/{(string.IsNullOrEmpty(overrideSlotName) ? SlotName : overrideSlotName)}/{SaveName}{BinarySaveExtension}";
        }
        else if (category == LoadCategory.Map)
            path = $"{pathRoot}/Maps/{GlobalData.Seed}/{SaveName}{mapSaveExtension}";
        else if (category == LoadCategory.Settings)
            path = $"{pathRoot}/Settings/{SaveName}{settingsSaveExtension}";

        if (!File.Exists(path))
            throw new noSaveFileFoundExeption();

        return path;
    }
    static string CreatePath(string SaveName, LoadCategory category, SerialisationType serialisationType, string overrideSlotName = "")
    {
        string path = "";
        if (category == LoadCategory.Slot)
            path = $"{pathRoot}/Savegames/{(string.IsNullOrEmpty(overrideSlotName) ? SlotName : overrideSlotName)}/{SaveName}{((serialisationType == SerialisationType.Json) ? JsonSaveExtension : BinarySaveExtension)}";

        else if (category == LoadCategory.Map)
            path = $"{pathRoot}/Maps/{GlobalData.Seed}/{SaveName}{mapSaveExtension}";
        
        else if (category == LoadCategory.Settings)
            path = $"{pathRoot}/Settings/{SaveName}{settingsSaveExtension}";
        
        if (!Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        
        return path;
    }

    // Save Methods (One for Bytes, One for Arrays, One for 2D Arrays and one for serializable classes and primitives)
    public static void SaveBytes(byte[] bytes, string SaveName, LoadCategory category, string overrideSlotName = "")
    {
        File.WriteAllBytes(CreatePath(SaveName, category, SerialisationType.Binary, overrideSlotName), bytes);
    }
    public static void Save<T>(T[] data, string SaveName, LoadCategory category)
    {
        File.WriteAllText(CreatePath(SaveName, category, SerialisationType.Json), JsonUtility.ToJson(new WrapperAr<T>() {Items = data}, false));
    }
    public static void Save<T>(T[,] data, string SaveName, LoadCategory category)
    {
        File.WriteAllText(CreatePath(SaveName, category, SerialisationType.Json), JsonUtility.ToJson(new WrapperAr2D<T>(data), false));
    }
    public static void Save<T>(T data, string SaveName, LoadCategory category, SerialisationType serialisationType, string overrideSlotName = "")
    {
        string path = CreatePath(SaveName, category, serialisationType, overrideSlotName);
        if (serialisationType == SerialisationType.Json) // Json Serialisation
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
            {
                File.WriteAllText(path, JsonUtility.ToJson(new Wrapper<T>() {Item = data}, true));
                Debug.Log("test");
            }
            else
                File.WriteAllText(path, JsonUtility.ToJson(data, true));
        }
        else if (serialisationType == SerialisationType.Binary) // Binary Serialisation
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                File.WriteAllBytes(path, SerializeBinary(new Wrapper<T>() {Item = data}));
            else
                File.WriteAllBytes(path, SerializeBinary(data));
        }
    }

    // Load Methods (One for Bytes, One for Arrays, One for 2D Arrays and one for serializable classes and primitives) The Load Method and the data type must be specified
    public static byte[] LoadBytes(string SaveName, LoadCategory category, string overrideSlotName = "")
    {
        return File.ReadAllBytes(GetPath(SaveName, category, overrideSlotName));
    }
    public static bool TryLoadBytes(out byte[] data, string SaveName, LoadCategory category, string overrideSlotName = "")
    {
        try {
            data = LoadBytes(SaveName, category, overrideSlotName);
            return true;
        }
        catch (noSaveFileFoundExeption)
        {
            data = default;
            return false;
        }
    }
    public static T Load<T>(string SaveName, LoadCategory category, string overrideSlotName = "")
    {
        string path = GetPath(SaveName, category, overrideSlotName);
        if (Path.GetExtension(path) == BinarySaveExtension) // Binary Serialisation
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                return ((Wrapper<T>)DeserializeBinary(File.ReadAllBytes(path))).Item;
            else
                return (T)DeserializeBinary(File.ReadAllBytes(path));
        }
        else // Json Serialisation
        {
            if (typeof(T).IsPrimitive || typeof(T) == typeof(string))
                return JsonUtility.FromJson<Wrapper<T>>(File.ReadAllText(path)).Item;
            else
                return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
    }
    public static bool TryLoad<T>(out T data, string SaveName, LoadCategory category, string overrideSlotName = "")
    {
        try {
            data = Load<T>(SaveName, category, overrideSlotName);
            return true;
        }
        catch (noSaveFileFoundExeption)
        {
            data = default;
            return false;
        }
    }
    public static T[] LoadArray<T>(string SaveName, LoadCategory category)
    {
        string path = GetPath(SaveName, category);
        return JsonUtility.FromJson<WrapperAr<T>>(File.ReadAllText(path)).Items;
    }
    public static T[,] LoadArray2D<T>(string SaveName, LoadCategory category)
    {
        string path = GetPath(SaveName, category);
        return JsonUtility.FromJson<WrapperAr2D<T>>(File.ReadAllText(path)).Get2DArray();
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T Item;
    }  
    [Serializable]
    private class WrapperAr<T>
    {
        public T[] Items;
    }
    [Serializable]
    private class WrapperAr2D<T>
    {
        public int Length;
        public T[] Items;
        public WrapperAr2D(T[,] Items)
        {
            if (Items.GetLength(0) != Items.GetLength(1))
                throw new Exception("The given 2D Array has to have the same width and length to be Serializable");
            Length = Items.GetLength(0);
            this.Items = new T[Length * Length];
            for (int x = 0; x < Length; x++)
            {
                for (int y = 0; y < Length; y++)
                {
                    this.Items[x + y * Length] = Items[x, y];
                }
            }
        }
        public T[,] Get2DArray()
        {
            T[,] Items = new T[Length,Length];
            for (int x = 0; x < Length; x++)
            {
                for (int y = 0; y < Length; y++)
                {
                    Items[x,y] = this.Items[x + y * Length];
                }
            }
            return Items;
        }
    }
}

[Serializable]
public class noSaveFileFoundExeption : Exception { public noSaveFileFoundExeption() {} }


[Serializable]
public class SerializableList<T>
{
    public List<T> list;
    public T this[int index]
    {
        get => list[index];
        set => list[index] = value;
    }
    public SerializableList()
    {
        list = new List<T>();
    }
    public SerializableList(List<T> list)
    {
        this.list = list;
    }
    public void Add(T item) => list.Add(item);
    public void Insert(int index, T item) => list.Insert(index, item);
    public void AddRange(SerializableList<T> values) => list.AddRange(values.list);
    public void RemoveAt(int index) => list.RemoveAt(index);
    public bool Remove(T item) => list.Remove(item);
    public void Clear() => list.Clear();
    public void CopyTo(T[] array, int index) => list.CopyTo(array, index);
    public bool Contains(T item) => list.Contains(item);
    public int IndexOf(T item) => list.IndexOf(item);
    public int Count => list.Count;
}