using UnityEngine;

public class BattleEnder : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] SceneRelay relay;
    [SerializeField] PlayerData playerData;
    [SerializeField] DropsUI dropsUI;
    public void PlayerWin()
    {
        if (playerData == null) Debug.LogWarning("No active Navigation Map found.");
        playerData.MapData.Zones[playerData.MapData.ZoneLocation].eventType = 0;
        dropsUI.OfferDrops(() => sceneLoader.Load(SceneType.NAVIGATION));

        foreach(TinyBot bot in TurnManager.TurnTakers)
        {
            if (bot.Allegiance != Allegiance.PLAYER) continue;
            bot.LinkedCore.HealthRatio = (float)bot.Stats.Current[StatType.HEALTH] / bot.Stats.Max[StatType.HEALTH];
        }
    }

    public void GameOver()
    {
        relay.generateNavMap = true;
        sceneLoader.Load(SceneType.MAINMENU);
    }
}
