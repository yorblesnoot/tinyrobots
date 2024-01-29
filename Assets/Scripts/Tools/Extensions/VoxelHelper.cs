using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelHelper
{
    public static Vector3Int FindUnoccupiedCoordinate(this byte[,,] map)
    {
        List<Vector3Int> flatCoords = new();
        for(int x = 0; x < map.GetLength(0); x++)
        {
            for(int y = 0;  y < map.GetLength(1); y++)
            {
                for(int z = 0;  z < map.GetLength(2); z++)
                {
                    if (map[x,y,z] == 0)
                    {
                        flatCoords.Add(new Vector3Int(x,y,z)); 
                    }
                }
            }
        }
        return flatCoords[Random.Range(0, flatCoords.Count)];
    }

    public static Vector3 ToWorldVector(this Vector3Int mapVec)
    {
        return (Vector3)mapVec * ProceduralNoiseVoxelGenerator.Scale;
    }
}
