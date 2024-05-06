using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Singletons/PlayerData")]
public class PlayerData : ScriptableObject
{
    public void LoadRecords()
    {
        botsInventory = records.Select(x => botConverter.StringToBot(x.record)).ToList();
    }
    [SerializeField] List<BotRecord> records;
    public List<CraftablePart> partInventory;
    public List<TreeNode<CraftablePart>> botsInventory;

    [Header("Components")]
    [SerializeField] BotConverter botConverter;
}
