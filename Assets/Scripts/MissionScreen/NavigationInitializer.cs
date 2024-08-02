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
        SceneGlobals.PlayerData.MapData = towerBuilder.BuildTowerFloor(SceneGlobals.SceneRelay.generateNavMap ? null : SceneGlobals.PlayerData.MapData);
        SceneGlobals.SceneRelay.generateNavMap = false;
        unitSwitcher.Initialize();
        originSlot.gameObject.SetActive(true);
        mainUI.SetActive(true);
    }
}
