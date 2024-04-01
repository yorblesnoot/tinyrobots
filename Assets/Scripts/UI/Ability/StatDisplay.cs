using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    [SerializeField] List<Image> abilityPoints;
    [SerializeField] TMP_Text abilityCount;

    [SerializeField] Slider moveSlider;
    [SerializeField] TMP_Text moveCount;

    TinyBot currentBot;

    public static UnityEvent Update = new();

    private void Awake()
    {
        Update.AddListener(UpdateMoveBar);
    }

    public void SyncStatDisplay(TinyBot bot)
    {
        StopAllCoroutines();
        currentBot = bot;
        for(int i = 0; i < abilityPoints.Count; i++)
        {
            abilityPoints[i].gameObject.SetActive(i < bot.Stats.Current[StatType.ACTION]);
        }
        abilityCount.text = Mathf.RoundToInt(bot.Stats.Current[StatType.ACTION]).ToString();

        moveSlider.gameObject.SetActive(true);
        moveSlider.maxValue = bot.Stats.Max[StatType.MOVEMENT] * 2;
        SetSliderAndNumber(bot.Stats.Current[StatType.MOVEMENT]);
        
    }

    void SetSliderAndNumber(float value)
    {
        moveSlider.value = value;
        moveCount.text = Mathf.RoundToInt(value).ToString();
    }

    public void UpdateMoveBar()
    {
        StartCoroutine(AnimateBar());
    }

    IEnumerator AnimateBar()
    {
        
        while(moveSlider.value != currentBot.Stats.Current[StatType.MOVEMENT])
        {
            float newValue = Mathf.Lerp(moveSlider.value, currentBot.Stats.Current[StatType.MOVEMENT], Time.deltaTime);
            SetSliderAndNumber(newValue);
            yield return null;
        }
    }
}
