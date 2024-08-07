using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ModuleDefiner : MonoBehaviour
{
    [SerializeField] Vector3Int dimensions = new(10, 10, 10);
    public List<Module> Modules;
    readonly float centerOffset = .5f;
    readonly int orientationCount = 4;
    Dictionary<int, Module[]> moduleDefinitions;
    Dictionary<Vector3Int, ModulePrototype> prototypeMap;

    readonly Vector3Int[] directions = { Vector3Int.forward, Vector3Int.right, Vector3Int.back, Vector3Int.left, Vector3Int.up, Vector3Int.down  };
    int directionCount;
    public void DeriveModuleDefinitions()
    {
        directionCount = directions.Length;
        prototypeMap = new();
        moduleDefinitions = new()
        {
            { 0, new Module[1] } //empty module
        };
        ModulePrototype[] prototypes = GetComponentsInChildren<ModulePrototype>();
        foreach (ModulePrototype prototype in prototypes)
        {
            prototype.Initialize();
            Vector3Int position = Vector3Int.FloorToInt(prototype.transform.position);
            prototypeMap.Add(position, prototype);
            if (!moduleDefinitions.ContainsKey(prototype.PieceIndex)) moduleDefinitions.Add(prototype.PieceIndex, new Module[4]);
        }
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
            associatedModules[prototypeOrientation] ??= new Module() { Prototype = prototype, GridPosition = entry.Key, PieceIndex = prototype.PieceIndex };
            Module baseModule = associatedModules[prototypeOrientation];
            for (int i = 0; i < directionCount; i++)
            {
                Vector3Int position = entry.Key + directions[i];
                if (!prototypeMap.TryGetValue(position, out ModulePrototype adjacentPrototype)) baseModule.FaceConnections[i].Add(new Connection() { pieceId = 0, orientation = 0 });
                else baseModule.FaceConnections[i].UnionWith(adjacentPrototype.GetImpliedOrientations().Select(p => new Connection() { pieceId = adjacentPrototype.PieceIndex, orientation = p }));
            }

            for (int orientationModifier = 1; orientationModifier < orientationCount; orientationModifier++)
            {
                //for each other orientation, generate a module set of face connections
                

                //use modulus to keep the final orientation within 4
                int newOrientationIndex = WrapMod(prototypeOrientation);
                associatedModules[newOrientationIndex] = new() { GridPosition = baseModule.GridPosition, PieceIndex = prototype.PieceIndex, Prototype = prototype };

                //for each face of the orientation, rotate its connections by the orientation modifier

                for (int face = 0; face < 4; face++)
                {
                    int targetFace = WrapMod(face);
                    //copy connections from the base orientation faces to the rotated faces
                    associatedModules[newOrientationIndex].FaceConnections[targetFace]
                        .UnionWith(baseModule.FaceConnections[face]
                        .Select(con => new Connection() { pieceId = con.pieceId, orientation = WrapMod(con.orientation) }));
                }

                int WrapMod(int value)
                {
                    return (value + orientationModifier) % orientationCount;
                }
            }
        }
        Modules = new();
        foreach(var pair in moduleDefinitions)
        {
            Modules.AddRange(pair.Value);
        }
    }

    private void DebugConnections()
    {
        foreach (var module in Modules)
        {
            Debug.Log(module.PieceIndex + " Connections");
            foreach (var connect in module.FaceConnections)
            {
                connect.DebugContents();
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
