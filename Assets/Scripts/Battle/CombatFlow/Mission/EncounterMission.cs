using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EncounterMission : Mission
{
    [SerializeField] List<SpawnTable> spawnTables;
    [SerializeField] int difficultyDivisor = 2;

    public override bool MetEndCondition(TurnManager turnManager)
    {
        if (TurnManager.TurnTakers.Where(bot => bot.Allegiance == Allegiance.PLAYER).Count() == 0)
        {
            BattleEnder.GameOver();
            return true;
        }
        else if (TurnManager.TurnTakers.Where(bot => bot.Allegiance == Allegiance.ENEMY).Count() == 0)
        {
            BattleEnder.PlayerWin();
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

        foreach (TinyBot bot in bots) SpawnZone.PlaceBot(bot);
    }
}
