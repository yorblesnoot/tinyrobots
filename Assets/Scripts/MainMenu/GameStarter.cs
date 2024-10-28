using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStarter : MonoBehaviour
{
    [SerializeField] Button loadButton;
    [SerializeField] BotConverter botConverter;
    [SerializeField] List<BotCharacter> starterCores;
    [SerializeField] GameObject characterSelect;
    SaveContainer container;
    private void Start()
    {
        container = new(SceneGlobals.PlayerData);
        loadButton.onClick.AddListener(LoadGame);
        loadButton.gameObject.SetActive(container.SaveExists());
    }

    public void NewGame()
    {
        characterSelect.SetActive(true);
    }

    public void LoadGame()
    {
        container.LoadPlayerData();
        SceneGlobals.SceneRelay.GenerateNavMap = false;
        SceneLoader.Load(SceneType.NAVIGATION);
    }
}
