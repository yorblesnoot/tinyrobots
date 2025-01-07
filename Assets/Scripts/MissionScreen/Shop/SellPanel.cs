using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellPanel : MonoBehaviour
{
    VisualizedPartInventory inventory;
    private void Awake()
    {
        inventory = GetComponent<VisualizedPartInventory>();
    }

    private void OnEnable()
    {
        inventory.Initialize(SceneGlobals.PlayerData.PartInventory);
        inventory.UpdatePartDisplays();
    }
}
