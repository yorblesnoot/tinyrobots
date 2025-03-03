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
    [SerializeField] List<BotRecord> enemyBotOverride;
    [SerializeField] bool selectFirstBot = true;
    [SerializeField] bool useDeployment = true;
    public static Mission Active;
    public void BeginMission()
    {
        Active = this;
        TurnManager.RoundEnded.AddListener(RoundEnd);
        InitializeMission();
        if(!useDeployment) TurnManager.BeginTurnSequence(selectFirstBot);
    }

    private void OnDestroy()
    {
        TurnManager.RoundEnded.RemoveListener(RoundEnd);
    }




    public TinyBot SpawnBot(Allegiance allegiance, BotRecord botRecord)
    {
        var tree = SceneGlobals.PlayerData.BotConverter.StringToBot(botRecord.Record);
        TinyBot bot = BotAssembler.BuildBot(tree, allegiance);
        TurnManager.AddTurnTaker(bot);
        return bot;
    }

    //spawn enemy bots and add to drops ~~~

    
    public virtual void RoundEnd() { }
    public abstract bool MetVictoryCondition();
    protected virtual void InitializeMission()
    {
        SceneGlobals.PlayerData.BotConverter.Initialize();
        SceneGlobals.PlayerData.LoadDefaultInventory();
        SpawnBots();
    }

    private void SpawnBots()
    {
        List<TinyBot> enemyBots = SpawnEnemyBots();
        List<TinyBot> playerBots = SpawnPlayerBots();
        if (useDeployment) StartCoroutine(DeploymentPhase.BeginDeployment(playerBots, () => TurnManager.BeginTurnSequence(selectFirstBot)));
        else enemyBots.AddRange(playerBots);
        

        foreach (TinyBot bot in enemyBots) SpawnZone.PlaceBot(bot);
    }

    protected List<TinyBot> SpawnPlayerBots()
    {
        List<TinyBot> bots = new();
        if (playerBotOverride != null) bots.AddRange(playerBotOverride.Select(record => SpawnBot(Allegiance.PLAYER, record)));
        foreach (var core in SceneGlobals.PlayerData.CoreInventory)
        {
            if (core.HealthRatio == 0 && !SceneGlobals.PlayerData.DevMode) continue;
            TinyBot bot = BotAssembler.BuildBot(core.Bot, Allegiance.PLAYER, grantUniversals: true);
            bot.LinkedCore = core;
            float healthRatio = SceneGlobals.PlayerData.DevMode ? 1 : core.HealthRatio;
            bot.Stats.Current[StatType.HEALTH] = Mathf.RoundToInt(bot.Stats.Max[StatType.HEALTH] * healthRatio);
            TurnManager.AddTurnTaker(bot);
            bots.Add(bot);
            bot.gameObject.SetActive(false);
        }
        return bots;
    }

    private List<TinyBot> SpawnEnemyBots()
    {
        List<TinyBot> bots = new();
        EncounterGenerator table = SceneGlobals.SceneRelay.ActiveSpawnTable;
        if (table != null)
        {
            List<BotRecord> enemyRecords = table.GetSpawnList(SceneGlobals.PlayerData.Difficulty);
            bots.AddRange(enemyRecords.Select(record => SpawnBot(Allegiance.ENEMY, record)));
        }
        if (enemyBotOverride != null) bots.AddRange(enemyBotOverride.Select(record => SpawnBot(Allegiance.ENEMY, record)));
        return bots;
    }
}
