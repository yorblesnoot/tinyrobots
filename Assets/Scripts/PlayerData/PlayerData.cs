using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Singletons/PlayerData")]
public class PlayerData : ScriptableObject
{
    public void LoadRecords()
    {
        foreach(BotCore core in coreInventory)
        {
            if(core.bot == null && core.StarterRecord != null)
            {
                core.bot = botConverter.StringToBot(core.StarterRecord.record);
            }
        }
    }
    public List<CraftablePart> partInventory;
    public List<BotCore> coreInventory;
    public List<SavedNavZone> mapData;
    public int zoneLocation;
    public int difficulty = 3;

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


