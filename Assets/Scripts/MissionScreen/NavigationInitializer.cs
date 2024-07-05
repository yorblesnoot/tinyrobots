using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationInitializer : MonoBehaviour
{
    [SerializeField] TowerBuilder towerBuilder;
    [SerializeField] SceneRelay relay;
    [SerializeField] BlueprintControl blueprintControl;
    [SerializeField] PartSlot originSlot;
    [SerializeField] PlayerData playerData;
    void Start()
    {
        towerBuilder.DeployNavSpace(playerData.MapData, relay.generateNavMap);
        relay.generateNavMap = false;
        blueprintControl.Initialize();
        originSlot.gameObject.SetActive(true);
    }
}
