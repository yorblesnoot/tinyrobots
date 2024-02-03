using System.IO;
using UnityEngine;

public class SaveContainer
{
    PlayerData playerData;
    string saveJSON;
    //LoadLibrary loadLibrary;

    string savePath;

    public SaveContainer(PlayerData data /*, LoadLibrary load*/)
    {
        playerData = data;
        //loadLibrary = load;
        Initialize();
    }

    void Initialize()
    {
        savePath = Application.persistentDataPath + Path.DirectorySeparatorChar + "runData.json";
    }

    public Random.State randomState;

    public void SaveGame()
    {

        SaveNums();
        SaveGUIDs();
        saveJSON = JsonUtility.ToJson(this, false);
        File.WriteAllText(savePath, saveJSON);
        //Debug.Log(saveJSON);
    }

    void SaveNums()
    {
        /*RunData.randomState = Random.state;
        randomState = RunData.randomState;*/
    }

    void SaveGUIDs()
    {

        /*playerDeck = RunData.playerDeck.deckContents.Select(x => x.Id).ToList();
        items = RunData.itemInventory.Select(x => x.Id).ToList();
        essenceInventory = RunData.essenceInventory.Select(x => x.Id).ToList();*/
    }

    public void LoadGame()
    {
        //EventManager.loadSceneWithScreen.Invoke(1);
        //get JSON from file or elsewhere
        saveJSON = File.ReadAllText(savePath);
        JsonUtility.FromJsonOverwrite(saveJSON, this);

        LoadNums();
        LoadGUIDs();

        //EventManager.loadSceneWithScreen.Invoke(-1);
    }

    void LoadNums()
    {
        //RunData.randomState = randomState;
    }

    void LoadGUIDs()
    {
        //loadLibrary.Initialize();
    }
}
