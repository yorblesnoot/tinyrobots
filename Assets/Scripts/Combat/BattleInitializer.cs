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

    [SerializeField] TreeRenderer treeRenderer;

    [SerializeField] bool testing = true;
    private void Start()
    {
        mapGenerators[generatorIndex].GenerateCoreMap();
        mapGenerators[generatorIndex].PlaceSecondaries();


        if (testing) return;
        byte[,,] mapGrid = mapGenerators[generatorIndex].GetByteMap();
        marchingCubesRenderer.RenderIntoCubes(mapGrid);
        

        Pathfinder3D.Initialize(mapGrid);
        //Pathfinder3D.lineRenderer = lineRenderer;

        botPlacer.PlaceBots();
    }
}
