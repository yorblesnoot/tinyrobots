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

    public static UnityEvent Update = new();

    private void Awake()
    {
        Update.AddListener(UpdateResourceDisplays);
        PrimaryCursor.PlayerSelectedBot.AddListener(SyncStatDisplay);
    }

    public void SyncStatDisplay(TinyBot bot)
    {
        Tween.StopAll(moveSlider.value);
        currentBot = bot;
        UpdateAbilityPoints();

        moveSlider.gameObject.SetActive(true);
        moveSlider.maxValue = bot.Stats.Max[StatType.MOVEMENT] * 2;
        SetSliderAndNumber(bot.Stats.Current[StatType.MOVEMENT]);

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
        float duration = Mathf.Abs(difference / currentBot.PrimaryMovement.FinalSpeed);
        Tween.Custom(moveSlider.value, currentBot.Stats.Current[StatType.MOVEMENT], duration, SetSliderAndNumber);
    }
}
