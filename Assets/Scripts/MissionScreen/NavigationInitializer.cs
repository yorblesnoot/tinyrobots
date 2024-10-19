using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationInitializer : MonoBehaviour
{
    [SerializeField] TowerBuilder towerBuilder;
    [SerializeField] UnitSwitcher unitSwitcher;
    [SerializeField] PartSlot originSlot;
    [SerializeField] GameObject mainUI;
    void Start()
    {
        PlayerNavigator.MoveComplete.AddListener(SaveGame);
        unitSwitcher.Initialize();
        towerBuilder.DeployTowerFloor(SceneGlobals.PlayerData.MapData, SceneGlobals.SceneRelay.GenerateNavMap);
        SceneGlobals.SceneRelay.GenerateNavMap = false;
        
        originSlot.gameObject.SetActive(true);
        mainUI.SetActive(true);
    }

    void SaveGame()
    {
        SaveContainer saver = new(SceneGlobals.PlayerData);
        saver.SavePlayerData();
    }
}
