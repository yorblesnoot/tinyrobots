using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationInitializer : MonoBehaviour
{
    [SerializeField] TowerBuilder towerBuilder;
    [SerializeField] SceneRelay relay;
    [SerializeField] BlueprintControl blueprintControl;
    [SerializeField] PartSlot originSlot;
    void Start()
    {
        towerBuilder.GeneratePlaySpace();
        relay.generateNavMap = false;
        blueprintControl.Initialize();
        originSlot.gameObject.SetActive(true);
    }
}
