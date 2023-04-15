using System.Collections.Generic;
using UnityEngine;
public static class HeightMapGenerator
{
    public static float[,] GenerateGlobalNoiseMap(TerrainSettings settings, GlobalBiomeMap biomeMap, Vector2[] octaveOffsets)
    {
        //This function generates a float-array-heightmap
        int simplificatedMapSize = settings.mapSize / settings.Simplification;
        float[,] noiseMap = new float[simplificatedMapSize, simplificatedMapSize];

        for (int y = 0; y < simplificatedMapSize; y++)
        {
            for (int x = 0; x < simplificatedMapSize; x++)
            {
                int rX = x * settings.Simplification;
                int rY = y * settings.Simplification;
                noiseMap[x,y] = CalculateHeight(settings, x, y, rX, rY, biomeMap.biomeMap, octaveOffsets, biomeMap.borderPoints);
            }
        }
        return noiseMap;
    }
    public static float[,] GenerateNoiseChunk(TerrainSettings settings, ushort[,] biomeMap, GlobalMap globalMap, Vector2Int pos)
    {
        //This function generates a float-array-heightchunk
        int mapChunkSize = settings.chunkSize + 1;
        float[,] noiseMap = new float[mapChunkSize, mapChunkSize];
        Vector2Int chunkCoord = new(pos.x / settings.chunkSize, pos.y / settings.chunkSize);
        int chunkCount = settings.mapSize / settings.chunkSize;

        // Creating List of relevant global objects for more performant checks
        List<(Vector2 position, float sqrtRadius, float height)> relevantGlobalObjects = new();
        for (int x = -1; x <= 1; x++)
			for (int y = -1; y <= 1; y++)
				if (chunkCoord.x + x > 0 && chunkCoord.x + x < chunkCount && chunkCoord.y + y > 0 && chunkCoord.y + y < chunkCount)
					foreach (GlobalObjectPoint point in globalMap.globalObjects[x+chunkCoord.x,y+chunkCoord.y].list)
                        relevantGlobalObjects.Add((point.localPosition + new Vector2(x,y) * settings.chunkSize, point.sqrtFlatRadius, point.height));
        
        // Looping through all the points and assigning the right height
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                bool globalObject = false;
                foreach (var point in relevantGlobalObjects)
                {
                    if ((new Vector2(x,y) - point.position).sqrMagnitude < point.sqrtRadius)
                    {
                        noiseMap[x,mapChunkSize - y - 1] = point.height;
                        globalObject = true;
                    }
                }
                if (!globalObject)
                    noiseMap[x,mapChunkSize - y - 1] = CalculateHeight(settings, x, mapChunkSize - y - 1, x + pos.x, y + pos.y, biomeMap, globalMap.octaveOffsets, globalMap.borderPoints);
            }
        }
        return noiseMap;
    }
    static float CalculateHeight(TerrainSettings settings, int x, int y, int realX, int realY, ushort[,] biomeMap, Vector2[] octaveOffsets, BiomeBorderPoint[,][] borderPoints)
    {
        List<ushort> NearBiomes = new();
        List<float> NearBiomesWeight = new();
        NearBiomes.Add(biomeMap[x,y]);
        NearBiomesWeight.Add(0f);
        float MainBiomeWeight = settings.Lerprange;
        Vector2Int currentChunk = new(realX / settings.chunkSize, realY / settings.chunkSize);
        int chunkCount = settings.mapSize / settings.chunkSize;
        for (int chunkX = currentChunk.x - 1; chunkX <= currentChunk.x + 1; chunkX++)
        {
            for (int chunkY = currentChunk.y - 1; chunkY <= currentChunk.y + 1; chunkY++)
            {
                if (chunkX >= 0 && chunkY >= 0 && chunkX < chunkCount && chunkY < chunkCount && borderPoints[chunkX, chunkY] != null)
                {
                    BiomeBorderPoint[] BPL = borderPoints[chunkX, chunkY];
                    foreach (BiomeBorderPoint borderPoint in BPL)
                    {
                        float currentDistance = Vector2.Distance(borderPoint.Position, new Vector2Int(realX, realY));
                        if (currentDistance < settings.Lerprange)
                        {
                            ushort firstBiome = borderPoint.Biomes.B1;
                            ushort secondBiome = borderPoint.Biomes.B2;
                            ushort thirdBiome = borderPoint.Biomes.B3;
                            CalculateWeight(firstBiome, currentDistance);
                            CalculateWeight(secondBiome, currentDistance);
                            if (borderPoint.Biomes.P3)
                                CalculateWeight(thirdBiome, currentDistance);
                            if (currentDistance < MainBiomeWeight)
                                MainBiomeWeight = currentDistance;
                        }
                    }
                }
            }
        }
        NearBiomesWeight[0] = settings.Lerprange + MainBiomeWeight;
        List<float> NearBiomesHeight = new ();
        foreach (ushort i in NearBiomes)
        {
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            float persistance = settings.Biomes[i].persistance;
            float lacunarity = settings.Biomes[i].lacunarity;
            float noiseScale = settings.Biomes[i].noiseScale;
            float height = settings.Biomes[i].height;
            float heightMultiplier = settings.Biomes[i].HeightMultiplier;
                
            for (int t = 0; t < settings.octaves; t++)
            {
                float sampleX = (realX + octaveOffsets[t].x) / noiseScale * frequency;
                float sampleY = (realY + octaveOffsets[t].y) / noiseScale * frequency;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }
            NearBiomesHeight.Add((noiseHeight * heightMultiplier) + height);
        }
        return LerpValues(NearBiomesHeight, NearBiomesWeight);

        void CalculateWeight(ushort Biome, float currentDistance)
        {
            if (Biome != biomeMap[x, y])
            {
                if (!NearBiomes.Contains(Biome))
                {
                    NearBiomes.Add(Biome);
                    NearBiomesWeight.Add(settings.Lerprange - currentDistance);
                }
                else if (settings.Lerprange - currentDistance > NearBiomesWeight[NearBiomes.IndexOf(Biome)])
                    NearBiomesWeight[NearBiomes.IndexOf(Biome)] = settings.Lerprange - currentDistance;
            }
        }
    }
    private static float LerpValues(List<float> Values, List<float> ValueWeights)
    {
        // this function can lerp more than two values together by looping through all values and adding them to an endresult via the Mathf.Lerp function
        float combinedValue = Values[0];
        float combinedWeight = ValueWeights[0];
        for (int i = 1; i < Values.Count; i++)
        {
            float lerpValue = combinedWeight / (combinedWeight + ValueWeights[i]);
            combinedValue = Mathf.Lerp(Values[i], combinedValue, lerpValue);
            combinedWeight += ValueWeights[i];
        }
        return combinedValue;
    }
}