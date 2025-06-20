using System.Collections.Generic;

public interface ITrader 
{
    public List<ModdedPart> PartInventory { get; }
    public int PartCurrency { get; set; }
}
