using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public const string globalHeightMapSaveName = "0-globalHeightMap";
    public const string globalBiomeMapSaveName = "0-globalBiomeMap";
    public const string voronoiLocationsSaveName = "0-voronoiPointLocations";
    public const string voronoiBiomesSaveName = "0-voronoiPointBiomes";
    public const string biomeDistortionSaveName = "0-biomeDistortion";
    public const string octaveOffsetsSaveName = "0-octaveOffsets";
    public const string globalObjectsSaveName = "0-globalObjects";

    public static TerrainSettings settings;
    public static void Initialize(TerrainSettings settings)
    {
        MapGenerator.settings = settings;
    }
    public static GlobalMap GenerateGlobalMap()
    {
        // This method will create a lowDensity map

        // Calculating the octaveOffsets (Basicly the randomness part) of the map generation
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = Random.Range(-100000, 100000);
            float offsetY = Random.Range(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Actual Generation
        GlobalBiomeMap globalBiomeMap = BiomeMapGenerator.GenerateGlobalBiomeMap(settings);
        float[,] lowDensityHeightMap = HeightMapGenerator.GenerateGlobalNoiseMap(settings, globalBiomeMap, octaveOffsets);
        SerializableList<GlobalObjectPoint>[,] globalObjects = GlobalObjectPlacer.GenerateGlobalObjectPoints(settings, globalBiomeMap.biomeMap, lowDensityHeightMap);

        // Saving the map
        SaveAndLoad.Save(lowDensityHeightMap, globalHeightMapSaveName, LoadCategory.Map);
        SaveAndLoad.Save(globalBiomeMap.biomeMap, globalBiomeMapSaveName, LoadCategory.Map);
        SaveAndLoad.Save(globalBiomeMap.voronoiPointLocations, voronoiLocationsSaveName, LoadCategory.Map);
        SaveAndLoad.Save(globalBiomeMap.voronoiPointBiomes, voronoiBiomesSaveName, LoadCategory.Map);
        SaveAndLoad.Save(globalBiomeMap.Distortion, biomeDistortionSaveName, LoadCategory.Map, SerialisationType.Json);
        SaveAndLoad.Save(octaveOffsets, octaveOffsetsSaveName, LoadCategory.Map);
        SaveAndLoad.Save(globalObjects, globalObjectsSaveName, LoadCategory.Map);
        //SaveBiomeMap(globalBiomeMap.biomeMap);

        return new GlobalMap(lowDensityHeightMap, globalBiomeMap.biomeMap, globalBiomeMap.voronoiPointLocations, globalBiomeMap.voronoiPointBiomes, globalBiomeMap.Distortion, octaveOffsets, globalObjects);
        
        // void SaveBiomeMap(ushort[,] biomeMap)
        // {
        //     SaveAndLoad.SaveBytes(TextureGenerator.TextureFromColorMap(TextureGenerator.GetGlobalBiomeTexture(biomeMap, settings.Biomes)).EncodeToPNG(), "BiomeMapImage", LoadCategory.Map);
        // }
    }
}
public class GlobalMap //struct that holds the data for the global map
{
    const string poissonDiscGridSaveName = "poissonDiscGrid";

    public readonly Vector2[] octaveOffsets;
    public readonly Vector2 biomeDistortion;
    public readonly float[,] heightMap;
    public readonly ushort[,] biomeMap;
    public readonly Vector2[,] voronoiPointLocations;
    public readonly ushort[,] voronoiPointBiomes;
    public BiomeBorderPoint[,][] borderPoints;
    public SerializableList<GlobalObjectPoint>[,] globalObjects;
    public PoissonDiscPoint[,] globalPoissonDiscPoints;
    public GlobalMap(float[,] heightMap, ushort[,] biomeMap, Vector2[,] voronoiPointLocations, ushort[,] voronoiPointBiomes, Vector2 biomeDistortion, Vector2[] octaveOffsets, SerializableList<GlobalObjectPoint>[,] globalObjects)
    {
        this.octaveOffsets = octaveOffsets;
        this.heightMap = heightMap;
        this.biomeMap = biomeMap;
        this.voronoiPointLocations = voronoiPointLocations;
        this.voronoiPointBiomes = voronoiPointBiomes;
        this.biomeDistortion = biomeDistortion;
        this.globalObjects = globalObjects;
        int chunkCount = MapGenerator.settings.mapSize / MapGenerator.settings.chunkSize;
        
        borderPoints = new BiomeBorderPoint[chunkCount,chunkCount][];
        float cellSize = ObjectPlacer.minRadius / Mathf.Sqrt(2);
        globalPoissonDiscPoints = new PoissonDiscPoint[(int)(MapGenerator.settings.mapSize/cellSize), (int)(MapGenerator.settings.mapSize/cellSize)];
    }
    public ChunkData GetChunkData(Vector2Int chunkCoord)
    {
        //this function extracts the chunk data from a part of the map data
        int chunkSize = MapGenerator.settings.chunkSize;
        Vector2Int position = chunkCoord * chunkSize;
        int Simplification = MapGenerator.settings.Simplification;
        int lowDensityChunkSize = chunkSize / Simplification;
        Vector2Int lowDensityPosition = position / Simplification;
        float[,] newHeightMap = new float[lowDensityChunkSize + 1, lowDensityChunkSize + 1];
        for (int x = 0; x < lowDensityChunkSize + 1; x++)
            for (int y = 0; y < lowDensityChunkSize + 1; y++)
                newHeightMap[x,y] = heightMap[x + lowDensityPosition.x, lowDensityPosition.y + lowDensityChunkSize - y];

        // BiomeMap Gen
        BiomeChunk biomeChunk = BiomeMapGenerator.GenerateBiomeChunk(MapGenerator.settings, position, voronoiPointBiomes, voronoiPointLocations, biomeDistortion);
        borderPoints[chunkCoord.x,chunkCoord.y] = biomeChunk.borderPoints;

        // Get "unsmoothed" textures
        ChunkTextures textures = TextureGenerator.GetChunkTextures(biomeChunk.biomeMap, MapGenerator.settings.Biomes, position, octaveOffsets[1], MapGenerator.settings.ColorRegionScale, GlobalData.Seed);

        return new ChunkData(newHeightMap, biomeChunk.biomeMap, textures, globalObjects[chunkCoord.x, chunkCoord.y]);
    }
    public GeneratedChunk GenerateChunk(ushort[,] biomeChunk, Vector2Int chunkCoord, int seed, bool generateObjects)
    {
        //this function generates a normal density chunk
        Vector2Int position = chunkCoord * MapGenerator.settings.chunkSize;
        GeneratedChunk generatedChunk = Generate();

        NearBiomeList[,] nearBiomeMap = TextureGenerator.GenerateTextureLerpMap(MapGenerator.settings, biomeChunk, borderPoints, position);
        generatedChunk.AddColorMap(TextureGenerator.ColorMapFromBiomeChunk(MapGenerator.settings.chunkSize, nearBiomeMap, MapGenerator.settings.Biomes, position, octaveOffsets[1], MapGenerator.settings.ColorRegionScale, GlobalData.Seed));
        return generatedChunk;

        // Local generate function
        GeneratedChunk Generate()
        {
            int chunkSize = MapGenerator.settings.chunkSize;
            float[,] heightMap = HeightMapGenerator.GenerateNoiseChunk(MapGenerator.settings, biomeChunk, this, position);
            Vector3[,] normalMap = NormalMapGenerator.CalculateNormals(heightMap);
            ObjectSpawnPoints objectSpawnPoints;
            if (generateObjects)
                objectSpawnPoints = ObjectPlacer.GeneratePoints(chunkSize, seed + position.x + position.y * chunkSize, normalMap, heightMap, biomeChunk, MapGenerator.settings.Biomes, chunkCoord, globalObjects, globalPoissonDiscPoints);
            else
                objectSpawnPoints = new();
            return new GeneratedChunk(heightMap, objectSpawnPoints);
        }
    }
    public static GlobalMap LoadGlobalMap()
    {
        float[,] heightMap = SaveAndLoad.LoadArray2D<float>(MapGenerator.globalHeightMapSaveName, LoadCategory.Map);
        ushort[,] biomeMap = SaveAndLoad.LoadArray2D<ushort>(MapGenerator.globalBiomeMapSaveName, LoadCategory.Map);
        Vector2[,] voronoiPointLocations = SaveAndLoad.LoadArray2D<Vector2>(MapGenerator.voronoiLocationsSaveName, LoadCategory.Map);
        ushort[,] voronoiPointBiomes = SaveAndLoad.LoadArray2D<ushort>(MapGenerator.voronoiBiomesSaveName, LoadCategory.Map);
        Vector2 biomeDistortion = SaveAndLoad.Load<Vector2>(MapGenerator.biomeDistortionSaveName, LoadCategory.Map);
        Vector2[] octaveOffsets = SaveAndLoad.LoadArray<Vector2>(MapGenerator.octaveOffsetsSaveName, LoadCategory.Map);
        SerializableList<GlobalObjectPoint>[,] globalObjects = SaveAndLoad.LoadArray2D<SerializableList<GlobalObjectPoint>>(MapGenerator.globalObjectsSaveName, LoadCategory.Map);
        GlobalMap globalMap = new(heightMap, biomeMap, voronoiPointLocations, voronoiPointBiomes, biomeDistortion, octaveOffsets, globalObjects);
        return globalMap;
    }
}
public class ChunkData //struct that holds the mesh data from a low density chunk
{
    public readonly float[,] heightMap;
    public readonly ushort[,] biomeMap;
    public readonly ChunkTextures textures;
    public readonly SerializableList<GlobalObjectPoint> globalObjects;

    public ChunkData (float[,] heightMap, ushort[,] biomeMap, ChunkTextures textures, SerializableList<GlobalObjectPoint> globalObjects)
	{
		this.heightMap = heightMap;
        this.biomeMap = biomeMap;
        this.textures = textures;
        this.globalObjects = globalObjects;
	}
}
public struct GeneratedChunk  //struct that holds the mesh data from a chunk
{
    public readonly float[,] heightMap;
    public readonly ObjectSpawnPoints objectSpawnPoints;
    public Color[] ColorMap;

    public GeneratedChunk (float[,] heightMap, ObjectSpawnPoints objectSpawnPoints)
	{
		this.heightMap = heightMap;
        this.objectSpawnPoints = objectSpawnPoints;
        ColorMap = new Color[0];
	}
    public void AddColorMap (Color[] ColorMap)
    {
        this.ColorMap = ColorMap;
    }
}
[System.Serializable]
public struct NearBiomeList
{
    public List<ushort> NearBiomes;
    public List<float> NearBiomesWeight;
    public NearBiomeList(List<ushort> NearBiomes, List<float> NearBiomesWeight)
    {
        this.NearBiomes = NearBiomes;
        this.NearBiomesWeight = NearBiomesWeight;
    }
}