using System.Collections.Generic;
using UnityEngine;

public class MapRuntimeHandler : MonoSingleton<MapRuntimeHandler>
{
    [Space(3, order=0)]
    [Header("---TerrainSettings:", order=1)]
    [SerializeField] private TerrainSettings settings;
    [SerializeField] private bool alwaysGenerate;

    [Space(10, order=2)]
    [Header("--Objects", order=3)]
    [SerializeField] private Transform viewer;
    [SerializeField] private GameObject testMap;
    [SerializeField] private GameObject spawnIsland;
    [SerializeField] private Transform spawnPoint;

    [HideInInspector] public Mesh waterMesh;

    int chunksVisibleInViewDst;
    private Dictionary<Vector2Int, TerrainChunk> terrainChunkDictionary = new();
    [HideInInspector] public List<Vector2Int> GeneratedChunks = new();
    [HideInInspector] public List<TerrainChunk> visibleChunks = new();
    [HideInInspector] public Vector2 viewerPosition;
    Vector2 lastUpdatedViewerPos;
    const float viewerMoveThresholdForChunkUpdate = 40f;
    const float SqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    private int IslandChunksCount;
    [HideInInspector] public GlobalMap globalMap;
    [HideInInspector] public bool Load;
    private bool hasWaterMesh = false;
    private bool isStarted = false;
    public static float waterHeight => Instance.settings.waterHeight;

    void Start()
    {
        if (GlobalData.loadMode == LoadMode.TestMap)
            return;
        MapGenerator.Initialize(settings);
        Generate();
    }
    public void Generate()
    {
        if (isStarted)
            return;
        
        isStarted = true;
        testMap.SetActive(false);
        spawnIsland.SetActive(true);
        if (GlobalData.loadMode == LoadMode.NewGame)
            viewer.position = spawnPoint.position;
        IslandChunksCount = settings.mapSize / settings.chunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(settings.maximumViewDistance / settings.chunkSize);
        lastUpdatedViewerPos = new Vector2(viewer.position.x, viewer.position.z);
        ThreadingHandler.RequestData(() => MeshGenerator.GenerateMesh(new float[settings.chunkSize + 1,settings.chunkSize + 1]), createWaterMesh);
        Random.InitState(GlobalData.Seed);

        if (SaveAndLoad.DirectoryExists(LoadCategory.Map) && !alwaysGenerate)
        {
            globalMap = GlobalMap.LoadGlobalMap();
            Load = true;
        }
        else
            globalMap = MapGenerator.GenerateGlobalMap();
    }
    void Update()
    {
        viewerPosition = viewer.transform.position.XZ();

        if ((lastUpdatedViewerPos - viewerPosition).sqrMagnitude > SqrViewerMoveThresholdForChunkUpdate && hasWaterMesh && isStarted)
        {
            UpdateVisibleChunks();
            lastUpdatedViewerPos = viewerPosition;
        }
    }

    void UpdateVisibleChunks()
    {
        //This function is called from the void Update function every time the player moves more than the SqrViewerMoveThresholdForChunkUpdate variable
        //it checks if all visible chunks should still be visible and it gererates new chunks if the player enters a new area
        HashSet<Vector2> alreadyUpdatedChunkCoords = new();
        for (int i = visibleChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleChunks[i].coord);
            visibleChunks[i].UpdateTerrainChunk();
        }
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / settings.chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / settings.chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2Int viewedChunkCoord = new(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord) && viewedChunkCoord.x >= -IslandChunksCount * 0.5f && viewedChunkCoord.y >= -IslandChunksCount * 0.5f && viewedChunkCoord.x < IslandChunksCount * 0.5f && viewedChunkCoord.y < IslandChunksCount * 0.5f)
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    else
                    {
                        TerrainChunk newChunk = new(viewedChunkCoord, transform, settings, this);
                        if (xOffset == 0 && yOffset == 0)
                        {
                            MapLoadingScreen.Lock();
                            newChunk.GeneratedCollider += ViewerChunkLoaded;
                        }
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                    }
                }
            }
        }
        // ChunkLoadedObjects
        List<Bounds> visibleChunkBounds = new();
        foreach (TerrainChunk chunk in visibleChunks)
            visibleChunkBounds.Add(chunk.bounds);
        ChunkLoadedObject.UpdateVisibleChunks(visibleChunkBounds, settings.chunkSize);
    }
    void ViewerChunkLoaded(TerrainChunk chunk)
    {
        MapLoadingScreen.Unlock();
        UpdateVisibleChunks();
        chunk.GeneratedCollider -= ViewerChunkLoaded;
    }

    public void createWaterMesh(object Data)
    {
        // Water Mesh Generation Callback
        MeshData meshData = (MeshData)Data;
        waterMesh = meshData.CreateMesh();
        hasWaterMesh = true;
        UpdateVisibleChunks();
    }
    public static BiomeData GetBiomeApproximately(Vector2 position)
    {
        Vector2Int mapPos = (position + Vector2.one * Instance.settings.mapSize/2).RoundToInt();
        return Instance.settings.Biomes[Instance.globalMap.biomeMap[mapPos.x, mapPos.y]];
    }
    public static BiomeData GetViewerBiome()
    {
        if (MapLoadingScreen.Loading)
            return Instance.settings.Biomes[0];
        
        Vector2 viewerPosition = Instance.viewer.transform.position.XZ();
        Vector2 chunkPosition = new(Utility.Floor(viewerPosition.x, Instance.settings.chunkSize), Utility.Floor(viewerPosition.y, Instance.settings.chunkSize));
        Vector2Int viewerPositionChunk = (viewerPosition / Instance.settings.chunkSize).RoundToInt();

        return Instance.settings.Biomes[Instance.terrainChunkDictionary[viewerPositionChunk].GetBiome(viewerPosition - chunkPosition)];
    }
    [Command]
    public static void DebugBiome(List<string> args)
    {
        Console.Print(GetViewerBiome().name);
    }
}