using System;
using UnityEngine;
using UnityEngine.UI;

public class PartyPortrait : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] Button button;
    [SerializeField] HealthOverlay healthOverlay;
    [SerializeField] GameObject weightOverlay;
    BotCharacter activeCore;
    public void Become(BotCharacter core, Action<BotCharacter> coreCallback)
    {
        activeCore = core;
        portrait.sprite = core.CharacterPortrait;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => coreCallback(core));
        UpdateHealthOverlay();
        weightOverlay.SetActive(!core.Energized);
        core.HealthRatioChanged.AddListener(UpdateHealthOverlay);
    }

    void UpdateHealthOverlay()
    {
        healthOverlay.UpdateHealth(Mathf.RoundToInt(activeCore.HealthRatio * 100), 100);
    }
    private void OnDisable()
    {
        if(activeCore != null) activeCore.HealthRatioChanged.RemoveListener(UpdateHealthOverlay);
    }
}
