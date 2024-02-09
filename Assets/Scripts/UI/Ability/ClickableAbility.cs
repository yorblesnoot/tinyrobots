using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClickableAbility : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Button button;
    [SerializeField] TMP_Text letter;
    Ability thisAbility;

    
    [SerializeField] Transform pipHolder;
    [SerializeField] List<Image> actionPoints;

    [SerializeField] float dislacementModifier;
    float pointWidth;

    public static UnityEvent clearActive = new();
    private void Awake()
    {
        clearActive.AddListener(Deactivate);
        pointWidth = actionPoints[0].GetComponent<RectTransform>().rect.width;
        pointWidth *= dislacementModifier;
    }

    private void Deactivate()
    {
        image.color = Color.white;
        if (thisAbility == null) return;
        thisAbility.ToggleTargetLine(false);
    }

    public void Become(Ability ability, KeyCode key)
    {
        gameObject.SetActive(true);
        thisAbility = ability;
        image.sprite = ability.icon;
        letter.text = key.ToString();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Activate);
        SetPips(ability.cost);
    }

    void SetPips(int pips)
    {
        for(int i = 0; i < actionPoints.Count; i++)
        {
            actionPoints[i].gameObject.SetActive(i < pips);
        }
        float newX = -pips * pointWidth / 2;
        Vector3 pos = pipHolder.transform.localPosition;
        pos.x = newX;
        pipHolder.transform.localPosition = pos;
    }

    public void Clear()
    {
        thisAbility = null;
        button.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        clearActive.Invoke();
        
        UnitControl.ActiveSkill = thisAbility;
        thisAbility.ToggleTargetLine(true);
        image.color = Color.red;
    }
}
