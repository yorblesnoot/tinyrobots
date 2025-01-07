using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ShopData
{
    public Shop this[int zone]
    {
        get { return shopCodex[zone]; }
    }
    public void Initialize()
    {
        shopCodex = Shops.ToDictionary(shop => shop.Location, shop => shop);
    }
    public List<Shop> Shops = new();
    Dictionary<int, Shop> shopCodex = new();
    public void GenerateShop(int zone, PartGenerator generator)
    {
        if(shopCodex.ContainsKey(zone)) return;
        Shop shop = new() { Location = zone, Inventory = generator.GenerateDropList() };
        Shops.Add(shop);
        shopCodex.Add(zone, shop);
    }

    
}

[System.Serializable]
public class Shop
{
    public int Location;
    public List<ModdedPart> Inventory;
}


