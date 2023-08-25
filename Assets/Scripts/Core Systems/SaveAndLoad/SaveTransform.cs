using UnityEngine;
using System.Collections.Generic;

public class SaveTransform : MonoBehaviour
{
    static List<IDTransformPair> transformsToSave = new();
    
    
    [SerializeField] private int prefabID;

    [Save(SaveType.world)]
    public static object Save
    {
        get {
            return new SavedTransforms(transformsToSave);
        }
        set {
            ((SavedTransforms)value).Load();
        }
    }

    void Start()
    {
        transformsToSave.Add(new(prefabID, transform));
    }
    void OnDestroy()
    {
        transformsToSave.Remove(new(prefabID, transform));
    }
}
[System.Serializable]
public class SavedTransforms
{
    public int[] prefabIDs;
    public SerializableVector3[] positions;
    public SerializableQuaternion[] rotations;
    public SavedTransforms(List<IDTransformPair> transformsToSave)
    {
        prefabIDs = new int[transformsToSave.Count];
        positions = new SerializableVector3[transformsToSave.Count];
        rotations = new SerializableQuaternion[transformsToSave.Count];
        for (int i = 0; i < transformsToSave.Count; i++)
        {
            prefabIDs[i] = transformsToSave[i].prefabID;

            Vector3 pos = transformsToSave[i].transform.position;
            Quaternion rot = transformsToSave[i].transform.rotation;

            positions[i] = new SerializableVector3(pos);
            rotations[i] = new SerializableQuaternion(rot);
        }
    }
    public void Load()
    {
        for (int i = 0; i < prefabIDs.Length; i++)
        {
            Vector3 pos = positions[i].getVec();
            Quaternion rot = rotations[i].getQuat();

            GameObject.Instantiate(SaveTransformPrefabs.SavablePrefabs[prefabIDs[i]], pos, rot);
        }
    }
}
public struct IDTransformPair
{
    public int prefabID;
    public Transform transform;
    public IDTransformPair(int prefabID, Transform transform)
    {
        this.prefabID = prefabID;
        this.transform = transform;
    }
}