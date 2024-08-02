using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    [SerializeField] SceneRelay sceneRelay;
    [SerializeField] BotConverter botConverter;
    [SerializeField] SceneLoader loader;
    [SerializeField] List<BotCore> starterCores;
    [SerializeField] int startDifficulty = 6;

    public void NewGame()
    {
        botConverter.Initialize();
        playerData.CoreInventory = new(starterCores);
        foreach (var core in playerData.CoreInventory) core.HealthRatio = 1;
        sceneRelay.generateNavMap = true;
        playerData.PartInventory = new();
        playerData.Difficulty = startDifficulty;
        loader.Load(SceneType.NAVIGATION);
    }

    public void LoadGame()
    {
        SaveContainer container = new(playerData);
        container.LoadPlayerData();
        sceneRelay.generateNavMap = false;
        loader.Load(SceneType.NAVIGATION);
    }
}
