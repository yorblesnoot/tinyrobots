using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MissionType
{
    ENCOUNTER,
    AMBUSH,
    SCRIPTED
}
public abstract class Mission : MonoBehaviour
{
    [SerializeField] protected PlayerData playerData;
    [SerializeField] protected SceneRelay relay;
    [SerializeField] List<BotRecord> playerBotOverride;
    public void BeginMission()
    {
        TurnManager.Mission = this;
        InitializeMission();
        TurnManager.BeginTurnSequence();
    }
    public TinyBot SpawnBot(Allegiance allegiance, BotRecord botRecord)
    {
        var tree = playerData.BotConverter.StringToBot(botRecord.record);
        TinyBot bot = BotAssembler.BuildBot(tree, allegiance);
        TurnManager.AddTurnTaker(bot);
        return bot;
    }

    protected List<TinyBot> SpawnPlayerBots()
    {
        List<TinyBot> bots = new();
        if (playerBotOverride != null) bots.AddRange(playerBotOverride.Select(record => SpawnBot(Allegiance.PLAYER, record)));
        foreach (var core in playerData.CoreInventory)
        {
            if (!core.Deployable || core.HealthRatio == 0) continue;
            TinyBot bot = BotAssembler.BuildBot(core.Bot, Allegiance.PLAYER);
            TurnManager.AddTurnTaker(bot);
            bot.LinkedCore = core;
            bot.Stats.Current[StatType.HEALTH] = Mathf.RoundToInt(bot.Stats.Max[StatType.HEALTH] * core.HealthRatio);
            bots.Add(bot);
        }
        return bots;
    }

    
    public virtual void RoundEnd() { }
    public abstract bool MetEndCondition(TurnManager turnManager);
    protected abstract void InitializeMission();
    protected void OrientBot(TinyBot bot, Vector3 position)
    {
        bot.transform.position = position;
        bot.PrimaryMovement.SpawnOrientation();
        bot.StartCoroutine(bot.PrimaryMovement.NeutralStance());
    }
}
