using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EncounterGenerator : ScriptableObject
{
    public abstract List<BotRecord> GetSpawnList(int tier);
}
