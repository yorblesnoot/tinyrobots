using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotPlacer : MonoBehaviour
{
    [SerializeField] BotConverter botConverter;
    [SerializeField] List<EnemyEncounter> encounters;

    [SerializeField] PlayerData playerData;
    [SerializeField] BotAssembler botAssembler;
    [SerializeField] TurnManager turnManager;

    [SerializeField] bool useRandomPlacement;

    public void PlaceBots()
    {
        botConverter.Initialize();
        playerData.LoadRecords();
        PlaceBotsInSpawnZones();
        SpawnPoint.ReadyToSpawn?.Invoke(this);
    }

    [HideInInspector] public List<SpawnZone> spawnZones;
    private void PlaceBotsInSpawnZones()
    {
        List<TinyBot> bots = new();
        bots.AddRange(playerData.coreInventory.Select(core => 
            SpawnBot(Allegiance.PLAYER, core.bot)));
        EnemyEncounter encounter = encounters.GrabRandomly(false);
        List<BotRecord> enemyRecords = encounter.GetSpawnList(playerData.difficulty);
        bots.AddRange(enemyRecords.Select(record => SpawnBot(Allegiance.ENEMY, record)));

        Dictionary<MoveStyle, List<Vector3Int>> styleNodes = Pathfinder3D.GetStyleNodes();
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
            List<Vector3Int> nodes = styleNodes[style];
            foreach (Vector3 node in nodes)
            {
                SpawnZone targetZone = spawnZones.Where(zone => Vector3.Distance(node, zone.Position) < zone.Radius).FirstOrDefault();
                if(targetZone == null) continue;
                spawnSlots[targetZone.Allegiance][style].Add(node);
            }
        }

        foreach (TinyBot bot in bots)
        {
            MoveStyle style = bot.PrimaryMovement.Style;
            OrientBot(bot, spawnSlots[bot.allegiance][style].GrabRandomly());
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
        return SpawnBot(allegiance, tree);
    }

    public TinyBot SpawnBot(Allegiance allegiance, TreeNode<ModdedPart> tree)
    {
        TinyBot botUnit = botAssembler.BuildBotFromPartTree(tree, allegiance);
        botUnit.allegiance = allegiance;

        turnManager.AddTurnTaker(botUnit);
        return botUnit;
    }

}
