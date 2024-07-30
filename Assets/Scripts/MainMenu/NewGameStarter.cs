using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGameStarter : MonoBehaviour
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
        sceneRelay.generateNavMap = true;
        playerData.PartInventory = new();
        playerData.Difficulty = startDifficulty;
        loader.Load(SceneType.NAVIGATION);
    }
}
