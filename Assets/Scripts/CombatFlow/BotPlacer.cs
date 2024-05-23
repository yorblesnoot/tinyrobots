using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotPlacer : MonoBehaviour
{
    [SerializeField] BotConverter botConverter;

    [SerializeField] List<BotRecord> playerBots;
    [SerializeField] List<BotRecord> enemyBots;

    [SerializeField] PlayerData playerData;
    [SerializeField] BotAssembler botAssembler;
    [SerializeField] TurnManager turnManager;

    [SerializeField] bool useRandomPlacement;

    public void PlaceBots()
    {
        botConverter.Initialize();
        PlaceBotsInSpawnZones();
        SpawnPoint.ReadyToSpawn?.Invoke(this);
    }

    [HideInInspector] public List<SpawnZone> spawnZones;
    private void PlaceBotsInSpawnZones()
    {
        List<TinyBot> bots = new();
        SpawnBotList(playerBots, Allegiance.PLAYER);
        SpawnBotList(enemyBots, Allegiance.ENEMY);

        Dictionary<MoveStyle, List<Vector3>> styleNodes = Pathfinder3D.GetStyleNodes();
        spawnZones = new();
        SpawnZone.GetSpawnZones.Invoke(this);


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
                SpawnZone targetZone = spawnZones.Where(zone => Vector3.Distance(node, zone.Position) < zone.Radius).FirstOrDefault();
                if(targetZone == null) continue;
                spawnSlots[targetZone.Allegiance][style].Add(node);
                Debug.Log(targetZone);
            }
        }
        Debug.Log(spawnSlots[Allegiance.PLAYER][MoveStyle.FLY].Count);

        foreach (TinyBot bot in bots)
        {
            MoveStyle style = bot.PrimaryMovement.Style;
            OrientBot(bot, spawnSlots[bot.allegiance][style].GrabRandomly());
        }

        void SpawnBotList(List<BotRecord> botRecords, Allegiance allegiance)
        {
            foreach (var botRecord in botRecords)
            {
                TinyBot botUnit = SpawnBot(allegiance, botRecord);
                bots.Add(botUnit);
            }
        }
    }

    public void SubmitZone(SpawnZone zone)
    {
        spawnZones.Add(zone);
    }

    public void OrientBot(TinyBot bot, Vector3 position)
    {
        bot.transform.position = position;
        bot.PrimaryMovement.SpawnOrientation();
        StartCoroutine(bot.PrimaryMovement.NeutralStance());
    }

    public TinyBot SpawnBot(Allegiance allegiance, BotRecord botRecord)
    {
        var tree = botConverter.StringToBot(botRecord.record);
        TinyBot botUnit = botAssembler.BuildBotFromPartTree(tree, allegiance);
        botUnit.allegiance = allegiance;

        turnManager.AddTurnTaker(botUnit);
        return botUnit;
    }

}
