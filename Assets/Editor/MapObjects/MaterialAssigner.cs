using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public class MaterialAssigner : EditorWindow
{
    private Editor gameObjectEditor;
    private GameObject previewObject;
    private Material referenceMat;
    private Material[] materials;
    private bool[] createNew;
    private Color[] subMeshColors;
    private Mesh mesh;
    private Action<Material[]> callback;
    private string path;
    private string SaveName;
    public static MaterialAssigner OpenWindow(Mesh mesh, string path, Action<Material[]> callback)
    {
        MaterialAssigner assigner = GetWindow<MaterialAssigner>("Assign Materials");
        assigner.mesh = mesh;
        assigner.callback = callback;
        assigner.subMeshColors = new Color[mesh.subMeshCount];
        assigner.createNew = new bool[mesh.subMeshCount];
        assigner.path = path;
        assigner.materials = new Material[mesh.subMeshCount];
        return assigner;
    }
    private void OnGUI()
    {
        // Create Color Fields
        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Submesh {i}:");
            createNew[i] = EditorGUILayout.Toggle("Create new:", createNew[i]);
            if (createNew[i])
                subMeshColors[i] = EditorGUILayout.ColorField(subMeshColors[i]);
            else
                materials[i] = (Material)EditorGUILayout.ObjectField(materials[i], typeof(Material), false);
            EditorGUILayout.EndHorizontal();
        }
        if (createNew.Contains(true))
            referenceMat = (Material)EditorGUILayout.ObjectField("reference material", referenceMat, typeof(Material), false);
        if (EditorGUI.EndChangeCheck())
            Refresh();
        if (createNew.Contains(true))
            SaveName = EditorGUILayout.TextField("SaveName:", SaveName);
        if (previewObject != null)
        {
            if (gameObjectEditor == null)
                gameObjectEditor = Editor.CreateEditor(previewObject);
            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(200,200), new GUIStyle());
        }
        if (GUILayout.Button("Apply"))
        {
            Apply();
            Destroy();
        }
        
        void Refresh()
        {
            if(gameObjectEditor != null) DestroyImmediate(gameObjectEditor);
            if(previewObject != null) DestroyImmediate(previewObject);
            // Create Materials
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                if (createNew[i] && referenceMat)
                {
                    materials[i] = new Material(referenceMat);
                    materials[i].color = subMeshColors[i];
                }
            }
            // Create PrewiewObject
            previewObject = new GameObject("preview");
            previewObject.AddComponent<MeshFilter>().mesh = mesh;
            previewObject.AddComponent<MeshRenderer>().materials = materials;
        }

        void Apply()
        {
            // SaveMaterials
            System.IO.DirectoryInfo dirInfo = new(path);
            dirInfo.Create();

            for (int i = 0; i < materials.Length; i++)
            {
                if (!AssetDatabase.Contains(materials[i]))
                    AssetDatabase.CreateAsset(materials[i], $"{path}/{SaveName}Material{i}.mat");
            }
            
            callback(materials);
        }
    }
    public void Destroy()
    {
        Close();
    }
    private void OnDestroy()
    {
        if(gameObjectEditor != null) DestroyImmediate(gameObjectEditor);
        if(previewObject != null) DestroyImmediate(previewObject);
    }
}
