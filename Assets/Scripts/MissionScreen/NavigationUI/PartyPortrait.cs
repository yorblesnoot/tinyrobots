using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class PartyPortrait : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] Button button;
    [SerializeField] HealthOverlay healthOverlay;
    [SerializeField] GameObject weightOverlay;
    BotCore activeCore;
    public void Become(BotCore core, Action<BotCore> coreCallback)
    {
        activeCore = core;
        portrait.sprite = core.CharacterPortrait;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => coreCallback(core));
        UpdateHealthOverlay();
        weightOverlay.SetActive(!core.Deployable);
        core.HealthRatioChanged.AddListener(UpdateHealthOverlay);
    }

    void UpdateHealthOverlay()
    {
        healthOverlay.UpdateHealth(Mathf.RoundToInt(activeCore.HealthRatio * 100), 100);
    }
    private void OnDisable()
    {
        activeCore.HealthRatioChanged.RemoveListener(UpdateHealthOverlay);
    }
}
