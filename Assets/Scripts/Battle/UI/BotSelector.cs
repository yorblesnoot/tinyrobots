using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSelector : MonoBehaviour
{
    int overlayLayer;
    private void Awake()
    {
        gameObject.SetActive(false);
        overlayLayer = gameObject.layer;
        TinyBot.ClearActiveBot.AddListener(Deselect);
        PrimaryCursor.PlayerSelectedBot.AddListener(Select);
    }
    public void Select(TinyBot bot)
    {
        gameObject.SetActive(true);
        transform.SetParent(bot.transform);
        transform.position = bot.TargetPoint.position;
    }

    void Deselect()
    {
        transform.SetParent(null);
        gameObject.layer = overlayLayer;
        gameObject.SetActive(false);
    }
}
