using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInitializer : MonoBehaviour
{
    [SerializeField] ProceduralMapGenerator mapGenerator;
    [SerializeField] MarchingCubes marchingCubesRenderer;
    [SerializeField] LineRenderer lineRenderer;

    private void Start()
    {
        byte[,,] mapGrid = mapGenerator.Generate();
        marchingCubesRenderer.RenderIntoCubes(mapGrid);
        Pathfinder3D.Initialize(mapGrid);
        Pathfinder3D.lineRenderer = lineRenderer;
    }
}
