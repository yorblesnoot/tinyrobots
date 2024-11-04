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
    [SerializeField] Color activeColor;

    [SerializeField] Animator animator;
    [SerializeField] GameObject grayOut;
    [SerializeField] HealthOverlay healthOverlay;

    TinyBot thisBot;
    public void Become(TinyBot bot)
    {
        thisBot = bot;
        PrimaryCursor.PlayerSelectedBot.AddListener(HighlightWhenActive);
        selectButton.onClick.AddListener(SelectThroughPortrait);
        bot.Stats.StatModified.AddListener(UpdateHealth);
        SetColorForAllegiance(bot);
        cardPortrait.sprite = bot.Portrait;
        healthOverlay.UpdateHealth(thisBot);
    }

    private void SetColorForAllegiance(TinyBot bot)
    {
        if (bot.Allegiance == Allegiance.PLAYER) frame.color = allyColor;
        else if (bot.Allegiance == Allegiance.ENEMY) frame.color = enemyColor;
    }

    void HighlightWhenActive(TinyBot bot)
    {
        if(bot == thisBot) frame.color = activeColor;
        else SetColorForAllegiance(thisBot);
    }

    void SelectThroughPortrait()
    {
        PrimaryCursor.SelectBot(thisBot);
        MainCameraControl.CutToEntity(thisBot.TargetPoint);
    }

    public void Clear()
    {
        thisBot.Stats.StatModified.RemoveListener(UpdateHealth);
        PrimaryCursor.PlayerSelectedBot.RemoveListener(HighlightWhenActive);
        selectButton.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
        thisBot = null;
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
