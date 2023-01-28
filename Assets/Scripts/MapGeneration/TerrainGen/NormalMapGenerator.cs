using UnityEngine;

public static class NormalMapGenerator
{
	public static Vector3[,] CalculateNormals(float[,] heightMap) 
    {
	    Vector3[,] vertexNormals = new Vector3[heightMap.GetLength(0), heightMap.GetLength(1)];
	    for (int x = 0; x < vertexNormals.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < vertexNormals.GetLength(1) - 1; y++)
            {
                Vector3 pointA = new(x, heightMap[x, y], y);
                Vector3 pointB = new(x, heightMap[x, y + 1], y + 1);
                Vector3 pointC = new(x + 1, heightMap[x + 1, y], y);
		        vertexNormals[x, y] = SurfaceNormalFromVertex(pointA, pointB, pointC);
                vertexNormals[x, y].z *= -1f;
            }
        }
        return vertexNormals;
	}

    static Vector3 SurfaceNormalFromVertex(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
		Vector3 sideAB = pointB - pointA;
		Vector3 sideAC = pointC - pointA;
		return Vector3.Cross(sideAB, sideAC).normalized;
	}
}
