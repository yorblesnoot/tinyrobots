using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TurnResourceCounter : MonoBehaviour
{
    [SerializeField] float durationPerMove = .3f;


    [Header("Components")]
    [SerializeField] List<Image> abilityPoints;
    [SerializeField] TMP_Text abilityCount;

    [SerializeField] Slider moveSlider;
    [SerializeField] TMP_Text moveCount;

    TinyBot currentBot;

    private void Awake()
    {
        PrimaryCursor.PlayerSelectedBot.AddListener(SyncStatDisplay);
    }

    public void SyncStatDisplay(TinyBot targetBot)
    {
        if(currentBot != null) currentBot.Stats.StatModified.RemoveListener(UpdateResourceDisplays);
        targetBot.Stats.StatModified.AddListener(UpdateResourceDisplays);
        currentBot = targetBot;
        Tween.StopAll(moveSlider.value);
        
        UpdateAbilityPoints();

        moveSlider.gameObject.SetActive(true);
        moveSlider.maxValue = targetBot.Stats.Max[StatType.MOVEMENT] * 2;
        SetSliderAndNumber(targetBot.Stats.Current[StatType.MOVEMENT]);

    }

    private void UpdateAbilityPoints()
    {
        for (int i = 0; i < abilityPoints.Count; i++)
        {
            abilityPoints[i].gameObject.SetActive(i < currentBot.Stats.Current[StatType.ACTION]);
        }
        abilityCount.text = Mathf.RoundToInt(currentBot.Stats.Current[StatType.ACTION]).ToString();
    }

    void SetSliderAndNumber(float value)
    {
        value = Mathf.Max(value, 0);
        moveSlider.value = value;
        moveCount.text = Mathf.RoundToInt(value).ToString();
    }

    public void UpdateResourceDisplays()
    {
        if (UnitControl.PlayerControlledBot == null) return;
        UpdateAbilityPoints();
        AnimateMoveBar();
    }

    void AnimateMoveBar()
    {
        float difference = currentBot.Stats.Current[StatType.MOVEMENT] - moveSlider.value;
        if (difference == 0) return;
        float duration = Mathf.Abs(difference / currentBot.Movement.FinalSpeed);
        Tween.Custom(moveSlider.value, currentBot.Stats.Current[StatType.MOVEMENT], duration, SetSliderAndNumber);
    }
}
