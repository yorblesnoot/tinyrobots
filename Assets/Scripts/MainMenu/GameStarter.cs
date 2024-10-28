using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStarter : MonoBehaviour
{
    [SerializeField] Button loadButton;
    [SerializeField] BotConverter botConverter;
    [SerializeField] List<BotCharacter> starterCores;
    [SerializeField] int startDifficulty = 6;

    SaveContainer container;
    private void Start()
    {
        container = new(SceneGlobals.PlayerData);
        loadButton.onClick.AddListener(LoadGame);
        loadButton.gameObject.SetActive(container.SaveExists());
    }

    public void NewGame()
    {
        botConverter.Initialize();
        SceneGlobals.PlayerData.CoreInventory = new(starterCores);
        foreach (var core in SceneGlobals.PlayerData.CoreInventory)
        {
            core.HealthRatio = 1;
            core.Bot = SceneGlobals.PlayerData.BotConverter.StringToBot(core.StarterRecord.Record);
        }
        SceneGlobals.SceneRelay.GenerateNavMap = true;
        SceneGlobals.PlayerData.PartInventory = new();
        SceneGlobals.PlayerData.Difficulty = startDifficulty;
        SceneGlobals.PlayerData.MapData = new();
        SceneLoader.Load(SceneType.NAVIGATION);
    }

    public void LoadGame()
    {
        container.LoadPlayerData();
        SceneGlobals.SceneRelay.GenerateNavMap = false;
        SceneLoader.Load(SceneType.NAVIGATION);
    }
}
