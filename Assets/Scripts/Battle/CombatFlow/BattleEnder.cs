
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleEnder : MonoBehaviour
{
    [SerializeField] SceneRelay relay;
    [SerializeField] PlayerData playerData;
    [SerializeField] DropsUI dropsUI;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] List<GameObject> clearedUI;

    static BattleEnder instance;
    static bool ended = false;
    private void Awake()
    {
        instance = this;
        TinyBot.BotDied.AddListener(CheckoutBot);
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
        instance.ClearUI();
        instance.relay.BattleComplete = true;
        instance.dropsUI.ShowDrops();

        foreach(TinyBot bot in TurnManager.TurnTakers.Where(bot => bot.Allegiance == Allegiance.PLAYER))
        {
            CheckoutBot(bot);
        }
    }

    static void CheckoutBot(TinyBot bot)
    {
        if (bot == null || bot.LinkedCore == null) return;
        bot.LinkedCore.HealthRatio.Value = (float)bot.Stats.Current[StatType.HEALTH] / bot.Stats.Max[StatType.HEALTH];
        bot.LinkedCore.Mana.Value = bot.Stats.Current[StatType.MANA];
    }

    static void GameOver()
    {
        if (ended) return;
        instance.ClearUI();
        SaveContainer save = new(SceneGlobals.PlayerData);
        save.ClearPlayerData();
        instance.gameOverScreen.SetActive(true);
    }

    void ClearUI()
    {
        PrimaryCursor.TogglePlayerLockout(true);
        PrimaryCursor.RestrictCursor();
        foreach (var obj in clearedUI) obj.SetActive(false);
    }
}
