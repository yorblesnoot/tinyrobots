using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EncounterRandomizer", menuName = "ScriptableObjects/EncounterRandomizer")]
public class EncounterRandomizer : EncounterGenerator
{
    [SerializeField] int tierMultiplier = 3;
    [SerializeField] int baseBudget = 3;
    [SerializeField] List<Pool> pools;

    public override List<BotRecord> GetSpawnList(int tier)
    {
        List<BotRecord> spawns = new();
        List<Pool> activePools = new(pools);
        int budget = baseBudget + tier * tierMultiplier;
        while(budget > 0 && activePools.Count > 0)
        {
            Pool selected = activePools.GrabRandomly(false);
            if(selected.Value > budget)
            {
                activePools.Remove(selected);
                continue;
            }
            spawns.Add(selected.Units.GrabRandomly(false));
            budget-= selected.Value;
        }
        return spawns;
    }


    [Serializable]
    struct Pool
    {
        public int Value;
        public List<BotRecord> Units;
    }
}
