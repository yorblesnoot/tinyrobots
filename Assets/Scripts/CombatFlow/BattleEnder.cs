using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleEnder : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] SceneRelay relay;
    [SerializeField] PlayerData playerData;
    public void PlayerWin()
    {
        playerData.mapData[playerData.zoneLocation].eventType = ZoneEventType.NONE;
        sceneLoader.Load(SceneType.NAVIGATION);
    }

    public void GameOver()
    {
        relay.generateNavMap = true;
        sceneLoader.Load(SceneType.MAINMENU);
        
    }
}
