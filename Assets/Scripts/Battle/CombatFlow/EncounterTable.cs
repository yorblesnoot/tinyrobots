using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyEncounter", menuName = "ScriptableObjects/EnemyEncounter")]
public class EncounterTable : EncounterGenerator
{
    [SerializeField] List<Spawn> enemies;

    public override List<BotRecord> GetSpawnList(int tier)
    {
        List<BotRecord> enemyOutput = new();
        for(int i = 0; i < enemies.Count; i++)
        {
            Spawn spawn = enemies[i];
            if (spawn.value > tier) continue;

            enemyOutput.Add(spawn.unit);
            tier -= spawn.value;
        }
        return enemyOutput;
    }

    [Serializable]
    class Spawn
    {
        public BotRecord unit;
        public int value = 1;
    }
}
