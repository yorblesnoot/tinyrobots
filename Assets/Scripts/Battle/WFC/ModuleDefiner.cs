using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ModuleDefiner : MonoBehaviour
{
    [SerializeField] bool showGuides = true;
    [SerializeField] int emptyWeight = 5;
    [SerializeField] Vector3Int dimensions = new(10, 10, 10);
    public List<Module> Modules;
    
    Dictionary<int, Module[]> moduleDefinitions;
    Bounds moduleBounds;

    readonly int orientationCount = 4;
    public static readonly int EmptyIndex = 0;
    public static readonly Vector3Int[] Directions = { Vector3Int.forward, Vector3Int.right, Vector3Int.back, Vector3Int.left, Vector3Int.up, Vector3Int.down  };
    int directionCount;
    public void DeriveModuleDefinitions()
    {
        Vector3 transformCorner = transform.position - Vector3.one / 2;
        moduleBounds = new(Vector3.Lerp(transformCorner, transformCorner + dimensions, .5f), dimensions);
        directionCount = Directions.Length;
        Dictionary<Vector3Int, ModulePrototype> prototypeMap = new();
        CreateEmptyModule();
        ModulePrototype[] prototypes = GetComponentsInChildren<ModulePrototype>();
        int moduleCount = 1;
        foreach (ModulePrototype prototype in prototypes)
        {
            prototype.Initialize();
            Vector3Int position = Vector3Int.FloorToInt(prototype.transform.position);
            prototypeMap.Add(position, prototype);
            if (!moduleDefinitions.ContainsKey(prototype.PieceIndex))
            {
                Module[] orientations = new Module[orientationCount];
                for (int i = 0; i < orientationCount; i++)
                {
                    orientations[i] = new Module()
                    {
                        ModuleIndex = moduleCount,
                        OrientationIndex = i,
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
        EstablishPrototypeConnections(prototypeMap);
        CleanFaceLists();
    }

    void CleanFaceLists()
    {
        foreach(Module mod in Modules)
        {
            foreach(var connects in mod.FaceConnections)
            {
                connects.ModuleLinks = connects.ModuleLinks.ToHashSet().ToList();
            }
        }
    }

    private void CreateEmptyModule()
    {
        moduleDefinitions = new()
        {
            { EmptyIndex, new Module[1] } //empty module
        };
        Module empty = new() { ModuleIndex = 0, Weight = emptyWeight };
        foreach (FaceConnections face in empty.FaceConnections) face.ModuleLinks.Add(EmptyIndex);
        moduleDefinitions[EmptyIndex][0] = empty;
    }

    private void EstablishPrototypeConnections(Dictionary<Vector3Int, ModulePrototype> prototypeMap)
    {
        
        foreach (var entry in prototypeMap)
        {
            ModulePrototype prototype = entry.Value;
            Module[] associatedModules = moduleDefinitions[prototype.PieceIndex];
            int prototypeOrientation = prototype.OrientationIndex;
            Module baseModule = associatedModules[prototypeOrientation];
            for (int i = 0; i < directionCount; i++)
            {
                Vector3Int position = entry.Key + Directions[i];
                List<int> faceLinks = baseModule.FaceConnections[i].ModuleLinks;
                if (!moduleBounds.Contains(position)) continue;
                if (!prototypeMap.TryGetValue(position, out ModulePrototype adjacentPrototype))
                {
                    faceLinks.Add(EmptyIndex);
                    AddConnectionToEmptyModule(baseModule.ModuleIndex, i);
                }
                else
                {
                    faceLinks.AddRange(adjacentPrototype.GetImpliedOrientations()
                        .Select(p => moduleDefinitions[adjacentPrototype.PieceIndex][p].ModuleIndex));
                    //if the face is top or bottom, apply base piece rotations to the connecting piece
                    if(i > 3) faceLinks.AddRange(prototype.GetImpliedOrientations()
                        .Select(p => moduleDefinitions[adjacentPrototype.PieceIndex][p].ModuleIndex));
                }
            }

            GenerateAlternateOrientations(prototype, associatedModules, prototypeOrientation, baseModule);
        }
    }

    private void GenerateAlternateOrientations(ModulePrototype prototype, Module[] associatedModules, int prototypeOrientation, Module baseModule)
    {
        for (int orientationModifier = 1; orientationModifier < orientationCount; orientationModifier++)
        {
            //for each other orientation, generate a module set of face connections

            int newOrientationIndex = WrapMod(prototypeOrientation);
            associatedModules[newOrientationIndex] ??= new Module()
            { Prototype = prototype, OrientationIndex = newOrientationIndex };
            Module targetModule = associatedModules[newOrientationIndex];

            //for each face of the orientation, rotate its connections by the orientation modifier

            for (int face = 0; face < 6; face++)
            {
                //copy connections from the base orientation faces to the rotated faces
                int targetFace = WrapMod(face);
                foreach (var link in baseModule.FaceConnections[face].ModuleLinks)
                {
                    int piece = Modules[link].Prototype == null ? EmptyIndex : Modules[link].Prototype.PieceIndex;
                    int finalLink;
                    int finalFace = face < 4 ? targetFace : face;
                    if (piece == EmptyIndex)
                    {
                        finalLink = EmptyIndex;
                        AddConnectionToEmptyModule(targetModule.ModuleIndex, finalFace);
                    }
                    else
                    {
                        int orientation = Modules[link].OrientationIndex;
                        int newOrientation = WrapMod(orientation);
                        finalLink = moduleDefinitions[piece][newOrientation].ModuleIndex;
                    }
                    
                    targetModule.FaceConnections[finalFace].ModuleLinks.Add(finalLink);
                }

            }

            int WrapMod(int value)
            {
                //use modulus to keep the final orientation within 4
                return (value + orientationModifier) % orientationCount;
            }
        }
    }

    void AddConnectionToEmptyModule(int moduleIndex, int face)
    {
        int newFace;
        if (face < 4) newFace = (face + 2) % 4;
        else if (face == 4) newFace = 5;
        else newFace = 4;
        moduleDefinitions[0][0].FaceConnections[newFace].ModuleLinks.Add(moduleIndex);
    }

    private void OnDrawGizmos()
    {
        if (!showGuides) return;
        DrawCubes();
        //Gizmos.DrawWireCube(moduleBounds.center, moduleBounds.size);
    }

    private void DrawCubes()
    {
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int z = 0; z < dimensions.z; z++)
                {
                    Vector3 center = new(x, y, z);
                    center += transform.position;
                    Gizmos.DrawWireCube(center, Vector3.one);
                    Handles.Label(center, $"{x}, {y}, {z}");
                }
            }
        }
    }
}
