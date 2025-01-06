using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartySidebar : MonoBehaviour
{
    [SerializeField] PartyPortrait[] portraits;
    [SerializeField] BotCrafter unitSwitcher;
    private void OnEnable()
    {
        List<BotCharacter> cores = SceneGlobals.PlayerData.CoreInventory;
        cores.PassDataToUI(portraits, EnablePortrait);
    }

    void EnablePortrait(BotCharacter core, PartyPortrait portrait)
    {
        portrait.Become(core, unitSwitcher.Enable);
    }
}
