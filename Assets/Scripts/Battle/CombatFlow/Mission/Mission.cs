using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] protected BotAssembler botAssembler;
    public void BeginMission()
    {
        TurnManager.Mission = this;
        InitializeMission();
        TurnManager.BeginTurnSequence();
    }
    public TinyBot SpawnBot(Allegiance allegiance, BotRecord botRecord)
    {
        var tree = playerData.BotConverter.StringToBot(botRecord.record);
        TinyBot bot = botAssembler.BuildBot(tree, allegiance);
        TurnManager.AddTurnTaker(bot);
        return bot;
    }

    protected List<TinyBot> SpawnPlayerBots()
    {
        List<TinyBot> bots = new();
        foreach (var core in playerData.CoreInventory)
        {
            if (!core.Deployable) continue;
            TinyBot bot = botAssembler.BuildBot(core.Bot, Allegiance.PLAYER);
            TurnManager.AddTurnTaker(bot);
            bot.LinkedCore = core;
            bot.Stats.Current[StatType.HEALTH] = Mathf.RoundToInt(bot.Stats.Max[StatType.HEALTH] * core.HealthRatio);
            bots.Add(bot);
        }
        return bots;
    }

    
    public virtual void RoundEnd() { }
    public abstract bool MetEndCondition(TurnManager turnManager, BattleEnder battleEnder);
    protected abstract void InitializeMission();
    protected void OrientBot(TinyBot bot, Vector3 position)
    {
        bot.transform.position = position;
        bot.PrimaryMovement.SpawnOrientation();
        bot.StartCoroutine(bot.PrimaryMovement.NeutralStance());
    }
}
