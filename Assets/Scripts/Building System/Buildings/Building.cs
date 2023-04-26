using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Building : MonoBehaviour
{
    const float checkStabilityDelay = 0.5f;
    public BuildingObject buildingObject;
    public BuildingObject BuildingObject => buildingObject;
    public int Id { get; private set; }
    [ResetOnDestroy]
    protected static Dictionary<int, BuildingData> buildings = new();
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
    public virtual void Initialize()
    {
        Id = 0;
        while (buildings.Keys.Contains(Id))
            Id++;
        buildings.Add(Id, new(buildingObject.Id, transform.position, transform.rotation));
        Invoke("checkStability", checkStabilityDelay);
    }
    private void checkStability()
    {
        buildings[Id].stability = GetStability();
    }
    public virtual void Load(int Id)
    {
        this.Id = Id;
    }
    public virtual int GetStability() => 100;
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
        public BuildingData(string buildingObjectId, Vector3 pos, Quaternion rot)
        {
            this.buildingObjectId = buildingObjectId;
            stability = 100;
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
