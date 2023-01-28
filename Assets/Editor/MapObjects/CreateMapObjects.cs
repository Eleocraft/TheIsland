using UnityEditor;
using UnityEngine;

public class CreateMapObjects : EditorWindow
{
    [MenuItem("TheIsland/Create/MapObject")]
    public static void OpenWindow()
    {
        GetWindow<CreateMapObjects>("CreateMapObjects");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Create Map Object", EditorStyles.boldLabel);
        if (GUILayout.Button("New Default Obj"))
            CreateDefaultMapObj.OpenWindow();
        if (GUILayout.Button("New PickupObject"))
            CreatePickupResource.OpenWindow();
        if (GUILayout.Button("New Simple Recource"))
            CreateSimpleResource.OpenWindow();
        if (GUILayout.Button("New Tree"))
            CreateTree.OpenWindow();
        EditorGUILayout.LabelField("Create Spawn info", EditorStyles.boldLabel);
        if (GUILayout.Button("New Object Spawn Data"))
            CreateSpawnInfo.OpenWindow();
        if (GUILayout.Button("New Spawnable Object"))
            CreateSpawnableObject.OpenWindow();
    }
}
