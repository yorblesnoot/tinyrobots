using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyPortrait : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] Button button;
    [SerializeField] HealthOverlay healthOverlay;
    [SerializeField] GameObject weightOverlay;
    [SerializeField] List<Image> manaPips;
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
        SetManaPips();
    }

    void UpdateHealthOverlay()
    {
        healthOverlay.UpdateHealth(Mathf.RoundToInt(activeCore.HealthRatio * 100), 100);
    }
    private void OnDisable()
    {
        if(activeCore != null) activeCore.HealthRatioChanged.RemoveListener(UpdateHealthOverlay);
    }

    void SetManaPips()
    {
        for(int i = 0; i < activeCore.Mana;  i++)
        {
            manaPips[i].gameObject.SetActive(i < activeCore.Mana);
        }
    }
}
