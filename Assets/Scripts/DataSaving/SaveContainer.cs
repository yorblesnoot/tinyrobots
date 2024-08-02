using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveContainer
{
    PlayerData playerData;
    string savePath;
    string saveJSON;


    public SaveContainer(PlayerData data)
    {
        playerData = data;
        savePath = Application.persistentDataPath + Path.DirectorySeparatorChar + "runData.json";
    }

    public Random.State randomState;

    public void SaveGame()
    {
        Save save = new()
        {
            partInventory = playerData.PartInventory.Select(part => BotConverter.PartToString(part)).ToList(),
            coreInventory = playerData.CoreInventory.Select(core => core.Id).ToList(),
            bots = playerData.CoreInventory.Select(core => BotConverter.BotToString(core.Bot)).ToList(),
            map = playerData.MapData
        };
        saveJSON = JsonUtility.ToJson(save, true);
        File.WriteAllText(savePath, saveJSON);
        //Debug.Log(saveJSON);
    }

    public void LoadGame()
    {
        saveJSON = File.ReadAllText(savePath);
        Save save = JsonUtility.FromJson<Save>(saveJSON);
    }

    class Save
    {
        public List<string> partInventory;
        public List<string> coreInventory;
        public List<string> bots;
        public MapData map;
    }
}
