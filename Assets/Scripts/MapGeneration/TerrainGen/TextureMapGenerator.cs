using UnityEngine;
using System.Collections.Generic;
public class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap)
	{
        // This Function converts a color array into a texture2DArray
        int colorSideLength = (int)Mathf.Sqrt(colorMap.Length);
		Texture2D texture = new(colorSideLength, colorSideLength);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colorMap);
		texture.Apply();
		return texture;
	}
    public static Color[] GetGlobalBiomeTexture(ushort[,] biomeMap, BiomeData[] Biomes)
    {
        int chunkSize = biomeMap.GetLength(0);
        Color[] ColorMap = new Color[chunkSize * chunkSize];
        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
                ColorMap[x + y * chunkSize] = Biomes[biomeMap[x,y]].Color.Evaluate(0);
        return ColorMap;
    }

    private static Color[] GrassMapFromBiomeChunk(ushort[,] biomeMap, BiomeData[] Biomes)
    {
        int chunkSize = biomeMap.GetLength(0);
        Color[] GrassMap = new Color[chunkSize * chunkSize];
        for (int x = 0; x < chunkSize; x++)
            for (int y = 0; y < chunkSize; y++)
                GrassMap[x + y * chunkSize] = Biomes[biomeMap[x,y]].GenerateGrass? Color.white : Color.black;
        return GrassMap;
    }
    private static Color[] LowDensityColorMapFromBiomeChunk(ushort[,] biomeMap, BiomeData[] Biomes, Vector2Int position, Vector2 Offsets, float ColorDistortion, int seed)
    {
        System.Random prng = new(seed);
        int chunkSize = biomeMap.GetLength(0);
        Color[] ColorMap = new Color[chunkSize * chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                float PerlinX = (position.x + x + Offsets.x) / ColorDistortion;
                float PerlinY = (chunkSize + position.y - y + Offsets.y) / ColorDistortion;
                ColorMap[x + y * chunkSize] = Biomes[biomeMap[x,y]].Color.Evaluate(Mathf.PerlinNoise(PerlinX, PerlinY) + (float)prng.NextDouble() * 0.25f);
            }
        }
        return ColorMap;
    }
    public static ChunkTextures GetChunkTextures(ushort[,] biomeMap, BiomeData[] Biomes, Vector2Int position, Vector2 Offsets, float ColorDistortion, int seed)
    {
        return new ChunkTextures(LowDensityColorMapFromBiomeChunk(biomeMap, Biomes, position, Offsets, ColorDistortion, GlobalData.Seed), GrassMapFromBiomeChunk(biomeMap, Biomes));
    }
    public static NearBiomeList[,] GenerateTextureLerpMap(TerrainSettings settings, ushort[,] biomeMap, BiomeBorderPoint[,][] borderPoints, Vector2Int pos)
    {
        int mapChunkSize = settings.chunkSize;
        NearBiomeList[,] nearBiomeList = new NearBiomeList[mapChunkSize, mapChunkSize];
        
        for (int y = 0; y < mapChunkSize; y++)
            for (int x = 0; x < mapChunkSize; x++)
                nearBiomeList[x,y] = CalculateTextureWeight(settings, x, y, x + pos.x, mapChunkSize + pos.y - y, biomeMap, borderPoints);
        return nearBiomeList;
    }
    private static NearBiomeList CalculateTextureWeight(TerrainSettings settings, int x, int y, int realX, int realY, ushort[,] biomeMap, BiomeBorderPoint[,][] borderPoints)
    {
        List<ushort> NearBiomes = new();
        List<float> NearBiomesWeight = new();
        NearBiomes.Add(biomeMap[x,y]);
        NearBiomesWeight.Add(0f);
        float MainBiomeWeight = settings.TextureLerprange;
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
                        if (currentDistance < settings.TextureLerprange)
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
        NearBiomesWeight[0] = settings.TextureLerprange + MainBiomeWeight;
        return new NearBiomeList(NearBiomes, NearBiomesWeight);

        void CalculateWeight(ushort Biome, float currentDistance)
        {
            if (Biome != biomeMap[x, y])
            {
                if (!NearBiomes.Contains(Biome))
                {
                    NearBiomes.Add(Biome);
                    NearBiomesWeight.Add(settings.TextureLerprange - currentDistance);
                }
                else if (settings.TextureLerprange - currentDistance > NearBiomesWeight[NearBiomes.IndexOf(Biome)])
                    NearBiomesWeight[NearBiomes.IndexOf(Biome)] = settings.TextureLerprange - currentDistance;
            }
        }
    }
    public static Color[] ColorMapFromBiomeChunk(int chunkSize, NearBiomeList[,] nearBiomeMap, BiomeData[] Biomes, Vector2Int position, Vector2 Offsets, float ColorDistortion, int seed)
    {
        System.Random prng = new(seed);
        Color[] ColorMap = new Color[chunkSize * chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                float X = position.x + x + Offsets.x;
                float Y = chunkSize + position.y - y + Offsets.y;
                List<Color> NearBiomesColor = new();
                float Perlin = Mathf.PerlinNoise(X/ColorDistortion, Y/ColorDistortion) + (float)prng.NextDouble() / 2f;
                foreach (int i in nearBiomeMap[x,y].NearBiomes)
                    NearBiomesColor.Add(Biomes[i].Color.Evaluate(Perlin));
                ColorMap[x + y * chunkSize] = LerpColors(NearBiomesColor, nearBiomeMap[x,y].NearBiomesWeight);
            }
        }
        return ColorMap;
    }
    private static Color LerpColors(List<Color> Values, List<float> ValueWeights)
    {
        // this function can lerp more than two values together by looping through all values and adding them to an endresult via the Mathf.Lerp function
        Color combinedValue = Values[0];
        float combinedWeight = ValueWeights[0];
        for (int i = 1; i < Values.Count; i++)
        {
            float lerpValue = combinedWeight / (combinedWeight + ValueWeights[i]);
            combinedValue = Color.Lerp(Values[i], combinedValue, lerpValue);
            combinedWeight += ValueWeights[i];
        }
        return combinedValue;
    }
}
public struct ChunkTextures
{
    public Color[] ColorMap;
    public Color[] GrassMap;
    public ChunkTextures(Color[] ColorMap, Color[] GrassMap)
    {
        this.ColorMap = ColorMap;
        this.GrassMap = GrassMap;
    }
}