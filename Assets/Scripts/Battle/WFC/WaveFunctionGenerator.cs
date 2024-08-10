using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WaveFunctionGenerator : MonoBehaviour
{
    [SerializeField] ModuleDefiner definer;
    [SerializeField] Vector3Int dimensions;
    [SerializeField] GameObject errorModule;

    Dictionary<Vector3Int, GenerationSlot> GenerationSpace;

    readonly float centerOffset = .5f;

    public static Module[] Modules;
    public static List<GameObject> Generated = new();
    int directionCount;

    void Regenerate()
    {
        DestroyChildren();
        BuildGenerationSpace();
        Generate();
    }

    private void DestroyChildren()
    {
        foreach(var piece in Generated)
        {
            Destroy(piece);
        }
        Generated.Clear();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.G)) Regenerate();
    }

    private void BuildGenerationSpace()
    {
        GenerationSlot.Error = errorModule;
        errorModule.SetActive(false);
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

    private void Generate()
    {
        while(GenerationSpace.Count > 0)
        {
            GenerationSlot lowestEntropy = GenerationSpace.Values.OrderBy(slot => slot.Entropy).FirstOrDefault();
            if(!Collapse(lowestEntropy)) return;
        }
    }
    bool Collapse(GenerationSlot slot)
    {
        bool success = slot.CollapseDomain();
        GenerationSpace.Remove(slot.VoxelPosition);
        PropagateConstraints(slot);
        return success;
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
                if(ModuleIsCompatible(module, baseSlot, i)) newDomain.Add(module);
            }
            /*Debug.Log("domains for direction " + i + " at "  + baseSlot.VoxelPosition);
            baseSlot.ModuleDomain.DebugContents();
            adjacentSlot.ModuleDomain.DebugContents();
            newDomain.DebugContents();*/
            if (newDomain.Count < initialCount)
            {
                adjacentSlot.ModuleDomain = newDomain;
                adjacentSlot.CalculateEntropy();
                PropagateConstraints(adjacentSlot);
            }
        }
    }

    bool ModuleIsCompatible(Module testModule, GenerationSlot baseSlot, int faceIndex)
    {
        foreach(Module baseDomainModule in baseSlot.ModuleDomain)
        {
            HashSet<int> face = baseDomainModule.FaceConnections[faceIndex].ModuleLinks.ToHashSet();
            if (face.Contains(testModule.ModuleIndex)) return true;
        }
        return false;
    }

    
}
