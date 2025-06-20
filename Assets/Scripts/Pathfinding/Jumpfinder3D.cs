using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumpfinder3D 
{
    //steps for jump pathfinding

    //1. find the curvature of voxels in each flood zone. curvature above a certain threshold indicates a jump point


    /*public static float CalculateWalkabilityBasedCurvature(Vector3Int position, Dictionary<Vector3Int, Node> voxelMap)
    {
        if (!voxelMap.TryGetValue(position, out Node centerNode) || !IsWalkable(centerNode))
            return 0f;

        // Check 8-directional neighbors in horizontal plane
        var directions = new Vector3Int[]
        {
            new Vector3Int(-1, 0, -1), new Vector3Int(0, 0, -1), new Vector3Int(1, 0, -1),
            new Vector3Int(-1, 0, 0),                            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 1),  new Vector3Int(0, 0, 1),  new Vector3Int(1, 0, 1)
        };

        bool[] walkable = new bool[8];
        for (int i = 0; i < 8; i++)
        {
            var neighborPos = position + directions[i];
            walkable[i] = voxelMap.TryGetValue(neighborPos, out Node node) && IsWalkable(node);
        }

        // Count transitions between walkable and non-walkable
        int transitions = 0;
        for (int i = 0; i < 8; i++)
        {
            if (walkable[i] != walkable[(i + 1) % 8])
                transitions++;
        }

        // More transitions = higher curvature (corners, complex edges)
        return transitions / 8f;
    }*/

    //2. test each jump point against nearby jump points using physics. if a jump point connects to another jump point,
    // connect the parent flood zones and record the point bridge

    //3. figure out what flood zone the target is in, then pathfind to its floodzone

    //4. find the first bridge point along the flood path, then jump to its partner on the next zone
}
