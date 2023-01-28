using UnityEngine;
using UnityEditor;

public class CreateTree : EditorWindow
{
    const string Prefabpath = "Assets/Prefabs/MapObjects/HarvestableObjects/Trees";
    const string SOpath = "Assets/CustomObjects/Terrain/MapObjects/ObjectData";
    const string Materialpath = "Assets/Art/Materials/MapObjects/Trees";

    private string TreeName;
    private Mesh TreeMesh;
    private Mesh TrunkMesh;
    private Mesh StumpMesh;
    private Material[] materials;
    private Material[] TrunkMaterials;
    private float TreeLife;
    private ToolDamageData toolDamageInfo;

    private string TrunkName;
    private float TrunkLife;
    private HarvestingData harvestingInfo;

    private MaterialAssigner treeMaterialAssigner;
    private MaterialAssigner trunkMaterialAssigner;

    public static void OpenWindow()
    {
        GetWindow<CreateTree>("CreateTree");
    }
    public void SetTreeMaterials(Material[] materials) => this.materials = materials;
    public void SetTrunkMaterials(Material[] TrunkMaterials) => this.TrunkMaterials = TrunkMaterials;

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Tree Information", EditorStyles.boldLabel);
        TreeName = EditorGUILayout.TextField("Tree name:", TreeName);
        TreeLife = EditorGUILayout.FloatField("Tree Life:", TreeLife);
        // Tree
        EditorGUI.BeginChangeCheck();
        TreeMesh = (Mesh)EditorGUILayout.ObjectField("Tree mesh:", TreeMesh, typeof(Mesh), false);
        if (EditorGUI.EndChangeCheck() && treeMaterialAssigner != null)
            treeMaterialAssigner.Destroy();

        if (treeMaterialAssigner == null && TreeMesh != null && GUILayout.Button("Assign TreeMaterials"))
            treeMaterialAssigner = MaterialAssigner.OpenWindow(TreeMesh, Materialpath, SetTreeMaterials);

        EditorGUILayout.LabelField("Trunk and Stump Information", EditorStyles.boldLabel);
        TrunkName = EditorGUILayout.TextField("TrunkName:", TrunkName);
        TrunkLife = EditorGUILayout.FloatField("Trunk Life:", TrunkLife);
        // Trunk / Stump
        EditorGUI.BeginChangeCheck();
        TrunkMesh = (Mesh)EditorGUILayout.ObjectField("Trunk mesh:", TrunkMesh, typeof(Mesh), false);
        StumpMesh = (Mesh)EditorGUILayout.ObjectField("Stump mesh:", StumpMesh, typeof(Mesh), false);
        if (EditorGUI.EndChangeCheck() && trunkMaterialAssigner != null)
            trunkMaterialAssigner.Destroy();

        if (trunkMaterialAssigner == null && TrunkMesh != null && GUILayout.Button("Assign TrunkMaterials"))
            trunkMaterialAssigner = MaterialAssigner.OpenWindow(TrunkMesh, Materialpath, SetTrunkMaterials);

        EditorGUILayout.LabelField("Harvesting Information", EditorStyles.boldLabel);

        harvestingInfo = (HarvestingData)EditorGUILayout.ObjectField("Trunk Harvestin Info:", harvestingInfo, typeof(HarvestingData), false);
        toolDamageInfo = (ToolDamageData)EditorGUILayout.ObjectField("Damage To Tree:", toolDamageInfo, typeof(ToolDamageData), false);

        if (GUILayout.Button("Create"))
        {
            // Tree Prefab
            GameObject Tree = new(TreeName);
            Tree.layer = LayerMask.GetMask("Harvestables");
            MeshFilter meshFilter = Tree.AddComponent<MeshFilter>();
            MeshRenderer renderer = Tree.AddComponent<MeshRenderer>();
            CapsuleCollider collider = Tree.AddComponent<CapsuleCollider>();
            Tree.AddComponent<TreeResource>();
            meshFilter.mesh = TreeMesh;
            renderer.materials = materials;
            collider.radius = TrunkMesh.bounds.size.x;
            collider.height = TrunkMesh.bounds.size.y;
            collider.center = TrunkMesh.bounds.center;

            // Trunk Prefab
            GameObject Trunk = new($"{TreeName}Trunk");
            MeshFilter TrunkMeshFilter = Trunk.AddComponent<MeshFilter>();
            MeshRenderer TrunkRenderer = Trunk.AddComponent<MeshRenderer>();
            MeshCollider TrunkCollider = Trunk.AddComponent<MeshCollider>();
            Rigidbody TrunkRB = Trunk.AddComponent<Rigidbody>();
            Trunk.AddComponent<TrunkResource>();
            TrunkMeshFilter.mesh = TrunkMesh;
            TrunkRenderer.materials = TrunkMaterials;
            TrunkCollider.sharedMesh = TrunkMesh;
            TrunkCollider.convex = true;
            TrunkRB.angularDrag = 10;
            TrunkRB.mass = 50;

            // Stump Prefab
            GameObject Stump = new($"{TreeName}Stump");
            MeshFilter StumpMeshFilter = Stump.AddComponent<MeshFilter>();
            MeshRenderer StumpRenderer = Stump.AddComponent<MeshRenderer>();
            MeshCollider StumpCollider = Stump.AddComponent<MeshCollider>();
            StumpMeshFilter.mesh = StumpMesh;
            StumpRenderer.materials = TrunkMaterials;
            StumpCollider.sharedMesh = StumpMesh;
            StumpCollider.convex = true;

            //Saving
            string TreePath = $"{Prefabpath}/{TreeName}.prefab";
            string TrunkPath = $"{Prefabpath}/Trunks/{TreeName}Trunk.prefab";
            string StumpPath = $"{Prefabpath}/Stumps/{TreeName}Stump.prefab";

            GameObject TreePrefab = PrefabUtility.SaveAsPrefabAsset(Tree, TreePath);
            GameObject TrunkPrefab = PrefabUtility.SaveAsPrefabAsset(Trunk, TrunkPath);
            GameObject StumpPrefab = PrefabUtility.SaveAsPrefabAsset(Stump, StumpPath);
            
            DestroyImmediate(Tree);
            DestroyImmediate(Trunk);
            DestroyImmediate(Stump);

            // SO
            TreeObjectData objectData = CreateInstance<TreeObjectData>();
            objectData.Object = TreePrefab;
            objectData.trunkPrefab = TrunkPrefab;
            objectData.stumpPrefab = StumpPrefab;
            objectData.treeName = TreeName;
            objectData.Life = TreeLife;
            objectData.toolDamageInfo = toolDamageInfo;
            objectData.trunkData = new();
            objectData.trunkData.harvestingInfo = harvestingInfo;
            objectData.trunkData.recourceName = TrunkName;
            objectData.trunkData.Life = TrunkLife;

            TreePrefab.GetComponent<TreeResource>().objectData = objectData;

            // Saving SO
            string ObjectDataPath = $"{SOpath}/{TreeName}.asset";

            AssetDatabase.CreateAsset(objectData, ObjectDataPath);

            Debug.Log($"Generation of {TreeName} Done");
        }
    }
}