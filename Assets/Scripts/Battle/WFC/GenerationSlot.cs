using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationSlot
{
    public int CollapsedPieceIndex;
    public Vector3Int VoxelPosition;
    public Vector3 WorldPosition;
    public float Entropy { get; private set; }
    public HashSet<Module> ModuleDomain;

    public void CalculateEntropy()
    {
        float sum, logsum;
        sum = logsum = 0;
        foreach(Module mod in ModuleDomain)
        {
            sum += mod.Weight;
            logsum += mod.Weight * Mathf.Log(mod.Weight);
        }
        Entropy = Mathf.Log(sum) - (Mathf.Log(logsum) / sum);
    }

    public void SetModule(Module module)
    {
        ModuleDomain = new() { module };
    }

    
}
