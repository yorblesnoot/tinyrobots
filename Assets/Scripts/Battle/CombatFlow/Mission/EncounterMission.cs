using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EncounterMission : Mission
{
    [SerializeField] List<SpawnTable> spawnTables;
    [SerializeField] int difficultyDivisor = 2;
    [HideInInspector] public List<SpawnZone> SpawnZones;

    public override bool MetEndCondition(TurnManager turnManager, BattleEnder battleEnder)
    {
        if (TurnManager.TurnTakers.Where(bot => bot.Allegiance == Allegiance.PLAYER).Count() == 0)
        {
            battleEnder.GameOver();
            return true;
        }
        else if (TurnManager.TurnTakers.Where(bot => bot.Allegiance == Allegiance.ENEMY).Count() == 0)
        {
            battleEnder.PlayerWin();
            return true;
        }
        return false;
    }

    protected override void InitializeMission()
    {
        playerData.BotConverter.Initialize();
        playerData.LoadRecords();
        PlaceBotsInSpawnZones();
        SpawnPoint.ReadyToSpawn?.Invoke(this);
    }

    private void PlaceBotsInSpawnZones()
    {
        List<TinyBot> bots = SpawnPlayerBots();
        List<BotRecord> enemyRecords = relay.activeSpawnTable.GetSpawnList(playerData.Difficulty/difficultyDivisor);
        bots.AddRange(enemyRecords.Select(record => SpawnBot(Allegiance.ENEMY, record)));

        Dictionary<MoveStyle, List<Vector3Int>> styleNodes = Pathfinder3D.GetStyleNodes();
        SpawnZones = new();
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
                SpawnZone targetZone = SpawnZones.Where(zone => Vector3.Distance(node, zone.Position) < zone.Radius).FirstOrDefault();
                if (targetZone == null) continue;
                spawnSlots[targetZone.Allegiance][style].Add(node);
            }
        }

        foreach (TinyBot bot in bots)
        {
            MoveStyle style = bot.PrimaryMovement.Style;
            OrientBot(bot, spawnSlots[bot.Allegiance][style].GrabRandomly());
        }
    }

    public void SubmitZone(SpawnZone zone)
    {
        SpawnZones.Add(zone);
    }

    
}
