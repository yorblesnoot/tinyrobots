using UnityEngine;

public class BattleEnder : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] SceneRelay relay;
    [SerializeField] PlayerData playerData;
    [SerializeField] DropsUI dropsUI;

    static BattleEnder instance;
    private void Awake()
    {
        instance = this;
    }
    public static void PlayerWin()
    {
        if (instance.playerData == null) Debug.LogWarning("No active Navigation Map found.");
        instance.relay.BattleComplete = true;
        instance.dropsUI.OfferDrops(() => instance.sceneLoader.Load(SceneType.NAVIGATION));

        foreach(TinyBot bot in TurnManager.TurnTakers)
        {
            if (bot.Allegiance != Allegiance.PLAYER) continue;
            bot.LinkedCore.HealthRatio = (float)bot.Stats.Current[StatType.HEALTH] / bot.Stats.Max[StatType.HEALTH];
        }
    }

    public static void GameOver()
    {
        instance.relay.GenerateNavMap = true;
        instance.sceneLoader.Load(SceneType.MAINMENU);
    }
}
