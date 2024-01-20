using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Singletons/PlayerData")]
public class PlayerData : ScriptableObject
{
    public List<CraftablePart> partInventory;
    public List<BotRecord> botsInventory;
}
