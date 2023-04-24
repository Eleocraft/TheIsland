using UnityEngine;
using System;

public class TerrainChunk //The class that holds all informations to the terrainchunks
{
    public Vector2Int coord;
    readonly GameObject meshObject;
    readonly GameObject waterObject;
    readonly GameObject objectHolder;
    Vector2Int position;
    public Bounds bounds;
    readonly MeshRenderer meshRenderer;
	readonly MeshFilter meshFilter;
    readonly MeshCollider meshCollider;
    ChunkData chunkData;

    readonly TerrainSettings settings;
    readonly MapRuntimeHandler MRH;

    Mesh lowDensityMesh;
    Mesh mesh;

    bool hasCollider;
    bool hasMesh;
    bool isGenerated;
    bool MeshRequestet;

    public Action<TerrainChunk> GeneratedCollider;

    readonly string saveName;
    public TerrainChunk(Vector2Int realCoord, Transform parent, TerrainSettings settings, MapRuntimeHandler MRH) // Constructor
    {
        this.settings = settings;
        this.MRH = MRH;
        coord = realCoord + Vector2Int.one * settings.mapSize/2 / settings.chunkSize;
        position = realCoord * settings.chunkSize;
        Vector3 positionV3 = new(position.x, 0, position.y);
        Vector3 waterPosition = new(position.x, settings.waterHeight, position.y);
        bounds = new Bounds(position - Vector2.one * settings.chunkSize / 2, Vector2.one * settings.chunkSize);

        saveName = $"Chunk X.{coord.x} Y.{coord.y} ";

        //The GameObjects are created and set up
        meshObject = new GameObject($"TerrainChunk {saveName}");
        waterObject = new GameObject("WaterChunk");
        objectHolder = new GameObject("Objects");

        meshObject.transform.position = positionV3;
        meshObject.transform.localScale = Vector3.one;
        meshObject.layer = 7;

        waterObject.transform.position = waterPosition;
        waterObject.transform.localScale = Vector3.one;
    
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();

        meshRenderer.materials = new Material[] { settings.material, settings.grassMaterial };

        meshObject.transform.parent = parent;

        waterObject.AddComponent<MeshRenderer>().material = settings.waterMaterial;
        waterObject.AddComponent<MeshFilter>().mesh = MRH.waterMesh;
        waterObject.transform.parent = meshObject.transform;

        objectHolder.transform.parent = meshObject.transform;
        objectHolder.transform.localPosition = Vector3.zero;

        SetVisible(false);
        //The low-density-Heightmap of this chunk is extracted from the general Map Data
        ThreadingHandler.RequestData(() => MRH.globalMap.GetChunkData(coord), ChunkDataCallback);
    }
    //  Low Density Chunk Callbacks
    void ChunkDataCallback(object Data)
    {
        MRH.GeneratedChunks.Add(coord);
        // LowDensityChunk extraction callback
        chunkData = (ChunkData)Data;
        // Texture Generation
        Texture2D Tex = TextureGenerator.TextureFromColorMap(chunkData.textures.ColorMap);
        Texture2D gTex = TextureGenerator.TextureFromColorMap(chunkData.textures.GrassMap);
        meshRenderer.materials[0].SetTexture("_ColorMap", Tex);
        meshRenderer.materials[1].SetTexture("_ColorMap", Tex);
        meshRenderer.materials[1].SetTexture("_GrassMap", gTex);
        
        CreateGlobalObjects();
        // Mesh Generation
        ThreadingHandler.RequestData(() => MeshGenerator.GenerateMesh(chunkData.heightMap, settings.Simplification), CreateLowDensityMesh);

        void CreateGlobalObjects()
        {
            // Create all global objects in chunk
            foreach (GlobalObjectPoint point in chunkData.globalObjects.list)
            {
                GameObject prefab = settings.Biomes[point.biomeID].GlobalObjects[point.objectID].prefab;
                Vector3 ObjectPos = new(point.localPosition.x - settings.chunkSize*0.5f, point.height, point.localPosition.y - settings.chunkSize*0.5f);
                GameObject GlobalObject = GameObject.Instantiate(prefab, meshObject.transform);
                GlobalObject.transform.localPosition = ObjectPos;
            }
        }
    }
    void CreateLowDensityMesh(object Data)
    {
        // Low Density Mesh Generation Callback
        MeshData meshData = (MeshData)Data;
        lowDensityMesh = meshData.CreateMesh();
        meshFilter.mesh = lowDensityMesh;
        UpdateTerrainChunk();
    }
    // Chunk Callbacks
    void GenerationCallback(object Data)
    {
        // Chunk Generation Callback
        GeneratedChunk generatedChunk = (GeneratedChunk)Data;

        Texture2D Tex = TextureGenerator.TextureFromColorMap(generatedChunk.ColorMap);
        meshRenderer.materials[0].SetTexture("_ColorMap", Tex);
        meshRenderer.materials[1].SetTexture("_ColorMap", Tex);
        if (MapObject.ChunkPopulated(coord))
            MapObject.LoadObjects(coord, settings.Biomes, objectHolder.transform);
        else
            MapObject.GenerateObjects(generatedChunk, chunkData, objectHolder.transform, coord, settings.chunkSize, settings.Biomes);
        //Mesh Generation
        ThreadingHandler.RequestData(() => MeshGenerator.GenerateMesh(generatedChunk.heightMap), CreateMesh);
    }
    void CreateMesh(object Data)
    {
        // Mesh Generation Callback
        hasMesh = true;
        MeshData meshData = (MeshData)Data;
        mesh = meshData.CreateMesh();
        UpdateTerrainChunk();
    }
    public void UpdateTerrainChunk()
    {
        //checks if the chunk should be generated or if the low density version is sufficient
        //Also checks if the chunk is still visible & if it has/needs a collider
        //For performance reasons the collider is only generated if the player is nearby
        float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(MRH.viewerPosition));
        bool wasVisible = IsVisible();
        bool visible = viewerDstFromNearestEdge <= settings.maximumViewDistance;
        bool Generated = viewerDstFromNearestEdge <= settings.maximumGenerationDistance;
        bool ColliderGeneration = viewerDstFromNearestEdge <= settings.colliderGenerationDistance;
        if (hasMesh && !hasCollider && ColliderGeneration)
        {
            hasCollider = true;
            meshCollider.sharedMesh = mesh;
            GeneratedCollider?.Invoke(this);
        }
        if (wasVisible != visible)
        {
            if (visible)
                MRH.visibleChunks.Add(this);
            else
                MRH.visibleChunks.Remove(this);
        }
        if (isGenerated != Generated && chunkData != null)
        {
            if (Generated)
            {
                if (hasMesh)
                {
                    isGenerated = Generated;
                    meshFilter.mesh = mesh;
                    SetObjectsVisible(true);
                }
                else if (!MeshRequestet)
                {
                    int chunkCount = settings.mapSize / settings.chunkSize;
                    for (int chunkX = coord.x - 1; chunkX <= coord.x + 1; chunkX++)
                        for (int chunkY = coord.y - 1; chunkY <= coord.y + 1; chunkY++)
                            if (chunkX >= 0 && chunkY >= 0 && chunkX < chunkCount && chunkY < chunkCount && new Vector2Int(chunkX, chunkY) != coord)
                                if (!MRH.GeneratedChunks.Contains(new Vector2Int(chunkX, chunkY)))
                                    return;
                    // Generate the chunk
                    MeshRequestet = true;
                    ThreadingHandler.RequestData(() => MRH.globalMap.GenerateChunk(chunkData.biomeMap, coord, GlobalData.Seed, !MapObject.ChunkPopulated(coord)), GenerationCallback);
                }
            }
            else
            {
                isGenerated = Generated;
                meshFilter.mesh = lowDensityMesh;
                SetObjectsVisible(false);
            }
        }
        SetVisible(visible);
    }
    public ushort GetBiome(Vector2 localPos)
    {
        if (chunkData == null)
            return 0;
        return chunkData.biomeMap[(int)localPos.x, settings.chunkSize - (int)localPos.y];
    }
    void SetObjectsVisible(bool visible)
    {
        //Changes the visibility of the objects in the chunk
        objectHolder.SetActive(visible);
    }
    void SetVisible(bool visible)
    {
        //Changes the visibility of the GameObject
        meshObject.SetActive(visible);
    }
    bool IsVisible()
    {
        //Checks if the GameObject is active
        return meshObject.activeSelf;
    }
}