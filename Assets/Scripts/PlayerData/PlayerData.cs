using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Singletons/PlayerData")]
public class PlayerData : ScriptableObject
{
    public void LoadRecords()
    {
        if(DevMode) PartInventory = BotConverter.PartLibrary.Where(part => part.Type != SlotType.CORE)
                .Select(part => new ModdedPart(part)).ToList();
        foreach(BotCore core in CoreInventory)
        {
            if (core.Bot != null || core.StarterRecord == null) continue;
            
            core.Bot = BotConverter.StringToBot(core.StarterRecord.record);
        }
    }
    public List<ModdedPart> PartInventory;
    public List<BotCore> CoreInventory;
    public MapData MapData;
    
    public int Difficulty = 3;


    public bool DevMode;

    [Header("Components")]
    public BotConverter BotConverter;
}




