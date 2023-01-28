using UnityEngine;
using UnityEditor;

public class CreatePickupResource : EditorWindow
{
    const string Prefabpath = "Assets/Prefabs/MapObjects/PickupObjects";
    const string SOpath = "Assets/CustomObjects/Terrain/MapObjects/ObjectData";
    const string Materialpath = "Assets/Art/Materials/MapObjects/Recources";

    private Mesh mesh;
    private Material[] materials;
    private string ObjectName;
    private ItemObject Item;
    private int DropAmount;

    private MaterialAssigner materialAssigner;

    public static void OpenWindow()
    {
        GetWindow<CreatePickupResource>("CreatePickupResourceObject");
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
        Item = (ItemObject)EditorGUILayout.ObjectField("DroppedItem:", Item, typeof(ItemObject), false);
        DropAmount = EditorGUILayout.IntField("DropAmount:", DropAmount);


        if (GUILayout.Button("Create"))
        {
            // Prefab
            GameObject Obj = new GameObject(ObjectName);
            MeshFilter meshFilter = Obj.AddComponent<MeshFilter>();
            MeshRenderer renderer = Obj.AddComponent<MeshRenderer>();
            MeshCollider collider = Obj.AddComponent<MeshCollider>();
            Obj.AddComponent<PickupResource>();
            meshFilter.mesh = mesh;
            renderer.materials = materials;
            collider.sharedMesh = mesh;

            GameObject ObjPrefab = PrefabUtility.SaveAsPrefabAsset(Obj, $"{Prefabpath}/{ObjectName}.prefab");
            DestroyImmediate(Obj);

            // SO
            PickupResourceData objectData = CreateInstance<PickupResourceData>();
            objectData.Object = ObjPrefab;
            objectData.Amount = DropAmount;
            objectData.recourceName = ObjectName;
            objectData.Item = Item;

            ObjPrefab.GetComponent<PickupResource>().objectData = objectData;

            string ObjectDataPath = $"{SOpath}/{ObjectName}.asset";
            AssetDatabase.CreateAsset(objectData, ObjectDataPath);

            Debug.Log($"Generation of {ObjectName} Done");
        }
    }
}
