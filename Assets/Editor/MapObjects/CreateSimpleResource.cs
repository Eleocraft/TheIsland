using UnityEngine;
using UnityEditor;

public class CreateSimpleResource : EditorWindow
{
    const string Prefabpath = "Assets/Prefabs/MapObjects/HarvestableObjects";
    const string SOpath = "Assets/CustomObjects/Terrain/MapObjects/ObjectData";
    const string Materialpath = "Assets/Art/Materials/MapObjects/Recources";

    private Mesh mesh;
    private Material[] materials;
    private string ObjectName;
    private HarvestingData harvestingInfo;
    private float Life;

    private MaterialAssigner materialAssigner;

    public static void OpenWindow()
    {
        GetWindow<CreateSimpleResource>("CreateSimpleResourceObject");
    }
    public void SetMaterials(Material[] materials) => this.materials = materials;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Object Information", EditorStyles.boldLabel);
        ObjectName = EditorGUILayout.TextField("Object name:", ObjectName);

        EditorGUI.BeginChangeCheck();
        mesh = (Mesh)EditorGUILayout.ObjectField("mesh:", mesh, typeof(Mesh), false);
        if (EditorGUI.EndChangeCheck() && materialAssigner != null)
            materialAssigner.Destroy();

        if (materialAssigner == null && mesh != null && GUILayout.Button("Assign Materials"))
            materialAssigner = MaterialAssigner.OpenWindow(mesh, Materialpath, SetMaterials);

        EditorGUILayout.LabelField("Harvesting Information", EditorStyles.boldLabel);
        Life = EditorGUILayout.FloatField("ObjectLife:", Life);
        harvestingInfo = (HarvestingData)EditorGUILayout.ObjectField("Harvesting info:", harvestingInfo, typeof(HarvestingData), false);


        if (GUILayout.Button("Create"))
        {
            // Prefab
            GameObject Obj = new GameObject(ObjectName);
            Obj.layer = LayerMask.GetMask("Harvestables");
            MeshFilter meshFilter = Obj.AddComponent<MeshFilter>();
            MeshRenderer renderer = Obj.AddComponent<MeshRenderer>();
            MeshCollider collider = Obj.AddComponent<MeshCollider>();
            Obj.AddComponent<SimpleResource>();
            meshFilter.mesh = mesh;
            renderer.materials = materials;
            collider.sharedMesh = mesh;

            GameObject ObjPrefab = PrefabUtility.SaveAsPrefabAsset(Obj, $"{Prefabpath}/{ObjectName}.prefab");
            DestroyImmediate(Obj);

            // SO
            SimpleResourceData objectData = CreateInstance<SimpleResourceData>();
            objectData.Object = ObjPrefab;
            objectData.recourceName = ObjectName;
            objectData.Life = Life;
            objectData.harvestingInfo = harvestingInfo;

            ObjPrefab.GetComponent<SimpleResource>().objectData = objectData;

            string ObjectDataPath = $"{SOpath}/{ObjectName}.asset";
            AssetDatabase.CreateAsset(objectData, ObjectDataPath);

            Debug.Log($"Generation of {ObjectName} Done");
        }
    }
}
