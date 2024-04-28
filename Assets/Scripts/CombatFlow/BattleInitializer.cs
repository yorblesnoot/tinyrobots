using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInitializer : MonoBehaviour
{
    enum MapMode
    {
        PERLIN_MARCH,
        VOXELIZE_PREMADE
    }
    [SerializeField] MapMode mapMode;

    [SerializeField] PhysicsVoxelizer voxelizer;

    [SerializeField] NoiseVoxelGenerator noiseGenerator;
    [SerializeField] MarchingCubes marchingCubesRenderer;

    [SerializeField] BotPlacer botPlacer;
    [SerializeField] MainCameraControl mainCameraControl;
    [SerializeField] TurnManager turnManager;
    private void Start()
    {
        byte[,,] mapGrid;
        if(mapMode == MapMode.PERLIN_MARCH)
        {
            noiseGenerator.GenerateCoreMap();
            mapGrid = noiseGenerator.GetByteMap();
            marchingCubesRenderer.RenderIntoCubes(mapGrid);
        }
        else
        {
            mapGrid = voxelizer.GetVoxelGrid();
        }
        
        Pathfinder3D.Initialize(mapGrid);
        mainCameraControl.Initialize(mapGrid);

        botPlacer.PlaceBots();
        turnManager.BeginTurnSequence();
        
    }
}
