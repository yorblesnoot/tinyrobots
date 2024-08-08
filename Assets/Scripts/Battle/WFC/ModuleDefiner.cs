using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ModuleDefiner : MonoBehaviour
{
    [SerializeField] int emptyWeight = 5;
    [SerializeField] Vector3Int dimensions = new(10, 10, 10);
    public List<Module> Modules;
    readonly float centerOffset = .5f;
    readonly int orientationCount = 4;
    Dictionary<int, Module[]> moduleDefinitions;
    Dictionary<Vector3Int, ModulePrototype> prototypeMap;

    public static readonly Vector3Int[] Directions = { Vector3Int.forward, Vector3Int.right, Vector3Int.back, Vector3Int.left, Vector3Int.up, Vector3Int.down  };
    int directionCount;
    public void DeriveModuleDefinitions()
    {
        directionCount = Directions.Length;
        prototypeMap = new();
        moduleDefinitions = new()
        {
            { 0, new Module[1] } //empty module
        };
        moduleDefinitions[0][0] = new Module() { ModuleIndex = 0, Weight = emptyWeight };
        ModulePrototype[] prototypes = GetComponentsInChildren<ModulePrototype>();
        int moduleCount = 1;
        foreach (ModulePrototype prototype in prototypes)
        {
            prototype.Initialize();
            Vector3Int position = Vector3Int.FloorToInt(prototype.transform.position);
            prototypeMap.Add(position, prototype);
            if (!moduleDefinitions.ContainsKey(prototype.PieceIndex))
            {
                Module[] orientations = new Module[4];
                for (int i = 0; i < 4; i++)
                {
                    orientations[i] = new Module()
                    {
                        GridPosition = position,
                        ModuleIndex = moduleCount,
                        OrientationIndex = i,
                        PieceIndex = prototype.PieceIndex,
                        Prototype = prototype,
                        Weight = prototype.BaseWeight
                    };
                    moduleCount++;
                }
                moduleDefinitions.Add(prototype.PieceIndex, orientations);
            }
            else
            {
                foreach (var module in moduleDefinitions[prototype.PieceIndex]) module.Weight += prototype.BaseWeight;
            }
        }
        Modules = new();
        foreach (var pair in moduleDefinitions) Modules.AddRange(pair.Value);
        EstablishPrototypeConnections();
        //Modules = moduleDefinitions.Values.ToList();
        DebugConnections();
    }

    private void EstablishPrototypeConnections()
    {
        
        foreach (var entry in prototypeMap)
        {
            ModulePrototype prototype = entry.Value;
            Module[] associatedModules = moduleDefinitions[prototype.PieceIndex];
            int prototypeOrientation = prototype.OrientationIndex;
            Module baseModule = associatedModules[prototypeOrientation];
            baseModule.FaceConnections.DebugContents();
            for (int i = 0; i < directionCount; i++)
            {
                Vector3Int position = entry.Key + Directions[i];
                if (!prototypeMap.TryGetValue(position, out ModulePrototype adjacentPrototype)) baseModule.FaceConnections[i].ModuleLinks.Add(0);
                else baseModule.FaceConnections[i].ModuleLinks.AddRange(adjacentPrototype.GetImpliedOrientations().Select(p => moduleDefinitions[adjacentPrototype.PieceIndex][p].ModuleIndex));
            }

            for (int orientationModifier = 1; orientationModifier < orientationCount; orientationModifier++)
            {
                //for each other orientation, generate a module set of face connections
                
                int newOrientationIndex = WrapMod(prototypeOrientation);
                associatedModules[newOrientationIndex] ??= new Module() 
                { GridPosition = baseModule.GridPosition, PieceIndex = prototype.PieceIndex, Prototype = prototype, OrientationIndex = newOrientationIndex };
                Module targetModule = associatedModules[newOrientationIndex];

                //for each face of the orientation, rotate its connections by the orientation modifier

                for (int face = 0; face < 6; face++)
                {
                    //copy connections from the base orientation faces to the rotated faces
                    int targetFace = WrapMod(face);
                    foreach(var link in baseModule.FaceConnections[face].ModuleLinks)
                    {
                        int piece = Modules[link].PieceIndex;
                        int orientation = Modules[link].OrientationIndex;
                        int newOrientation = WrapMod(orientation);
                        int finalLink = piece == 0 ? 0 : moduleDefinitions[piece][newOrientation].ModuleIndex;
                        targetModule.FaceConnections[face < 4 ? targetFace: face].ModuleLinks.Add(finalLink);
                    }
                    
                }

                int WrapMod(int value)
                {
                    //use modulus to keep the final orientation within 4
                    return (value + orientationModifier) % orientationCount;
                }
            }
        }
        
    }

    private void DebugConnections()
    {
        foreach (var module in Modules)
        {
            Debug.Log(module.PieceIndex + " Connections");
            foreach (var connect in module.FaceConnections)
            {
                connect.ModuleLinks.DebugContents();
            }
        }
    }

    private void OnDrawGizmos()
    {
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    Vector3 center = new(x + centerOffset, y + centerOffset, z + centerOffset);
                    center += transform.position;
                    Gizmos.DrawWireCube(center, Vector3.one);
                    Handles.Label(center, $"{x}, {y}, {z}");
                }
            }
        }
    }
}
