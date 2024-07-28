using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartySidebar : MonoBehaviour
{
    [SerializeField] PartyPortrait[] portraits;
    [SerializeField] UnitSwitcher unitSwitcher;
    private void OnEnable()
    {
        List<BotCore> cores = SceneGlobals.PlayerData.CoreInventory;
        cores.PassDataToUI(portraits, EnablePortrait);
    }

    void EnablePortrait(BotCore core, PartyPortrait portrait)
    {
        portrait.Become(core, unitSwitcher.Enable);
    }
}
