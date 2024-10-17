using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MissionType
{
    ENCOUNTER,
    AMBUSH,
    TUTORIAL
}
public abstract class Mission : MonoBehaviour
{
    [SerializeField] List<BotRecord> playerBotOverride;
    [SerializeField] bool selectFirstBot = true;
    public static Mission Active;
    public void BeginMission()
    {
        Active = this;
        InitializeMission();
        TurnManager.BeginTurnSequence(selectFirstBot);
    }

    
    public TinyBot SpawnBot(Allegiance allegiance, BotRecord botRecord)
    {
        var tree = SceneGlobals.PlayerData.BotConverter.StringToBot(botRecord.record);
        TinyBot bot = BotAssembler.BuildBot(tree, allegiance);
        TurnManager.AddTurnTaker(bot);
        return bot;
    }

    protected List<TinyBot> SpawnPlayerBots()
    {
        List<TinyBot> bots = new();
        if (playerBotOverride != null) bots.AddRange(playerBotOverride.Select(record => SpawnBot(Allegiance.PLAYER, record)));
        foreach (var core in SceneGlobals.PlayerData.CoreInventory)
        {
            if (core.HealthRatio == 0) continue;
            TinyBot bot = BotAssembler.BuildBot(core.Bot, Allegiance.PLAYER);
            bot.LinkedCore = core;
            bot.Stats.Current[StatType.HEALTH] = Mathf.RoundToInt(bot.Stats.Max[StatType.HEALTH] * core.HealthRatio);
            TurnManager.AddTurnTaker(bot);
            bots.Add(bot);
        }
        return bots;
    }

    
    public virtual void RoundEnd() { }
    public abstract bool MetVictoryCondition();
    protected virtual void InitializeMission()
    {
        SceneGlobals.PlayerData.BotConverter.Initialize();
        SceneGlobals.PlayerData.LoadRecords();
        PlaceBotsInSpawnZones();
    }

    private void PlaceBotsInSpawnZones()
    {
        List<TinyBot> bots = SpawnPlayerBots();
        EncounterGenerator table = SceneGlobals.SceneRelay.ActiveSpawnTable;
        if (table != null)
        {
            List<BotRecord> enemyRecords = table.GetSpawnList(SceneGlobals.PlayerData.Difficulty);
            bots.AddRange(enemyRecords.Select(record => SpawnBot(Allegiance.ENEMY, record)));
        }

        foreach (TinyBot bot in bots) SpawnZone.PlaceBot(bot);
    }
}
