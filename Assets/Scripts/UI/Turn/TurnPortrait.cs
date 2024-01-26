using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPortrait : MonoBehaviour
{

    [SerializeField] Button selectButton;
    [SerializeField] Image cardPortrait;
    public void Become(TinyBot bot)
    {
        gameObject.SetActive(true);
        selectButton.onClick.AddListener(() => PrimaryCursor.SelectBot(bot));
        cardPortrait.sprite = bot.portrait;
    }

    public void Clear()
    {
        selectButton.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
    }
}
