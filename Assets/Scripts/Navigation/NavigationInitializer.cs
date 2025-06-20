using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationInitializer : MonoBehaviour
{
    [SerializeField] TowerBuilder towerBuilder;
    [SerializeField] BotCrafter unitSwitcher;
    [SerializeField] GameObject mainUI;
    void Start()
    {
        SceneGlobals.PlayerData.ShopData.Initialize();
        PlayerNavigator.MoveComplete.AddListener(SaveGame);
        unitSwitcher.Initialize();
        towerBuilder.DeployTowerFloor(SceneGlobals.PlayerData.MapData, SceneGlobals.SceneRelay.GenerateNavMap);
        SceneGlobals.SceneRelay.GenerateNavMap = false;
        
        
        mainUI.SetActive(true);
    }

    void SaveGame()
    {
        SaveContainer saver = new(SceneGlobals.PlayerData);
        saver.SavePlayerData();
    }
}
