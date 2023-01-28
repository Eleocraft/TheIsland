using System.Collections.Generic;
using UnityEngine;

public static class ObjectPlacer
{
	public const int maxRadius = 30;
	public const int minRadius = 3;
	const int realSeachRange = maxRadius;
	const int numSamplesBeforeRejection = 20;

    public static ObjectSpawnPoints GeneratePoints(int chunkSize, int seed, Vector3[,] normalMap, float[,] heightMap, ushort[,] biomeMap, BiomeData[] biomeData, Vector2Int chunkCoord, SerializableList<GlobalObjectPoint>[,] globalObjects, PoissonDiscPoint[,] grid)
    {
        float cellSize = minRadius / Mathf.Sqrt(2);
		float celledSeachRange = realSeachRange / cellSize;
		int chunkCount = globalObjects.Length - 1;
        List<Vector2> points = new();
		List<Quaternion> rotations = new();
		List<int> objectIDs = new();
		Vector2 position = chunkCoord * chunkSize;
		Vector2 cellPos = position / cellSize;
		
		List<PoissonDiscPoint> spawnPoints = new();
		
        System.Random prng = new(seed);
		
		// Set Initial spawn points
		Vector2 centerInitPos = Vector2.one * chunkSize/2;
        spawnPoints.Add(new PoissonDiscPoint(centerInitPos, prng.Next(minRadius, maxRadius)));

		// Main Algorithm
        while (spawnPoints.Count > 0)
		{
			// choosing a spawnpoint from the list
			int spawnIndex = prng.Next(0,spawnPoints.Count);
			Vector2 spawnCenter = spawnPoints[spawnIndex].pos;
			float spawnRadius = spawnPoints[spawnIndex].rad;
			bool candidateAccepted = false;
			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				// Determining the potential poisson disc possition
				float angle = (float)prng.NextDouble() * Mathf.PI * 2;
				Vector2 dir = new(Mathf.Sin(angle), Mathf.Cos(angle));
				Vector2 candidate = spawnCenter + dir * prng.Next((int)spawnRadius, Mathf.Max(2*(int)spawnRadius, maxRadius));
				if (IsInRegion(candidate, chunkSize))
				{
					// Finding the correct Biome
					int biome = biomeMap[(int)candidate.x, chunkSize - (int)candidate.y];
					if (biomeData[biome].Objects.Count == 0)
						continue;
					// Finding the correct Object
					float[] Weights = new float[biomeData[biome].Objects.Count];

					for (int t = 0; t < Weights.Length; t++)
						Weights[t] = biomeData[biome].Objects[t].objectSpawnData.frequency;
					
					int objectID = Utility.WeightedSystemRandom(Weights, prng);
					float radius = biomeData[biome].Objects[objectID].objectSpawnData.radius;
					// Checking if the Objectplacement is valid
					if (IsValid(candidate, radius, objectID, biomeData[biome]))
					{
						objectIDs.Add(objectID);
						points.Add(candidate);
						spawnPoints.Add(new PoissonDiscPoint(candidate, radius));
						grid[(int)(candidate.x / cellSize + cellPos.x),(int)(candidate.y / cellSize + cellPos.y)] = new(candidate + position, radius);
						candidateAccepted = true;
						Quaternion pointRotation = Quaternion.AngleAxis(prng.Next(0, 360), biomeData[biome].Objects[objectID].objectSpawnData.rotateWithTerrain ? normalMap[(int)candidate.x, chunkSize - (int)candidate.y] : Vector3.up);
						if (biomeData[biome].Objects[objectID].objectSpawnData.rotateWithTerrain)
							pointRotation *= Quaternion.FromToRotation(Vector3.up, normalMap[(int)candidate.x, chunkSize - (int)candidate.y]);
						
						rotations.Add(pointRotation);
						break;
					}
				}
			}
			if (!candidateAccepted)
				spawnPoints.RemoveAt(spawnIndex);
		}
		return new ObjectSpawnPoints(points, rotations, objectIDs);

		bool IsInRegion(Vector2 candidate, int sampleRegionSize)
		{
			if (candidate.x >=0 && candidate.x < sampleRegionSize && candidate.y >= 0 && candidate.y < sampleRegionSize)
				return true;

			return false;
		}

		bool IsValid(Vector2 candidate, float cRadius, int objectID, BiomeData biomeData)
		{
			if (Utility.CalcualteSteepnessRadientFromNormal(normalMap[(int)candidate.x, chunkSize - (int)candidate.y]) < biomeData.Objects[objectID].objectSpawnData.GeneraionSteepnessRadiant && heightMap[(int)candidate.x, chunkSize - (int)candidate.y] > biomeData.Objects[objectID].objectSpawnData.minHeight && heightMap[(int)candidate.x, chunkSize - (int)candidate.y] < biomeData.Objects[objectID].objectSpawnData.maxHeight)
			{
				//Check for colliding global Objects
				for (int x = -1; x <= 1; x++)
					for (int y = -1; y <= 1; y++)
						if (chunkCoord.x + x > 0 && chunkCoord.x + x < chunkCount && chunkCoord.y + y > 0 && chunkCoord.y + y < chunkCount)
							foreach (GlobalObjectPoint point in globalObjects[x+chunkCoord.x,y+chunkCoord.y].list)
								if ((point.localPosition + new Vector2(x,y) * chunkSize - candidate).sqrMagnitude < point.sqrtBlockRadius)
									return false;
				
				// Check for other points in proximity
				int cellX = (int)(candidate.x / cellSize);
				int cellY = (int)(candidate.y / cellSize);
				int searchStartX = (int)(cellX - celledSeachRange);
				int searchEndX = (int)(cellX + celledSeachRange);
				int searchStartY = (int)(cellY - celledSeachRange);
				int searchEndY = (int)(cellY + celledSeachRange);
				for (int x = searchStartX; x <= searchEndX; x++) {
					for (int y = searchStartY; y <= searchEndY; y++) {
						if (x+cellPos.x < 0 || x+cellPos.x >= grid.GetLength(0) || y+cellPos.y < 0 || y+cellPos.y >= grid.GetLength(1))
							continue;
						PoissonDiscPoint point = grid[(int)(x+cellPos.x),(int)(y+cellPos.y)];
						if (!point.Initialized)
							continue;
						float sqrDst = (candidate + position - point.pos).sqrMagnitude;
						float radius = Mathf.Max(cRadius, point.rad);
						if (sqrDst < radius*radius)
							return false;
					}
				}
				return true;
			}
			return false;
		}
	}
}
[System.Serializable]
public struct PoissonDiscPoint
{
	public Vector2 pos;
	public float rad;
	public bool Initialized => rad != 0f;
	public PoissonDiscPoint(Vector2 pos, float rad)
	{
		this.pos = pos;
		this.rad = rad;
	}
}
[System.Serializable]
public struct ObjectSpawnPoints
{
	public List<Vector2> positions;
	public List<Quaternion> rotations;
	public List<int> objectIDs;
	public ObjectSpawnPoints(List<Vector2> positions, List<Quaternion> rotations, List<int> objectIDs)
	{
		this.positions = positions;
		this.rotations = rotations;
		this.objectIDs = objectIDs;
	}
}