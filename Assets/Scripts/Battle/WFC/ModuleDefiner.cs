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
    Dictionary<int, Module> moduleDefinitions;
    Dictionary<Vector3Int, ModulePrototype> prototypeMap;

    readonly Vector3Int[] directions = { Vector3Int.up, Vector3Int.left, Vector3Int.forward, Vector3Int.right, Vector3Int.back, Vector3Int.down  };
    int directionCount;
    public void DeriveModuleDefinitions()
    {
        directionCount = directions.Length;
        prototypeMap = new();
        moduleDefinitions = new()
        {
            { 0, new() } //empty module
        };
        ModulePrototype[] prototypes = GetComponentsInChildren<ModulePrototype>();
        foreach (ModulePrototype prototype in prototypes)
        {
            prototype.Initialize();
            Vector3Int position = Vector3Int.FloorToInt(prototype.transform.position);
            prototypeMap.Add(position, prototype);

        }
        foreach (var entry in prototypeMap)
        {
            ModulePrototype prototype = entry.Value;
            if (!moduleDefinitions.ContainsKey(prototype.PieceIndex))
                moduleDefinitions.Add(prototype.PieceIndex, new Module() { Prototype = prototype, GridPosition = entry.Key, PieceIndex = prototype.PieceIndex });
            Module module = moduleDefinitions[prototype.PieceIndex];
            for (int i = 0; i < directionCount; i++)
            {
                Vector3Int position = entry.Key + directions[i];
                if (!prototypeMap.TryGetValue(position, out ModulePrototype value)) module.FaceConnections[i].Add(new() { pieceId = 0, orientation = 0 });
                else module.FaceConnections[i].UnionWith(value.GetOrientations().Select(p => new Connection() { pieceId = value.PieceIndex, orientation = p }));
            }

        }
        Modules = moduleDefinitions.Values.ToList();
        DebugConnections();
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
