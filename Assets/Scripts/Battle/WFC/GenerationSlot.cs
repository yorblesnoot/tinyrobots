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
    public static GameObject Error;

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

    public bool CollapseDomain()
    {
        Module module;
        if (ModuleDomain.Count == 0)
        {
            Error.SetActive(true);
            Error.transform.position = WorldPosition;
            Debug.LogError("Tried to collapse empty domain.");
            return false;
        }
        else module = (Module)ModuleDomain.RandomByWeight();
        //Debug.LogWarning(VoxelPosition + " collapsed " + ModuleDomain.Count  + " modules to module index " + module.ModuleIndex);
        ModuleDomain = new() { module };
        
        Quaternion rotation = Quaternion.Euler(0, module.OrientationIndex * 90, 0);
        if(module.Prototype != null) WaveFunctionGenerator.Generated.Add(Object.Instantiate(module.Prototype, WorldPosition, rotation).gameObject);
        return true;
    }
}
