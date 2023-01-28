using UnityEngine;
using UnityEditor;

public class CreateSpawnInfo : EditorWindow
{
    const string SOpath = "Assets/CustomObjects/Terrain/MapObjects/SpawnData";

    private float GenerationSteepnessDegrees = 60f;
    private float radius = 10f;
    private float frequency = 10f;
    private float minHeight = 40f;
    private bool hasMinHeight = false;
    private float maxHeight = float.MaxValue;
    private bool hasMaxHeight = false;
    private bool rotateWithTerrain;

    private string DataName;

    public static void OpenWindow()
    {
        GetWindow<CreateSpawnInfo>("CreateObjectSpawnData");
    }
    private void OnGUI()
    {
        DataName = EditorGUILayout.TextField("Data name:", DataName);

        EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
        GenerationSteepnessDegrees = EditorGUILayout.FloatField("max steepness in degrees", GenerationSteepnessDegrees);

        radius = EditorGUILayout.FloatField("radius", radius);
        frequency = EditorGUILayout.FloatField("frequency", frequency);
        hasMinHeight = EditorGUILayout.Toggle("use Min Height", hasMinHeight);
        if (hasMinHeight)
            minHeight = EditorGUILayout.FloatField("Min Height", minHeight);
        hasMaxHeight = EditorGUILayout.Toggle("use Max Height", hasMaxHeight);
        if (hasMaxHeight)
            maxHeight = EditorGUILayout.FloatField("Max Height", maxHeight);
        rotateWithTerrain = EditorGUILayout.Toggle("Rotate With Terrain", rotateWithTerrain);

        if (GUILayout.Button("Create"))
        {
            ObjectSpawnData so = CreateInstance<ObjectSpawnData>();
            so.GeneraionSteepnessRadiant = GenerationSteepnessDegrees * Mathf.Deg2Rad;
            so.radius = radius;
            so.frequency = frequency;
            so.minHeight = hasMinHeight ? minHeight : float.MinValue;
            so.maxHeight = hasMaxHeight ? maxHeight : float.MaxValue;
            so.rotateWithTerrain = rotateWithTerrain;

            string spawnDataPath = $"{SOpath}/{DataName}.asset";
            AssetDatabase.CreateAsset(so, spawnDataPath);

            Debug.Log($"Generation of Spawn Info for {DataName} Done");
        }
    }
}
