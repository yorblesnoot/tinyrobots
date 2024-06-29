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

    public void NewGame()
    {
        botConverter.Initialize();
        playerData.CoreInventory = new();
        foreach (BotCore core in starterCores)
        {
            core.Initialize(botConverter);
            playerData.CoreInventory.Add(core);
        }
        sceneRelay.generateNavMap = true;
        playerData.PartInventory = new();
        loader.Load(SceneType.NAVIGATION);
    }
}
