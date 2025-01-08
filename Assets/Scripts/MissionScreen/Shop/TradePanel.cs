using TMPro;
using UnityEngine;

public class TradePanel : MonoBehaviour
{
    VisualizedPartInventory visualInventory;
    [SerializeField] TradePanel tradeTarget;
    [SerializeField] TMP_Text currencyDisplay;
    ITrader identity;
    static SaveContainer saver;
    private void Awake()
    {
        saver ??= new(SceneGlobals.PlayerData);
        visualInventory = GetComponent<VisualizedPartInventory>();
    }

    public void Display(ITrader trader)
    {
        identity = trader;
        visualInventory.Initialize(trader.PartInventory);
        UpdateDisplay();
        visualInventory.PartDoubleActivated.AddListener(AttemptToSell);
    }

    private void AttemptToSell(ModdedPart part)
    {
        int partCost = PartEconomy.GetCost(part) * visualInventory.CostMultiplier;
        if(tradeTarget.identity.PartCurrency < partCost) return;
        tradeTarget.identity.PartCurrency -= partCost;
        tradeTarget.visualInventory.AddPart(part);
        tradeTarget.UpdateDisplay();
        identity.PartCurrency += partCost;
        visualInventory.RemovePart(part);
        UpdateDisplay();
        saver.SavePlayerData();
    }

    void UpdateDisplay()
    {
        visualInventory.UpdatePartDisplays();
        currencyDisplay.text = identity.PartCurrency.ToString();
    }
}
