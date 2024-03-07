using System;
using UnityEngine;
using UnityEngine.UI;

public class TurnPortrait : MonoBehaviour
{

    [SerializeField] Button selectButton;
    [SerializeField] Image cardPortrait;
    [SerializeField] Image frame;
    [SerializeField] Color allyColor;
    [SerializeField] Color enemyColor;
    [SerializeField] Color neutralColor;

    [SerializeField] Animator animator;
    [SerializeField] GameObject grayOut;
    [SerializeField] HealthOverlay healthOverlay;

    TinyBot thisBot;
    public void Become(TinyBot bot)
    {
        thisBot = bot;
        gameObject.SetActive(true);
        selectButton.onClick.AddListener(SelectThroughPortrait);
        if (bot.allegiance == Allegiance.PLAYER)
        {
            frame.color = allyColor;
        }
        else if(bot.allegiance == Allegiance.ENEMY)
        {
            frame.color = enemyColor;
        }
        cardPortrait.sprite = bot.portrait;
        healthOverlay.UpdateHealth(thisBot);
    }

    void SelectThroughPortrait()
    {
        PrimaryCursor.SelectBot(thisBot);
        MainCameraControl.CutToUnit(thisBot);
    }

    public void Clear()
    {
        thisBot = null;
        selectButton.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }

    public void ToggleGrayOut(bool on)
    {
        grayOut.SetActive(on);
    }

    

    public void Die()
    {
        animator.Play("DropAway");
    }

    internal void UpdateHealth()
    {
        healthOverlay.UpdateHealth(thisBot);
    }
}
