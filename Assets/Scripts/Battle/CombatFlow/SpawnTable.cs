using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyEncounter", menuName = "ScriptableObjects/EnemyEncounter")]
public class SpawnTable : ScriptableObject
{
    [SerializeField] List<Spawn> enemies;

    public List<BotRecord> GetSpawnList(int size)
    {
        List<BotRecord> enemyOutput = new();
        for(int i = 0; i < enemies.Count; i++)
        {
            Spawn spawn = enemies[i];
            if (spawn.value > size) continue;

            enemyOutput.Add(spawn.unit);
            size -= spawn.value;
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
