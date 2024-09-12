using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [SerializeField] BotConverter botConverter;
    [SerializeField] SceneLoader loader;
    [SerializeField] List<BotCore> starterCores;
    [SerializeField] int startDifficulty = 6;

    public void NewGame()
    {
        botConverter.Initialize();
        SceneGlobals.PlayerData.CoreInventory = new(starterCores);
        foreach (var core in SceneGlobals.PlayerData.CoreInventory) core.HealthRatio = 1;
        SceneGlobals.SceneRelay.generateNavMap = true;
        SceneGlobals.PlayerData.PartInventory = new();
        SceneGlobals.PlayerData.Difficulty = startDifficulty;
        loader.Load(SceneType.NAVIGATION);
    }

    public void LoadGame()
    {
        SaveContainer container = new(SceneGlobals.PlayerData);
        container.LoadPlayerData();
        SceneGlobals.SceneRelay.generateNavMap = false;
        loader.Load(SceneType.NAVIGATION);
    }
}
