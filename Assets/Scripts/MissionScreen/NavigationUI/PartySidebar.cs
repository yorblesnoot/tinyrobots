using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartySidebar : MonoBehaviour
{
    [SerializeField] PartyPortrait[] portraits;
    [SerializeField] PlayerData playerData;
    [SerializeField] UnitSwitcher unitSwitcher;
    private void OnEnable()
    {
        List<BotCore> cores = playerData.CoreInventory;
        cores.PassDataToUI(portraits, EnablePortrait);
    }

    void EnablePortrait(BotCore core, PartyPortrait portrait)
    {
        portrait.Become(core, unitSwitcher.Enable);
    }
}
