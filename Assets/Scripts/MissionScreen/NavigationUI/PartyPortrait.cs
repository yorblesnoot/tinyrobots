using System;
using UnityEngine;
using UnityEngine.UI;

public class PartyPortrait : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] Button button;
    [SerializeField] HealthOverlay healthOverlay;
    public void Become(BotCore core, Action<BotCore> coreCallback)
    {
        portrait.sprite = core.CharacterPortrait;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => coreCallback(core));
        healthOverlay.UpdateHealth(Mathf.RoundToInt(core.HealthRatio * 100), 100);
    }
}
