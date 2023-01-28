using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class BiomeMapGenerator
{
    public static GlobalBiomeMap GenerateGlobalBiomeMap(TerrainSettings settings)
    {
        // This method creates a biomeMap that calculates the shapes of the different map-biomes
        // and the points at the border of the biomes.
        ushort[,] biomeMap = new ushort[settings.mapSize / settings.Simplification, settings.mapSize / settings.Simplification];
        bool[,] borderMap = new bool[settings.mapSize, settings.mapSize];
        int biomeCount = settings.mapSize / settings.biomeSize - 1;
        ushort[,] voronoiPointBiomes = new ushort[biomeCount, biomeCount];
        Vector2[,] voronoiPointLocations = new Vector2[biomeCount, biomeCount];
        int islandCount = settings.mapSize / settings.islandSize;
        Vector2[,] islandPointLocations = new Vector2[islandCount, islandCount];
        BiomeBorderPoint[,][] globalBorderPoints = new BiomeBorderPoint[settings.mapSize/settings.chunkSize, settings.mapSize/settings.chunkSize][];

        CreateIslands();

        // Set the biomeSpawnPoints
        for (int x = 0; x < biomeCount; x++)
        {
            for (int y = 0; y < biomeCount; y++)
            {
                int Xpos = Random.Range(x * settings.biomeSize, x * settings.biomeSize + settings.biomeSize);
                int Ypos = Random.Range(y * settings.biomeSize, y * settings.biomeSize + settings.biomeSize);
                voronoiPointLocations[x, y] = new Vector2(Xpos, Ypos);
                voronoiPointBiomes[x, y] = GetBiome(x, y, biomeCount, settings.Biomes);
            }
        }

        // Voronoi Caluculation 
        Vector2 distortion = new(Random.value * settings.BiomeDistortion, Random.value * settings.BiomeDistortion);
        for (int x = 0; x < settings.mapSize; x += settings.Simplification)
        {
            for (int y = 0; y < settings.mapSize; y += settings.Simplification)
            {
                //get the closest Biome cell
                float smallestDistance = float.MaxValue;
                ushort closestBiome = 0;
                Vector2Int currentBiome = new(x / settings.biomeSize, y / settings.biomeSize);
                float PerlinX = x + (Mathf.PerlinNoise(x / settings.DistortionPerlinScale, y / settings.DistortionPerlinScale) * 2 - 1) * distortion.x;
                float PerlinY = y + (Mathf.PerlinNoise(x / settings.DistortionPerlinScale, y / settings.DistortionPerlinScale) * 2 - 1) * distortion.y;
                for (int biomeX = currentBiome.x - 1; biomeX <= currentBiome.x + 1; biomeX++)
                {
                    for (int biomeY = currentBiome.y - 1; biomeY <= currentBiome.y + 1; biomeY++)
                    {
                        if (biomeX >= 0 && biomeY >= 0 && biomeX < biomeCount && biomeY < biomeCount)
                        {
                            float currentDistance = Vector2.Distance(voronoiPointLocations[biomeX, biomeY], new Vector2(PerlinX,PerlinY));
                            if (currentDistance < smallestDistance)
                            {
                                closestBiome = voronoiPointBiomes[biomeX, biomeY];
                                smallestDistance = currentDistance;
                            }
                        }
                    }
                }
                biomeMap[x/settings.Simplification,y/settings.Simplification] = closestBiome;
                //check if it is a border point
                if (x != 0 && y != 0)
                {
                    if (globalBorderPoints[x/settings.chunkSize, y/settings.chunkSize] == null)
                        globalBorderPoints[x/settings.chunkSize, y/settings.chunkSize] = new BiomeBorderPoint[0];
                    List<BiomeBorderPoint> tempList = globalBorderPoints[x/settings.chunkSize, y/settings.chunkSize].ToList();
                    BiomeBorderCheck(ref tempList, x / settings.Simplification, y / settings.Simplification, x, y, biomeMap, ref borderMap);
                    globalBorderPoints[x/settings.chunkSize, y/settings.chunkSize] = tempList.ToArray();
                }
            }
        }
        return new GlobalBiomeMap(biomeMap, globalBorderPoints, voronoiPointBiomes, voronoiPointLocations, distortion);

        void CreateIslands()
        {
            // Set the Island Locations
            for (int x = 0; x < islandCount; x++)
            {
                for (int y = 0; y < islandCount; y++)
                {
                    int Xpos = Random.Range(x * settings.islandSize, x * settings.islandSize + settings.islandSize);
                    int Ypos = Random.Range(y * settings.islandSize, y * settings.islandSize + settings.islandSize);
                    islandPointLocations[x, y] = new Vector2(Xpos, Ypos);
                }
            }
        }

        ushort GetBiome(int x, int y, int biomeCount, BiomeData[] Biomes)
        {
            // This Method finds the Biome for a specific voronoi Point
            if (x <= 0 || y <= 0 || x >= biomeCount - 1 || y >= biomeCount - 1)
                return 0; // First all border Biomes are forced to be Oceans
            else
            {
                float[] BiomeWeights = new float[Biomes.Length];
                float Height = GetHeight(x, y, islandPointLocations);
                Vector2 centerPoint = Vector2.one * settings.mapSize * 0.5f;
                float distFromSpawn = Vector2.Distance(voronoiPointLocations[x,y], centerPoint);
                if (distFromSpawn < settings.spawnSize)
                    return 0; // checking if the biome is in the spawn region;
                float normalizedDistFromSpawn = Mathf.Clamp01(distFromSpawn / (settings.mapSize * 0.5f));
                for (int i = 0; i < Biomes.Length; i++)
                {
                    // Also the heightValue is calculated from the perlinValue
                    float heightValue = Biomes[i].placementHeight.InBounds(Height) ? 1 : 0;
                    // The distance to spawn
                    float distanceValue = Biomes[i].distanceFromSpawn.InBounds(normalizedDistFromSpawn) ? 1 : 0;
                    // The Frequency value needs no Computing:
                    float frequencyValue = Biomes[i].frequency;
                        
                    BiomeWeights[i] = frequencyValue * heightValue * distanceValue;
                }
                // Finding the actual Biome that should be at place XY
                return (ushort)Utility.WeightedRandom(BiomeWeights);
            }
        }
        float GetHeight(int x, int y, Vector2[,] islandPointLocations)
        {
            float smallestDistance = float.MaxValue;
            Vector2Int currentCell = new(x / settings.islandSize, y / settings.islandSize);
            Vector2Int closestIsland = Vector2Int.zero;
            Vector2 toClosestIsland = Vector2.zero;
            for (int islandX = currentCell.x - 1; islandX <= currentCell.x + 1; islandX++)
            {
                for (int islandY = currentCell.y - 1; islandY <= currentCell.y + 1; islandY++)
                {
                    if (islandX >= 0 && islandY >= 0 && islandX < islandCount && islandY < islandCount)
                    {
                        float currentDistance = Vector2.Distance(islandPointLocations[islandX, islandY], new Vector2(x,y));
                        if (currentDistance < smallestDistance)
                        {
                            smallestDistance = currentDistance;
                            closestIsland = new Vector2Int(islandX, islandY);
                            toClosestIsland = islandPointLocations[islandX, islandY] - new Vector2(x,y);
                        }
                    }
                }
            }
            
            float minDistToIslandEdge = float.MaxValue;
            for (int islandX = currentCell.x - 1; islandX <= currentCell.x + 1; islandX++)
            {
                for (int islandY = currentCell.y - 1; islandY <= currentCell.y + 1; islandY++)
                {
                    if (islandX < 0 || islandY < 0 || islandX >= islandCount || islandY >= islandCount)
                        continue;
                    
                    Vector2 islandPos = islandPointLocations[islandX, islandY];
                    if (closestIsland.x == islandX && closestIsland.y == islandY)
                        continue;
                    
                    Vector2 toIsland = islandPos - new Vector2(x,y);
                    Vector2 toCenter = (toClosestIsland + toIsland) * 0.5f;
                    Vector2 islandDifference = (toIsland - toClosestIsland).normalized;
                    float edgeDistance = Vector2.Dot(toCenter, islandDifference);
                    if (edgeDistance < minDistToIslandEdge)
                        minDistToIslandEdge = edgeDistance;
                }
            }
            return minDistToIslandEdge / settings.islandSize;
        }
    }
    public static BiomeChunk GenerateBiomeChunk(TerrainSettings settings, Vector2Int pos, ushort[,] voronoiPointBiomes, Vector2[,] voronoiPointLocations, Vector2 Distortion)
    {
        // This method creates a biomeMap that calculates the shapes of the different map-biomes
        // and the points at the border of the biomes.
        int mapChunkSize = settings.chunkSize + 1;
        ushort[,] biomeMap = new ushort[mapChunkSize, mapChunkSize];
        ushort[,] borderBiomeMap = new ushort[mapChunkSize + 2, mapChunkSize + 2];
        bool[,] borderMap = new bool[mapChunkSize + 2, mapChunkSize + 2];
        int biomeCount = settings.mapSize / settings.biomeSize - 1;
        List<BiomeBorderPoint> borderPoints = new();

        // Voronoi Caluculation 
        for (int x = -1; x < mapChunkSize + 1; x++)
        {
            for (int y = -1; y < mapChunkSize + 1; y++)
            {
                //get the closest Biome cell
                int rX = x + pos.x;
                int rY = y + pos.y;
                float smallestDistance = float.MaxValue;
                ushort closestBiome = 0;
                int currentBiomeX = rX / settings.biomeSize;
                int currentBiomeY = rY / settings.biomeSize;
                float PerlinX = rX + (Mathf.PerlinNoise(rX / settings.DistortionPerlinScale, rY / settings.DistortionPerlinScale) * 2 - 1) * Distortion.x;
                float PerlinY = rY + (Mathf.PerlinNoise(rX / settings.DistortionPerlinScale, rY / settings.DistortionPerlinScale) * 2 - 1) * Distortion.y;
                for (int biomeX = currentBiomeX - 1; biomeX <= currentBiomeX + 1; biomeX++)
                {
                    for (int biomeY = currentBiomeY - 1; biomeY <= currentBiomeY + 1; biomeY++)
                    {
                        if (biomeX >= 0 && biomeY >= 0 && biomeX < biomeCount && biomeY < biomeCount)
                        {
                            float currentDistance = (voronoiPointLocations[biomeX, biomeY] - new Vector2(PerlinX,PerlinY)).sqrMagnitude;
                            if (currentDistance < smallestDistance)
                            {
                                closestBiome = voronoiPointBiomes[biomeX, biomeY];
                                smallestDistance = currentDistance;
                            }
                        }
                    }
                }
                borderBiomeMap[x + 1,y + 1] = closestBiome;
                //check if it is a border point
                if (x >= 0 && x < mapChunkSize && y >= 0 && y < mapChunkSize)
                {
                    biomeMap[x,mapChunkSize - y - 1] = closestBiome;
                    BiomeBorderCheck(ref borderPoints, x + 1, y + 1, x + pos.x, y + pos.y, borderBiomeMap, ref borderMap);
                }
            }
        }
        return new BiomeChunk(biomeMap, borderPoints.ToArray());
    }
    private static void BiomeBorderCheck(ref List<BiomeBorderPoint> borderPoints, int x, int y, int globalX, int globalY, ushort[,] biomeMap, ref bool[,] borderMap)
    {
        ushort[] adjacentBiomes = new ushort[2];
        bool[] adjacentBorders = new bool[2];

        adjacentBiomes[0] = biomeMap[x,y-1];
        adjacentBiomes[1] = biomeMap[x-1,y];

        adjacentBorders[0] = borderMap[x,y-1];
        adjacentBorders[1] = borderMap[x-1,y];

        bool borderPoint = false;
        for (int i = 0; i < adjacentBiomes.Length; i++)
        {
            if (adjacentBiomes[i] != biomeMap[x, y] && !adjacentBorders[i])
            {
                borderMap[x,y] = true;
                if (borderPoint)
                {
                    for (int j = 0; j < borderPoints.Count; j++)
                    {
                        if (borderPoints[j].Position == new Vector2Int(globalX, globalY))
                        {
                            borderPoints[j].Biomes.Add(adjacentBiomes[i]);
                            break;
                        }
                    }
                }
                else
                {
                    if (borderPoints == null) { borderPoints = new List<BiomeBorderPoint>(); }
                    borderPoints.Add(new BiomeBorderPoint(new Vector2Int(globalX, globalY), new (adjacentBiomes[i], biomeMap[x,y])));
                }
                borderPoint = true;
            }
        }
    }
}
[System.Serializable]
public struct BiomeBorderPoint
{
    public Vector2Int Position;
    public AdjacentBiomes Biomes;
    public BiomeBorderPoint(Vector2Int Position, AdjacentBiomes Biomes)
    {
        this.Position = Position;
        this.Biomes = Biomes;
    }
}
[System.Serializable]
public struct AdjacentBiomes // A really weired class to replace a list while not being a reference type. Its 1.06 in the morning.
{
    public ushort B1;
    public ushort B2;
    public ushort B3;
    public bool P3;
    public AdjacentBiomes(ushort B1, ushort B2)
    {
        this.B1 = B1;
        this.B2 = B2;
        B3 = 0;
        P3 = false;
    }
    public void Add(ushort newB)
    {
        B3 = newB;
        P3 = true;
    }
}
public struct GlobalBiomeMap
{
    public readonly Vector2 Distortion;
    public readonly ushort[,] biomeMap;
    public readonly BiomeBorderPoint[,][] borderPoints;
    public readonly ushort[,] voronoiPointBiomes;
    public readonly Vector2[,] voronoiPointLocations;
    public GlobalBiomeMap(ushort[,] biomeMap, BiomeBorderPoint[,][] borderPoints, ushort[,] voronoiPointBiomes, Vector2[,] voronoiPointLocations, Vector2 Distortion)
    {
        this.biomeMap = biomeMap;
        this.borderPoints = borderPoints;
        this.voronoiPointBiomes = voronoiPointBiomes;
        this.voronoiPointLocations = voronoiPointLocations;
        this.Distortion = Distortion;
    }
}
[System.Serializable]
public struct BiomeChunk
{
    public readonly ushort[,] biomeMap;
    public readonly BiomeBorderPoint[] borderPoints;
    public BiomeChunk(ushort[,] biomeMap, BiomeBorderPoint[] borderPoints)
    {
        this.biomeMap = biomeMap;
        this.borderPoints = borderPoints;
    }
}