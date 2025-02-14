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
        TinyBot.ClearActiveBot.AddListener(DeselectVisual);
        PrimaryCursor.PlayerSelectedBot.AddListener(SelectVisual);
    }
    public void SelectVisual(TinyBot bot)
    {
        gameObject.SetActive(true);
        transform.SetParent(bot.transform);
        transform.position = bot.TargetPoint.position;
    }

    void DeselectVisual()
    {
        transform.SetParent(null);
        gameObject.layer = overlayLayer;
        gameObject.SetActive(false);
    }
}
