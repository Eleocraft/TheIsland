using System.Collections.Generic;
using UnityEngine;

public class MapObject : MonoBehaviour
{
    const int ChunklessObjIdX = int.MaxValue;

    public MapObjectID Id { get; private set; }
    public virtual void UpdateLocalState(float Life) { }
    public virtual float GetBaseLife => 0;

    private static Dictionary<ChunkID, SerializableList<MapObjectData>> MapObjects = new();
    private static Dictionary<ChunkID, List<MapObject>> CreatedMapObjects = new();
    private static Dictionary<MapObjectID, float> modifiedObjects = new();

    [Save(SaveType.world)]
    public static object MapObjectSaveData
    {
        get => MapObjects;
        set => MapObjects = (Dictionary<ChunkID, SerializableList<MapObjectData>>)value;
    }

    public static void GenerateObjects(GeneratedChunk generatedChunk, ChunkData chunkData, Transform parent, Vector2Int chunkCoord, int chunkSize, BiomeData[] Biomes)
    {
        if (ChunkPopulated(chunkCoord))
            return;
        ChunkID chunkId = new(chunkCoord);
        MapObjects.Add(chunkId, new());
        CreatedMapObjects.Add(chunkId, new());
        for (int i = 0; i < generatedChunk.objectSpawnPoints.positions.Count; i++)
        {
            Vector2 pos = generatedChunk.objectSpawnPoints.positions[i];
            BiomeData Biome = Biomes[chunkData.biomeMap[(int)pos.x, chunkSize - (int)pos.y]];
            ObjectData objData = Biome.Objects[generatedChunk.objectSpawnPoints.objectIDs[i]].objectData;
            GameObject obj = Instantiate(objData.Object, parent);
            Vector2 localObjPos = new(pos.x - chunkSize*0.5f, pos.y - chunkSize*0.5f);
            ChunkID localMapPos = new(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
            obj.transform.localPosition = new Vector3(localObjPos.x, generatedChunk.heightMap[localMapPos.x, chunkSize - localMapPos.y], localObjPos.y);
            obj.transform.rotation = generatedChunk.objectSpawnPoints.rotations[i];

            MapObject objControl = obj.GetComponent<MapObject>();
            MapObjectID mapObjectId = new(chunkId, i);
            objControl.Id = mapObjectId;
            float life = objControl.GetBaseLife;
            if (modifiedObjects.ContainsKey(mapObjectId))
            {
                life = modifiedObjects[mapObjectId];
                modifiedObjects.Remove(mapObjectId);
                objControl.UpdateLocalState(life);
            }
            CreatedMapObjects[chunkId].Add(objControl);
            MapObjects[chunkId].Add(new(life, chunkData.biomeMap[(int)pos.x, chunkSize - (int)pos.y], generatedChunk.objectSpawnPoints.objectIDs[i], obj.transform.position, generatedChunk.objectSpawnPoints.rotations[i]));
        }
    }
    public static void LoadObjects(Vector2Int chunkCoord, BiomeData[] Biomes, Transform parent)
    {
        if (!ChunkPopulated(chunkCoord))
            return;
        ChunkID chunkId = new(chunkCoord);
        CreatedMapObjects.Add(chunkId, new());

        for (int i = 0; i < MapObjects[chunkId].Count; i++)
        {
            MapObjectData data = MapObjects[chunkId][i];
            BiomeData Biome = Biomes[data.biome];
            ObjectData objData = Biome.Objects[data.prefabId].objectData;
            GameObject obj = Instantiate(objData.Object, parent);

            obj.transform.position = data.Position;
            obj.transform.rotation = data.Rotation;

            MapObject objControl = obj.GetComponent<MapObject>();
            objControl.Id = new(chunkId, i);
            objControl.UpdateLocalState(data.life);
            CreatedMapObjects[chunkId].Add(objControl);
        }
    }
    public static void CreateChunklessObject(MapObject newObject)
    {
        ChunkID chunklessObjId = new(ChunklessObjIdX, 0);
        if (!CreatedMapObjects.ContainsKey(chunklessObjId))
            CreatedMapObjects.Add(chunklessObjId, new());
        
        CreatedMapObjects[chunklessObjId].Add(newObject);
        newObject.Id = new(chunklessObjId, CreatedMapObjects[chunklessObjId].Count - 1);
    }
    public static bool ChunkPopulated(Vector2Int chunkCoord) => MapObjects.ContainsKey(new(chunkCoord));
    public static void ResetMapObjectLists()
    {
        CreatedMapObjects = new();
        MapObjects = new();
        modifiedObjects = new();
    }
    protected static void UpdateState(MapObjectID Id, float Life)
    {
        if (!Id.chunkId.Chunkless())
            MapObjects[Id.chunkId][Id.objectId].UpdateLife(Life);

        // Message stateUpdate = Message.Create(MessageSendMode.reliable, MessageId.recourceStateUpdate);
        // stateUpdate.AddInt(Id.chunkId.x);
        // stateUpdate.AddInt(Id.chunkId.y);
        // stateUpdate.AddInt(Id.objectId);
        // stateUpdate.AddFloat(Life);
        // NetworkManager.Send(stateUpdate);
    }

    // [MessageHandler((ushort)MessageId.recourceStateUpdate, clientServerIndependent : true)]
    // public static void ReceiveStateUpdate(Message message)
    // {
    //     int ChunkIdX = message.GetInt();
    //     int ChunkIdY = message.GetInt();
    //     int ObjectId = message.GetInt();
    //     float Life = message.GetFloat();
    //     ChunkID chunkId = new(ChunkIdX, ChunkIdY);
    //     if (MapObjects.ContainsKey(chunkId))
    //         MapObjects[chunkId][ObjectId].UpdateLife(Life);

    //     else if (!chunkId.Chunkless())
    //         modifiedObjects.Add(new(chunkId, ObjectId), Life);        
        
    //     if (CreatedMapObjects.ContainsKey(chunkId))
    //         CreatedMapObjects[chunkId][ObjectId].UpdateLocalState(Life);
    // }
    [System.Serializable]
    public class MapObjectData
    {
        public float life;
        public readonly int biome;
        public readonly int prefabId;
        public Vector3 Position => new(posX, posY, posZ);
        readonly float posX;
        readonly float posY;
        readonly float posZ;
        public Quaternion Rotation => new(rotX, rotY, rotZ, rotW);
        readonly float rotX;
        readonly float rotY;
        readonly float rotZ;
        readonly float rotW;
        public MapObjectData(float life, int biome, int prefabId, Vector3 pos, Quaternion rot)
        {
            this.life = life;
            this.biome = biome;
            this.prefabId = prefabId;
            posX = pos.x;
            posY = pos.y;
            posZ = pos.z;
            rotX = rot.x;
            rotY = rot.y;
            rotZ = rot.z;
            rotW = rot.w;
        }
        public void UpdateLife(float newLife)
        {
            life = newLife;
        }
    }
    [System.Serializable]
    public struct ChunkID
    {
        public readonly int x;
        public readonly int y;
        public ChunkID(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public ChunkID(Vector2Int coords)
        {
            x = coords.x;
            y = coords.y;
        }
        public bool Chunkless()
        {
            return x == ChunklessObjIdX;
        }
    }
    [System.Serializable]
    public struct MapObjectID
    {
        public readonly ChunkID chunkId;
        public readonly int objectId;
        public MapObjectID(ChunkID chunkId, int objectId)
        {
            this.chunkId = chunkId;
            this.objectId = objectId;
        }
        public MapObjectID(int objectId) // Constructor for chunkless objects
        {
            chunkId = new(ChunklessObjIdX, 0);
            this.objectId = objectId;
        }
    }
}