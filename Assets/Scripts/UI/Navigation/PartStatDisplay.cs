using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartStatDisplay : MonoBehaviour
{

    [SerializeField] StatIcons[] icons;
    [SerializeField] TMP_Text amount;
    [SerializeField] Image displayedIcon;

    static Dictionary<StatType, StatIcons> statIcons;

    public void AssignStat(StatType type, int value)
    {
        gameObject.SetActive(true);
        statIcons ??= icons.ToDictionary(icon => icon.type, icon => icon);
        amount.text = value.ToString();
        displayedIcon.sprite = statIcons[type].icon;
        displayedIcon.color = statIcons[type].color;
        //amount.color = statIcons[type].color;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }


    [Serializable]
    class StatIcons
    {
        public StatType type;
        public Sprite icon;
        public Color color = Color.white;
    }
}