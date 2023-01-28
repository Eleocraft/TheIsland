using UnityEngine;
using UnityEditor;

public class CreateDefaultMapObj : EditorWindow
{
    const string Prefabpath = "Assets/Prefabs/MapObjects";
    const string SOpath = "Assets/CustomObjects/Terrain/MapObjects/ObjectData";
    const string Materialpath = "Assets/Art/Materials/MapObjects";

    private Mesh mesh;
    private Material[] materials;
    private string ObjectName;
    private bool AddCollider = true;

    private MaterialAssigner materialAssigner;

    public static void OpenWindow()
    {
        GetWindow<CreateDefaultMapObj>("CreateMapObject");
    }
    public void SetMaterials(Material[] materials) => this.materials = materials;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Object Information", EditorStyles.boldLabel);
        ObjectName = EditorGUILayout.TextField("Object name:", ObjectName);
        AddCollider = EditorGUILayout.Toggle("Add Collider:", AddCollider);

        EditorGUI.BeginChangeCheck();
        mesh = (Mesh)EditorGUILayout.ObjectField("mesh:", mesh, typeof(Mesh), false);
        if (EditorGUI.EndChangeCheck() && materialAssigner != null)
            materialAssigner.Destroy();

        if (materialAssigner == null && mesh != null && GUILayout.Button("Assign Materials"))
            materialAssigner = MaterialAssigner.OpenWindow(mesh, Materialpath, SetMaterials);


        if (GUILayout.Button("Create"))
        {
            // Prefab
            GameObject Obj = new GameObject(ObjectName);
            MeshFilter meshFilter = Obj.AddComponent<MeshFilter>();
            MeshRenderer renderer = Obj.AddComponent<MeshRenderer>();
            MeshCollider collider = Obj.AddComponent<MeshCollider>();
            Obj.AddComponent<MapObject>();
            meshFilter.mesh = mesh;
            renderer.materials = materials;
            collider.sharedMesh = mesh;

            GameObject ObjPrefab = PrefabUtility.SaveAsPrefabAsset(Obj, $"{Prefabpath}/{ObjectName}.prefab");
            DestroyImmediate(Obj);

            // SO
            ObjectData objectData = CreateInstance<ObjectData>();
            objectData.Object = ObjPrefab;

            string ObjectDataPath = $"{SOpath}/{ObjectName}.asset";
            AssetDatabase.CreateAsset(objectData, ObjectDataPath);

            Debug.Log($"Generation of {ObjectName} Done");
        }
    }
}
