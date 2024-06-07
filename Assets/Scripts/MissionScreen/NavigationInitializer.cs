using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationInitializer : MonoBehaviour
{
    [SerializeField] TowerBuilder towerBuilder;
    [SerializeField] SceneRelay relay;
    void Start()
    {
        towerBuilder.GeneratePlaySpace();
        relay.generateNavMap = false;
    }
}
