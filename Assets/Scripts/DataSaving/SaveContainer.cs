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

    public static void SaveGame(PlayerData data)
    {
        SaveContainer container = new(data);
        container.SavePlayerData();
    }

    public void SavePlayerData()
    {
        Save save = new()
        {
            PartInventory = SaveInventory(playerData.PartInventory),
            Map = playerData.MapData,
            Shops = SaveShops(playerData.ShopData),
            Difficulty = playerData.Difficulty,
            CoreInventory = SaveCores(),
            PartCurrency = playerData.PartCurrency,
        };
        string saveJSON = JsonUtility.ToJson(save, true);
        File.WriteAllText(savePath, saveJSON);
        //Debug.Log(saveJSON);
    }

    ShopData SaveShops(ShopData data)
    {
        foreach (Shop shop in data.Shops)
        {
            shop.SavedInventory = SaveInventory(shop.PartInventory);
        }
        return data;
    }

    ShopData LoadShops(ShopData data, BotConverter converter)
    {
        foreach (Shop shop in data.Shops)
        {
            shop.PartInventory = LoadInventory(shop.SavedInventory, converter);
        }
        data.Initialize();
        return data;
    }

    private List<CoreData> SaveCores()
    {
        List<CoreData> coreInventory = new();
        foreach (var core in playerData.CoreInventory)
        {
            CoreData coreData = new()
            {
                Guid = core.Id,
                HealthRatio = core.HealthRatio.Value,
                Mana = core.Mana.Value,
                Bot = BotConverter.BotToString(core.Bot)
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
        Save save = JsonUtility.FromJson<Save>(saveJSON);
        
        playerData.MapData = save.Map;
        playerData.ShopData = LoadShops(save.Shops, converter);
        playerData.Difficulty = save.Difficulty;
        playerData.PartInventory = LoadInventory(save.PartInventory, converter);
        playerData.CoreInventory = LoadCores(save, converter);
        playerData.PartCurrency = save.PartCurrency;
    }

    public static List<string> SaveInventory(List<ModdedPart> input)
    {
        return input.Select(part => BotConverter.PartToString(part)).ToList();
    }

    public static List<ModdedPart> LoadInventory(List<string> input, BotConverter converter)
    {
        List<ModdedPart> parts = new();
        foreach (var partString in input)
        {
            converter.GetPartFromSequence(partString, 0, out ModdedPart part);
            parts.Add(part);
            //Debug.Log($"Loaded part: {part.BasePart.name} with {part.Mutators.Count} mutators.");
        }
        return parts;
    }

    List<BotCharacter> LoadCores(Save save, BotConverter converter)
    {
        List<BotCharacter> finalCores = new();
        foreach(var core in save.CoreInventory)
        {
            BotCharacter loadedCore = converter.GetCore(core.Guid);
            loadedCore.Bot = converter.StringToBot(core.Bot);
            loadedCore.HealthRatio.Value = core.HealthRatio;
            loadedCore.Mana.Value = core.Mana;
            finalCores.Add(loadedCore);
        }
        return finalCores;
    }

    public void ClearPlayerData()
    {
        File.Delete(savePath);
    }

    public bool SaveExists()
    {
        return File.Exists(savePath);
    }

    class Save
    {
        public List<string> PartInventory;
        public List<CoreData> CoreInventory;
        public MapData Map;
        public ShopData Shops;
        public int Difficulty;
        public int PartCurrency;
    }

    [Serializable]
    class CoreData
    {
        public string Guid;
        public string Bot;
        public float HealthRatio;
        public int Mana;
    }
}
