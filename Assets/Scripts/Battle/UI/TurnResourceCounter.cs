using PrimeTween;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnResourceCounter : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] List<Image> abilityPoints;
    [SerializeField] TMP_Text abilityCount;
    [SerializeField] StatType targetStat;


    TinyBot currentBot;

    private void Awake()
    {
        PrimaryCursor.PlayerSelectedBot.AddListener(SyncStatDisplay);
        ClickableAbility.RefreshUsability.AddListener(UpdateResourceDisplays);
    }

    public void SyncStatDisplay(TinyBot targetBot)
    {
        if(currentBot != null) currentBot.Stats.StatModified.RemoveListener(UpdateResourceDisplays);
        targetBot.Stats.StatModified.AddListener(UpdateResourceDisplays);
        currentBot = targetBot;      
        UpdateAbilityPoints();
    }

    private void UpdateAbilityPoints()
    {
        for (int i = 0; i < abilityPoints.Count; i++)
        {
            abilityPoints[i].gameObject.SetActive(i < currentBot.Stats.Current[targetStat]);
        }
        abilityCount.text = Mathf.RoundToInt(currentBot.Stats.Current[targetStat]).ToString();
    }


    public void UpdateResourceDisplays()
    {
        if (UnitControl.PlayerControlledBot == null) return;
        UpdateAbilityPoints();
    }

}
