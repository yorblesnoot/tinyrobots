using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Singletons/PlayerData")]
public class PlayerData : ScriptableObject
{
    public void LoadRecords()
    {
        foreach(BotCore core in coreInventory)
        {
            if(core.bot == null && core.record != null)
            {
                core.bot = botConverter.StringToBot(core.record.record);
            }
        }
    }
    public List<CraftablePart> partInventory;
    public List<BotCore> coreInventory;
    public List<SavedNavZone> mapData;
    public int zoneLocation;

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
    public ZoneEventType eventType;
}


