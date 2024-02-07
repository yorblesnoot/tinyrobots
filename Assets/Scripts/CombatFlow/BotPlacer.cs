using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotPlacer : MonoBehaviour
{
    [SerializeField] BotConverter botConverter;

    [SerializeField] List<BotRecord> playerBots;
    [SerializeField] List<BotRecord> enemyBots;

    [SerializeField] PlayerData playerData;
    [SerializeField] BotAssembler botAssembler;
    [SerializeField] TurnManager turnManager;

    public void PlaceBots()
    {
        botConverter.Initialize();
        SpawnBotList(playerBots, Allegiance.PLAYER);
        SpawnBotList(enemyBots, Allegiance.ENEMY);
    }
    public void SpawnBotList(List<BotRecord> botRecords, Allegiance allegiance)
    {
        foreach (var botRecord in botRecords)
        {
            var tree = botConverter.StringToBot(botRecord.record);
            TinyBot botUnit = botAssembler.BuildBotFromPartTree(tree);
            botUnit.allegiance = allegiance;
            turnManager.AddTurnTaker(botUnit);
        }

    }
}
