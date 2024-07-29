using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationInitializer : MonoBehaviour
{
    [SerializeField] TowerBuilder towerBuilder;
    [SerializeField] SceneRelay relay;
    [SerializeField] BlueprintControl blueprintControl;
    [SerializeField] PartSlot originSlot;
    [SerializeField] GameObject mainUI;
    void Start()
    {
        SceneGlobals.PlayerData.MapData = towerBuilder.BuildTowerFloor(relay.generateNavMap ? null : SceneGlobals.PlayerData.MapData);
        relay.generateNavMap = false;
        blueprintControl.Initialize();
        originSlot.gameObject.SetActive(true);
        mainUI.SetActive(true);
    }
}
