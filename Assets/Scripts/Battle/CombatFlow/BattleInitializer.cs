using UnityEngine;

public class BattleInitializer : MonoBehaviour
{
    enum MapMode
    {
        PERLIN_MARCH,
        VOXELIZE_PREMADE
    }
    [SerializeField] SerializableDictionary<MissionType, Mission> missions;
    [SerializeField] MapMode mapMode;

    [SerializeField] SceneRelay relay;

    [SerializeField] NoiseVoxelGenerator noiseGenerator;
    [SerializeField] MarchingCubes marchingCubesRenderer;

    [SerializeField] MainCameraControl mainCameraControl;
    [SerializeField] TurnManager turnManager;
    private void Start()
    {
        byte[,,] mapGrid = GenerateMap();
        Pathfinder3D.Initialize(mapGrid);
        mainCameraControl.Initialize(mapGrid);
        missions[relay.MissionType].BeginMission();
    }

    private byte[,,] GenerateMap()
    {
        byte[,,] mapGrid;
        if (mapMode == MapMode.PERLIN_MARCH)
        {
            noiseGenerator.GenerateCoreMap();
            mapGrid = noiseGenerator.GetByteMap();
            marchingCubesRenderer.RenderIntoCubes(mapGrid);
        }
        else
        {
            GameObject map = Instantiate(relay.battleMap);
            MapScanner voxelizer = new();
            mapGrid = voxelizer.GetVoxelGrid(map);
        }

        return mapGrid;
    }
}
