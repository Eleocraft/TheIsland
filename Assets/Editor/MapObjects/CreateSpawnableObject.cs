using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CreateSpawnableObject : EditorWindow
{
    const string SOpath = "Assets/CustomObjects/Terrain/MapObjects/SpawnableObjects";

    private ObjectData objectData;
    private ObjectSpawnData spawnInfo;

    private List<BiomeData> biomes = new();

    public static void OpenWindow()
    {
        GetWindow<CreateSpawnableObject>("CreateSpawnableObjects");
    }
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Object and spawn data", EditorStyles.boldLabel);
        objectData = (ObjectData)EditorGUILayout.ObjectField("Object data:", objectData, typeof(ObjectData), false);
        spawnInfo = (ObjectSpawnData)EditorGUILayout.ObjectField("Object spawn data:", spawnInfo, typeof(ObjectSpawnData), false);

        EditorGUILayout.LabelField("Biomes", EditorStyles.boldLabel);        
        int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("number of biomes", biomes.Count));
        while (newCount < biomes.Count)
            biomes.RemoveAt( biomes.Count - 1 );
        while (newCount > biomes.Count)
            biomes.Add(null);
        for (int i = 0; i < biomes.Count; i++)
            biomes[i] = (BiomeData)EditorGUILayout.ObjectField(biomes[i], typeof(BiomeData), false);

        if (GUILayout.Button("Create"))
        {
            SpawnableObject so = CreateInstance<SpawnableObject>();
            so.objectData = objectData;
            so.objectSpawnData = spawnInfo;

            string spawnDataPath = $"{SOpath}/{objectData.name}.asset";
            AssetDatabase.CreateAsset(so, spawnDataPath);

            foreach (BiomeData biome in biomes)
                biome.Objects.Add(so);

            Debug.Log($"Generation of Spawn Info for {objectData.name} Done");
        }
    }
}
