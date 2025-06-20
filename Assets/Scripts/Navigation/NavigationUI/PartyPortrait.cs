using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyPortrait : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] Button button;
    [SerializeField] HealthOverlay healthOverlay;
    [SerializeField] List<Image> manaPips;
    BotCharacter activeCore;
    public void Become(BotCharacter core, Action<BotCharacter> coreCallback)
    {
        activeCore = core;
        portrait.sprite = core.CharacterPortrait;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => coreCallback(core));
        UpdateHealthOverlay();
        core.HealthRatio.OnChange.AddListener(UpdateHealthOverlay);
        core.Mana.OnChange.AddListener(SetManaPips);
        SetManaPips();
    }

    void UpdateHealthOverlay()
    {
        healthOverlay.UpdateHealth(Mathf.RoundToInt(activeCore.HealthRatio.Value * 100), 100);
    }
    private void OnDisable()
    {
        if(activeCore != null) activeCore.HealthRatio.OnChange.RemoveListener(UpdateHealthOverlay);
    }

    void SetManaPips()
    {
        for(int i = 0; i < activeCore.Mana.Value;  i++)
        {
            manaPips[i].gameObject.SetActive(i < activeCore.Mana.Value);
        }
    }
}
