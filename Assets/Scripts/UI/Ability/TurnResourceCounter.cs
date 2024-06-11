using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TurnResourceCounter : MonoBehaviour
{
    [SerializeField] List<Image> abilityPoints;
    [SerializeField] TMP_Text abilityCount;

    [SerializeField] Slider moveSlider;
    [SerializeField] TMP_Text moveCount;

    TinyBot currentBot;

    public static UnityEvent Update = new();

    private void Awake()
    {
        Update.AddListener(UpdateResourceDisplays);
    }

    public void SyncStatDisplay(TinyBot bot)
    {
        StopAllCoroutines();
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
        UpdateAbilityPoints();
        StartCoroutine(AnimateMoveBar());
    }

    IEnumerator AnimateMoveBar()
    {
        while(moveSlider.value != currentBot.Stats.Current[StatType.MOVEMENT])
        {
            float newValue = Mathf.Lerp(moveSlider.value, currentBot.Stats.Current[StatType.MOVEMENT], Time.deltaTime);
            SetSliderAndNumber(newValue);
            yield return null;
        }
    }
}
