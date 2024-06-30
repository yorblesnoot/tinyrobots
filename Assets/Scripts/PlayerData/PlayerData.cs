using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Singletons/PlayerData")]
public class PlayerData : ScriptableObject
{
    public void LoadRecords()
    {
        if(DevMode) PartInventory = botConverter.PartLibrary.Select(part => new ModdedPart(part)).ToList();
        foreach(BotCore core in CoreInventory)
        {
            if (core.bot != null || core.StarterRecord == null) continue;
            
            core.bot = botConverter.StringToBot(core.StarterRecord.record);
        }
    }
    public List<ModdedPart> PartInventory;
    public List<BotCore> CoreInventory;
    public List<SavedNavZone> MapData;
    public int ZoneLocation;
    public int Difficulty = 3;
    public bool DevMode;

    [Header("Components")]
    [SerializeField] BotConverter botConverter;
}

public class SavedNavZone
{
    public int pieceIndex;
    public Vector3 position;
    public Quaternion rotation;
    public int[] neighborIndices;
    public bool revealed;
    public int eventType;
}


