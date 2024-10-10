
using System.Linq;
using UnityEngine;

public class BattleEnder : MonoBehaviour
{
    [SerializeField] SceneRelay relay;
    [SerializeField] PlayerData playerData;
    [SerializeField] DropsUI dropsUI;

    static BattleEnder instance;
    static bool ended = false;
    private void Awake()
    {
        instance = this;
    }

    public static bool IsMissionOver()
    {
        if (TurnManager.TurnTakers.Where(bot => bot.Allegiance == Allegiance.PLAYER).Count() == 0)
        {
            GameOver();
            return true;
        }
        else if (Mission.Active.MetVictoryCondition())
        {
            PlayerWin();
            return true;
        }
        return false;
    }

    static void PlayerWin()
    {
        if(ended) return;
        if (instance.playerData == null) Debug.LogWarning("No active Navigation Map found.");
        instance.relay.BattleComplete = true;
        instance.dropsUI.ShowDrops(() => SceneLoader.Load(SceneType.NAVIGATION));

        foreach(TinyBot bot in TurnManager.TurnTakers)
        {
            if (bot.Allegiance != Allegiance.PLAYER) continue;
            bot.LinkedCore.HealthRatio = (float)bot.Stats.Current[StatType.HEALTH] / bot.Stats.Max[StatType.HEALTH];
        }
    }

    static void GameOver()
    {
        if (ended) return;
        instance.relay.GenerateNavMap = true;
        SceneLoader.Load(SceneType.MAINMENU);
    }
}
