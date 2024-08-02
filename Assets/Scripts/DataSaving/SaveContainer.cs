using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveContainer
{
    readonly PlayerData playerData;
    readonly string savePath;

    public SaveContainer(PlayerData data)
    {
        playerData = data;
        savePath = Application.persistentDataPath + Path.DirectorySeparatorChar + "runData.json";
    }

    public void SavePlayerData()
    {
        Save save = new()
        {
            partInventory = playerData.PartInventory.Select(part => BotConverter.PartToString(part)).ToList(),
            map = playerData.MapData,
            difficulty = playerData.Difficulty,
            coreInventory = SaveCores()
        };
        string saveJSON = JsonUtility.ToJson(save, true);
        File.WriteAllText(savePath, saveJSON);
        //Debug.Log(saveJSON);
    }

    private List<CoreData> SaveCores()
    {
        List<CoreData> coreInventory = new();
        foreach (var core in playerData.CoreInventory)
        {
            CoreData coreData = new()
            {
                guid = core.Id,
                healthRatio = core.HealthRatio,
                bot = BotConverter.BotToString(core.Bot)
            };
            coreInventory.Add(coreData);
        }
        return coreInventory;
    }

    public void LoadPlayerData()
    {
        BotConverter converter = playerData.BotConverter;
        converter.Initialize();

        string saveJSON = File.ReadAllText(savePath);
        Debug.Log(saveJSON);
        Save save = JsonUtility.FromJson<Save>(saveJSON);
        
        playerData.MapData = save.map;
        playerData.Difficulty = save.difficulty;
        playerData.PartInventory = save.partInventory.Select(s => { converter.GetPartFromSequence(s, 0, out ModdedPart part); return part; }).ToList();
        playerData.CoreInventory = LoadCores(save, converter);
    }

    List<BotCore> LoadCores(Save save, BotConverter converter)
    {
        List<BotCore> finalCores = new();
        foreach(var core in save.coreInventory)
        {
            BotCore loadedCore = converter.GetCore(core.guid);
            loadedCore.Bot = converter.StringToBot(core.bot);
            loadedCore.HealthRatio = core.healthRatio;
            finalCores.Add(loadedCore);
        }
        return finalCores;
    }

    class Save
    {
        public List<string> partInventory;
        public List<CoreData> coreInventory;
        public List<string> bots;
        public MapData map;
        public int difficulty;
    }

    [Serializable]
    class CoreData
    {
        public string guid;
        public string bot;
        public float healthRatio;
    }
}
