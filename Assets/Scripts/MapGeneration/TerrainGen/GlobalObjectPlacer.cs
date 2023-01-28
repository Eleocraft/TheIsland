using UnityEngine;

public static class GlobalObjectPlacer
{
    public static SerializableList<GlobalObjectPoint>[,] GenerateGlobalObjectPoints(TerrainSettings settings, ushort[,] biomeMap, float[,] heightMap)
    {
        // This is a very temporary method to determine the placement of global objects
        int chunkCount = settings.mapSize / settings.chunkSize;
        SerializableList<GlobalObjectPoint>[,] objectPoints = new SerializableList<GlobalObjectPoint>[chunkCount,chunkCount];
        Vector3[,] normalMap = NormalMapGenerator.CalculateNormals(heightMap);
        for (int chunkX = 0; chunkX < chunkCount; chunkX++)
        {
            for (int chunkY = 0; chunkY < chunkCount; chunkY++)
            {
                if (objectPoints[chunkX,chunkY] == null)
                    objectPoints[chunkX,chunkY] = new SerializableList<GlobalObjectPoint>();
                if (Random.value < 0.6f)
                    continue;
                Vector2Int globalChunkPos = new(chunkX * settings.chunkSize, chunkY * settings.chunkSize);
                Vector2 localPosition = new(Random.Range(settings.Simplification, settings.chunkSize-settings.Simplification), Random.Range(settings.Simplification, settings.chunkSize-settings.Simplification));
                Vector2 position = localPosition + globalChunkPos;
                Vector2Int flooredPos = new(Mathf.FloorToInt(position.x / settings.Simplification), Mathf.FloorToInt(position.y / settings.Simplification));
                Vector2Int ceildedPos = new(Mathf.CeilToInt(position.x / settings.Simplification), Mathf.CeilToInt(position.y / settings.Simplification));
                // Finding Biome
                BiomeData biome = settings.Biomes[biomeMap[flooredPos.x, flooredPos.y]];
                if (biome.GlobalObjects.Count == 0)
                    continue;
                // Calculating aproximate height from simplified heightMap through lerping
                Vector2 weightVec = position / settings.Simplification - flooredPos;
                float heightBL = heightMap[flooredPos.x, flooredPos.y];
                float heightBR = heightMap[ceildedPos.x, flooredPos.y];
                float heightTL = heightMap[flooredPos.x, ceildedPos.y];
                float heightTR = heightMap[ceildedPos.x, ceildedPos.y];
                float height = Utility.LerpBetween4Values(weightVec, heightBL, heightBR, heightTL, heightTR);
                
                int objectID = Random.Range(0, biome.GlobalObjects.Count - 1);
                SpawnableGlobalObject objectData = biome.GlobalObjects[objectID];
                if (objectData.minHeight > height || Utility.CalcualteSteepnessRadientFromNormal(normalMap[flooredPos.x, flooredPos.y]) > objectData.GeneraionSteepnessRadiant)
                    continue;
                objectPoints[chunkX,chunkY].Add(new GlobalObjectPoint(localPosition, objectData, height, objectID, biomeMap[flooredPos.x, flooredPos.y]));
            }
        }
        return objectPoints;
    }
}
[System.Serializable]
public struct GlobalObjectPoint
{
    public Vector2 localPosition;
    public float sqrtBlockRadius;
    public float sqrtFlatRadius;
    public int objectID;
    public int biomeID;
    public float height;
    public GlobalObjectPoint(Vector2 localPosition, SpawnableGlobalObject objectData, float height, int objectID, int biomeID)
    {
        this.localPosition = localPosition;
        sqrtBlockRadius = objectData.ObjectBlockRadius * objectData.ObjectBlockRadius;
        sqrtFlatRadius = objectData.flatRadius * objectData.flatRadius;
        this.height = height;
        this.objectID = objectID;
        this.biomeID = biomeID;
    }
}