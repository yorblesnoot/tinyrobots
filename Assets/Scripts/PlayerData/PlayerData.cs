using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Singletons/PlayerData")]
public class PlayerData : ScriptableObject, ITrader
{
    public void LoadDefaultInventory()
    {
        if(DevMode) PartInventory = BotConverter.PartLibrary.Where(part => part.Type != SlotType.CORE)
                .Select(part => BotConverter.GetDefaultPart(part)).ToList();
        foreach(BotCharacter core in CoreInventory)
        {
            if (RandomizeParty) core.Bot = BotRandomizer.GeneratePartTree(core.ModdedCore, BotConverter);
            else if (core.Bot == null && core.StarterRecord != null) core.Bot = BotConverter.StringToBot(core.StarterRecord.Record);
        }
    }
    [field: SerializeField] public List<ModdedPart> PartInventory { get; set; }
    public List<BotCharacter> CoreInventory;
    public MapData MapData;
    public ShopData ShopData;
    
    public int Difficulty = 3;
    [field: SerializeField] public int PartCurrency { get; set; }


    public bool DevMode;
    public bool RandomizeParty;

    [Header("Components")]
    public BotConverter BotConverter;
}





