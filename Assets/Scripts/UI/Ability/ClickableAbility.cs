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

    public static UnityEvent clearActive = new();
    private void Awake()
    {
        clearActive.AddListener(Deactivate);
    }

    private void Deactivate()
    {
        image.color = Color.white;
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
        
        AbilityUI.Active = thisAbility;
        thisAbility.ToggleTargetLine(true);
        image.color = Color.red;
    }
}
