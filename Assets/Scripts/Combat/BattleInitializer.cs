using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInitializer : MonoBehaviour
{
    [SerializeField] MapGenerator[] mapGenerators;
    [SerializeField] int generatorIndex;

    [SerializeField] MarchingCubes marchingCubesRenderer;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] BotPlacer botPlacer;

    private void Start()
    {
        byte[,,] mapGrid = mapGenerators[generatorIndex].GenerateCoreMap();
        marchingCubesRenderer.RenderIntoCubes(mapGrid);
        mapGenerators[generatorIndex].PlaceSecondaries();

        Pathfinder3D.Initialize(mapGrid);
        //Pathfinder3D.lineRenderer = lineRenderer;

        botPlacer.PlaceBots();
    }
}
