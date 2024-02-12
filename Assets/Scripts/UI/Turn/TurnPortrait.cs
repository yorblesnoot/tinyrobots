using System.Collections;
using System.Collections.Generic;
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
    public void Become(TinyBot bot)
    {
        gameObject.SetActive(true);
        //animator.Play("Idle");
        
        if (bot.allegiance == Allegiance.PLAYER)
        {
            frame.color = allyColor;
            selectButton.onClick.AddListener(() => PrimaryCursor.SelectBot(bot));
        }
        else if(bot.allegiance == Allegiance.ENEMY)
        {
            frame.color = enemyColor;
        }
        cardPortrait.sprite = bot.portrait;
    }

    public void Clear()
    {
        selectButton.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }

    public void Die()
    {
        animator.Play("DropAway");
    }
}
