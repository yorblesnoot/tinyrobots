using UnityEngine;

public class BattleEnder : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] SceneRelay relay;
    [SerializeField] PlayerData playerData;
    [SerializeField] DropsUI dropsUI;
    public void PlayerWin()
    {
        playerData.mapData[playerData.zoneLocation].eventType = 0;
        dropsUI.OfferDrops(() => sceneLoader.Load(SceneType.NAVIGATION));
    }

    public void GameOver()
    {
        relay.generateNavMap = true;
        sceneLoader.Load(SceneType.MAINMENU);
    }
}
