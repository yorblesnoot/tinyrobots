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
        playerData.coreInventory = new();
        foreach (BotCore core in starterCores)
        {
            core.Initialize(botConverter);
            playerData.coreInventory.Add(core);
        }
        sceneRelay.generateNavMap = true;
        loader.Load(SceneType.NAVIGATION);
    }
}
