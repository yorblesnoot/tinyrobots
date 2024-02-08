using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    [SerializeField] List<Image> abilityPoints;
    [SerializeField] TMP_Text abilityCount;

    [SerializeField] Slider moveSlider;
    [SerializeField] TMP_Text moveCount;

    TinyBot currentBot;

    public void SyncStatDisplay(TinyBot bot)
    {
        for(int i = 0; i < abilityPoints.Count; i++)
        {
            abilityPoints[i].gameObject.SetActive(i < bot.Stats.Current[StatType.ACTION]);
        }
        abilityCount.text = Mathf.RoundToInt(bot.Stats.Current[StatType.ACTION]).ToString();

        moveSlider.gameObject.SetActive(true);
        moveSlider.maxValue = bot.Stats.Max[StatType.MOVEMENT];
        moveSlider.value = bot.Stats.Current[StatType.MOVEMENT];
        moveCount.text = Mathf.RoundToInt(bot.Stats.Current[StatType.MOVEMENT]).ToString();
    }
}
