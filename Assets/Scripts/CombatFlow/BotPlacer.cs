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
        List<TinyBot> bots = new();
        botConverter.Initialize();
        SpawnBotList(playerBots, Allegiance.PLAYER);
        SpawnBotList(enemyBots, Allegiance.ENEMY);

        Dictionary<MoveStyle, List<Vector3>> styleNodes = Pathfinder3D.GetStyleNodes();
        foreach(var bot in bots)
        {
            Debug.Log(bot);
            Debug.Log(bot.PrimaryMovement);
            Debug.Log(bot.PrimaryMovement.MoveStyle);
            MoveStyle style = bot.PrimaryMovement.MoveStyle;
            bot.transform.position = styleNodes[style].GrabRandomly();
        }

        void SpawnBotList(List<BotRecord> botRecords, Allegiance allegiance)
        {
            
            foreach (var botRecord in botRecords)
            {
                var tree = botConverter.StringToBot(botRecord.record);
                TinyBot botUnit = botAssembler.BuildBotFromPartTree(tree);
                botUnit.allegiance = allegiance;
                bots.Add(botUnit);
                turnManager.AddTurnTaker(botUnit);
            }

        }
    }
    
}
