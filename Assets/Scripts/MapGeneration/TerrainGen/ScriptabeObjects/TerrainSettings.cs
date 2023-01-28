using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain Settings", menuName = "CustomObjects/Terrain/TerrainSettings")]
public class TerrainSettings : ScriptableObject
{
    [Space(3, order=0)]
    [Header("---General:", order=1)]
    public int mapSize = 5000;
    public int islandSize = 600;
    public int Simplification = 25;
    public int spawnSize = 300;
    public float waterHeight;

    [Space(10, order=2)]
    [Header("--Chunk Loader", order=3)]
    public int maximumViewDistance = 1000;
    public int maximumGenerationDistance = 250;
    public float colliderGenerationDistance = 150;
    public int chunkSize = 100;
    
    [Space(10, order=4)]
    [Header("--Biome Map:", order=5)]
    public int biomeSize = 200;
    public float DistortionPerlinScale = 50;
    public int BiomeDistortion = 50;
    public BiomeData[] Biomes;

    [Space(10, order=6)]
    [Header("--Height Map:", order=7)]
    [Range(2, 8)]
    public int octaves;
    public int Lerprange;

    [Space(10, order=8)]
    [Header("--Visuals:", order=9)]
    [Range(0, 50)] public float ColorRegionScale;
    public int TextureLerprange;
    public Material material;
    public Material grassMaterial;
    public Material waterMaterial;
}
