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
            if(core.record != null)
            {
                core.bot = botConverter.StringToBot(core.record.record);
            }
        }
    }
    public List<CraftablePart> partInventory;
    public List<BotCore> coreInventory;

    [Header("Components")]
    [SerializeField] BotConverter botConverter;
}
