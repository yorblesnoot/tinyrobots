using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyPanel : MonoBehaviour
{
    VisualizedPartInventory inventory;
    private void Awake()
    {
        inventory = GetComponent<VisualizedPartInventory>();
    }

    public void Display(Shop shop)
    {
        inventory.Initialize(shop.Inventory);
        inventory.UpdatePartDisplays();
    }
}
