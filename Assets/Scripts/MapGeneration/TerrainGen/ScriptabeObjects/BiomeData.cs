using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Biome", menuName = "CustomObjects/Terrain/Biome")]
public class BiomeData : ScriptableObject // Scriptable Object that holds all generation informations for a biome
{
    public Gradient Color;
    [Header("--Heightmap Generation Values")]
    [Range(1, 100)]
    public int height = 50;
    [Range(0, 1)]
    public float persistance = 0.5f;
    [Range(1, 3)]
    public float lacunarity = 3f;
    public float noiseScale = 100f;
    public float HeightMultiplier = 6f;

    [Header("--Biomemap Generation Values")]
    [Range(1, 100)]
    public int frequency = 50;
    public RangeSlider placementHeight;
    public RangeSlider distanceFromSpawn;

    [Header("--Spawnable Objects")]
    public List<SpawnableObject> Objects;

    [Header("--Spawnable Global Objects")]
    public List<SpawnableGlobalObject> GlobalObjects;

    [Header("--Grass")]
    public bool GenerateGrass;
}
