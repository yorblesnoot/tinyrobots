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
        SceneGlobals.PlayerData.MapData = towerBuilder.BuildTowerFloor(SceneGlobals.SceneRelay.GenerateNavMap ? null : SceneGlobals.PlayerData.MapData);
        SceneGlobals.SceneRelay.GenerateNavMap = false;
        unitSwitcher.Initialize();
        originSlot.gameObject.SetActive(true);
        mainUI.SetActive(true);
    }

    void SaveGame()
    {
        SaveContainer saver = new(SceneGlobals.PlayerData);
        saver.SavePlayerData();
    }
}
