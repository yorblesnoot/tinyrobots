using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotPlacer : MonoBehaviour
{
    [SerializeField] BotConverter botConverter;
    [SerializeField] List<BotRecord> botRecords;
    [SerializeField] PlayerData playerData;
    [SerializeField] BotAssembler botAssembler;

    public void PlaceBots()
    {
        foreach (var botRecord in botRecords)
        {
            var tree = botConverter.StringToBot(botRecord.record);
            botAssembler.BuildBotFromTree(tree);
        }
    }
}
