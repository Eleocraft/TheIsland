using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateMesh(float[,] heightMap, int Simplification = 1)
    {
        //here the mesh is generated from a height map
        //this way of generating a mesh is really simple: it just loops through all heights and assigns a vertex, 6 triangles and uv-coordinates
        int ChunkSize = heightMap.GetLength(0);

        float topLeftX = (ChunkSize-1) * Simplification / -2f;
        float topLeftZ = (ChunkSize-1) * Simplification / 2f;
        MeshData meshData = new(ChunkSize, ChunkSize);
        int vertexIndex = 0;
        int triangleIndex = 0;
        for (int y = 0; y < ChunkSize; y++)
        {
            for (int x = 0; x < ChunkSize; x++)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x * Simplification, heightMap[x,y], topLeftZ - y * Simplification);
                meshData.uvs[vertexIndex] = new Vector2((float)x / (ChunkSize-1), (float)y / (ChunkSize-1));
                if (x < ChunkSize - 1 && y < ChunkSize - 1)
                {
                    // First Triangle
                    meshData.triangles[triangleIndex] = vertexIndex;
                    meshData.triangles[triangleIndex + 1] = vertexIndex + ChunkSize + 1;
                    meshData.triangles[triangleIndex + 2] = vertexIndex + ChunkSize;
                    // Second Triangle
                    meshData.triangles[triangleIndex + 3] = vertexIndex + ChunkSize + 1;
                    meshData.triangles[triangleIndex + 4] = vertexIndex;
                    meshData.triangles[triangleIndex + 5] = vertexIndex + 1;
                    triangleIndex += 6;
                }
                vertexIndex++;
            }
        }
        return meshData;
    }

}
public class MeshData // this class holds all informations necessary to generate a unity mesh
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }
    public Mesh CreateMesh()
    {
        // this function converts the Mesh Data into an actual mesh that can be used by unity
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        return mesh;
    }
}