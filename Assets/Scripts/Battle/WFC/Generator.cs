using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] ModuleDefiner definer;
    [SerializeField] Vector3Int dimensions;

    GenerationSlot[,,] generationSpace;

    readonly float centerOffset = .5f;

    private void Awake()
    {
        generationSpace = new GenerationSlot[dimensions.x, dimensions.y, dimensions.z];
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    Vector3 center = new(x + centerOffset, y + centerOffset, z + centerOffset);
                    center += transform.position;
                    generationSpace[x,y,z] = new() { WorldPosition = center, ModuleDomain = definer.Modules.ToHashSet() };
                }
            }
        }
    }
}
