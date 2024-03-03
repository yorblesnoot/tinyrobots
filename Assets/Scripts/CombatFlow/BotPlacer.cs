using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BotPlacer : MonoBehaviour
{
    [SerializeField] BotConverter botConverter;

    [SerializeField] List<BotRecord> playerBots;
    [SerializeField] List<BotRecord> enemyBots;

    [SerializeField] PlayerData playerData;
    [SerializeField] BotAssembler botAssembler;
    [SerializeField] TurnManager turnManager;

    //Dictionary<Allegiance, Color>
    [SerializeField] SerializableDictionary<Allegiance, Color> colorMaps;


    public void PlaceBots()
    {
        List<TinyBot> bots = new();
        botConverter.Initialize();
        SpawnBotList(playerBots, Allegiance.PLAYER);
        SpawnBotList(enemyBots, Allegiance.ENEMY);

        Dictionary<MoveStyle, List<Vector3>> styleNodes = Pathfinder3D.GetStyleNodes();
        Dictionary<Allegiance, Dictionary<MoveStyle, List<Vector3>>> spawnSlots = new()
        {
            { Allegiance.PLAYER, new() },
            { Allegiance.ENEMY, new() }
        };
        foreach (var style in styleNodes.Keys)
        {
            spawnSlots[Allegiance.PLAYER].Add(style, new());
            spawnSlots[Allegiance.ENEMY].Add(style, new());
            List<Vector3> nodes = styleNodes[style];
            foreach (Vector3 node in nodes)
            {
                if (node.y < spawnLowerCutoff || node.y > spawnUpperCutoff) continue;
                if (node.x + node.z < Pathfinder3D.xSize / zoneDivisor) spawnSlots[Allegiance.PLAYER][style].Add(node);
                if (node.x + node.z > Pathfinder3D.xSize * 2 - Pathfinder3D.xSize / zoneDivisor) spawnSlots[Allegiance.ENEMY][style].Add(node);
            }
        }

        foreach (var bot in bots)
        {
            MoveStyle style = bot.PrimaryMovement.Style;
            bot.transform.position = spawnSlots[bot.allegiance][style].GrabRandomly();
            bot.PrimaryMovement.SpawnOrientation();
            StartCoroutine(bot.PrimaryMovement.NeutralStance());
        }

        void SpawnBotList(List<BotRecord> botRecords, Allegiance allegiance)
        {
            
            foreach (var botRecord in botRecords)
            {
                var tree = botConverter.StringToBot(botRecord.record);
                TinyBot botUnit = botAssembler.BuildBotFromPartTree(tree, allegiance);
                botUnit.allegiance = allegiance;
                bots.Add(botUnit);
                turnManager.AddTurnTaker(botUnit);
            }

        }
    }

    [SerializeField] int zoneDivisor = 2;
    [SerializeField] int spawnLowerCutoff = 5;
    [SerializeField] int spawnUpperCutoff = 30;
}
