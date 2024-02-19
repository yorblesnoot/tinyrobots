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

    TinyBot thisBot;
    public void Become(TinyBot bot)
    {
        thisBot = bot;
        gameObject.SetActive(true);
        //animator.Play("Idle");
        
        if (bot.allegiance == Allegiance.PLAYER)
        {
            frame.color = allyColor;
            selectButton.onClick.AddListener(SelectThroughPortrait);
        }
        else if(bot.allegiance == Allegiance.ENEMY)
        {
            frame.color = enemyColor;
        }
        cardPortrait.sprite = bot.portrait;
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

    public void Die()
    {
        animator.Play("DropAway");
    }
}
