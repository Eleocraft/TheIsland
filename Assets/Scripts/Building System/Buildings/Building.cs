using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Building : MonoBehaviour
{
    public BuildingObject buildingObject;
    public BuildingObject BuildingObject => buildingObject;
    public int Id { get; private set; }
    protected int stability = 100;
    private static Dictionary<int, BuildingData> buildings = new();
    [Save(SaveType.world)]
    public static object BuildingSaveData
    {
        get => buildings;
        set
        {
            buildings = (Dictionary<int, BuildingData>)value;
            for (int i = 0; i < buildings.Count; i++)
            {
                KeyValuePair<int, BuildingData> DataAndId = buildings.ElementAt(i);
                Instantiate(BuildingDatabase.BuildingObjects[DataAndId.Value.buildingObjectId].buildingPrefab, DataAndId.Value.Position, DataAndId.Value.Rotation).Load(DataAndId.Key);
            }
        }
    }
    public static void ResetBuildingLists() => buildings = new();
    public virtual void Initialize()
    {
        Id = 0;
        while (buildings.Keys.Contains(Id))
            Id++;
        buildings.Add(Id, new(buildingObject.Id, stability, transform.position, transform.rotation));
    }
    public virtual void Load(int Id)
    {
        this.Id = Id;
        stability = buildings[Id].stability;
    }
    void OnDestroy()
    {
        buildings.Remove(Id);
    }
    [System.Serializable]
    public class BuildingData
    {
        public string buildingObjectId;
        public int stability;
        public Vector3 Position => new(posX, posY, posZ);
        readonly float posX;
        readonly float posY;
        readonly float posZ;
        public Quaternion Rotation => new(rotX, rotY, rotZ, rotW);
        readonly float rotX;
        readonly float rotY;
        readonly float rotZ;
        readonly float rotW;
        public BuildingData(string buildingObjectId, int stability, Vector3 pos, Quaternion rot)
        {
            this.buildingObjectId = buildingObjectId;
            this.stability = stability;
            posX = pos.x;
            posY = pos.y;
            posZ = pos.z;
            rotX = rot.x;
            rotY = rot.y;
            rotZ = rot.z;
            rotW = rot.w;
        }
    }
}
