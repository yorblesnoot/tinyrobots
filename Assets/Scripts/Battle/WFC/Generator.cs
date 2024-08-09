using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField] ModuleDefiner definer;
    [SerializeField] Vector3Int dimensions;

    Dictionary<Vector3Int, GenerationSlot> GenerationSpace;

    readonly float centerOffset = .5f;

    public static Module[] Modules;
    int directionCount;

    private void Awake()
    {
        BuildGenerationSpace();
        Generate();
    }

    private void Generate()
    {
        while(GenerationSpace.Count > 0)
        {
            GenerationSlot lowestEntropy = GenerationSpace.Values.OrderBy(slot => slot.Entropy).FirstOrDefault();
            Collapse(lowestEntropy);
        }
    }
    void Collapse(GenerationSlot slot)
    {
        slot.CollapseDomain();
        GenerationSpace.Remove(slot.VoxelPosition);
        PropagateConstraints(slot);
    }


    private void PropagateConstraints(GenerationSlot baseSlot)
    {
        for(int i = 0; i < directionCount; i++)
        {
            Vector3Int position = baseSlot.VoxelPosition + ModuleDefiner.Directions[i];
            if (!GenerationSpace.TryGetValue(position, out GenerationSlot adjacentSlot)) continue;
            int initialCount = adjacentSlot.ModuleDomain.Count;

            HashSet<Module> newDomain = new();
            foreach(Module module in adjacentSlot.ModuleDomain)
            {
                if(baseSlot.ModuleDomain.Where(mod => mod.FaceConnections[i].ModuleLinks.Contains(module.ModuleIndex)).Count() == 0) continue;
                newDomain.Add(module);
            }
            if (newDomain.Count < initialCount)
            {
                adjacentSlot.ModuleDomain = newDomain;
                adjacentSlot.CalculateEntropy();
                PropagateConstraints(adjacentSlot);
            }
        }
    }

    private void BuildGenerationSpace()
    {
        directionCount = ModuleDefiner.Directions.Length;
        GenerationSpace = new();
        HashSet<int> mainDomain = definer.Modules.Select(module => module.ModuleIndex).ToHashSet();
        Modules = definer.Modules.ToArray();
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    Vector3 center = new(x + centerOffset, y + centerOffset, z + centerOffset);
                    center += transform.position;
                    Vector3Int vox = new(x, y, z);
                    GenerationSlot slot = new() { VoxelPosition = vox, WorldPosition = center, ModuleDomain = Modules.ToHashSet() };
                    slot.CalculateEntropy();
                    GenerationSpace.Add(vox, slot);
                }
            }
        }
    }
}
