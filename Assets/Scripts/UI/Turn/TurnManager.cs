using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Singleton;

    [SerializeField] List<TurnPortrait> turnPortraitList;
    [SerializeField] Camera headshotCam;
    [SerializeField] float activeUnitScaleFactor = 1.5f;
    static float ActiveUnitScaleFactor = 1.5f;

    static List<TurnPortrait> PortraitStock;

    static float cardWidth;
    static float cardHeight;

    static int activeIndex = 0;

    public static List<TinyBot> TurnTakers;
    static Dictionary<TinyBot, TurnPortrait> activePortraits;
    static HashSet<TinyBot> currentlyActive;
    private void Awake()
    {
        ActiveUnitScaleFactor = activeUnitScaleFactor;
        TurnTakers = new(); activePortraits = new(); currentlyActive = new();
        PortraitStock = turnPortraitList;
        Singleton = this;
        RectTransform rectTransform = turnPortraitList[0].GetComponent<RectTransform>();
        cardWidth = rectTransform.rect.width;
        cardHeight = rectTransform.rect.height;
    }
    public void AddTurnTaker(TinyBot bot)
    {
        PortraitStock[0].Become(bot);
        activePortraits.Add(bot, PortraitStock[0]);
        PortraitStock.RemoveAt(0);
        
        TurnTakers.Add(bot);
    }

    public static void RemoveTurnTaker(TinyBot bot)
    {
        currentlyActive.Remove(bot);
        TurnTakers.Remove(bot);
        TurnPortrait removed = activePortraits[bot];
        removed.Die();
        activePortraits.Remove(bot);
        Singleton.StartCoroutine(RecyclePortrait(removed, 1.5f));
    }

    static IEnumerator RecyclePortrait(TurnPortrait removed, float wait)
    {
        yield return new WaitForSeconds(wait);
        removed.Clear();
        PortraitStock.Add(removed);
    }

    public void BeginTurnSequence()
    {
        GetActiveBots();
    }

    static void GetActiveBots()
    {
        AddActiveUnit();
        while (TurnTakers[activeIndex].allegiance == Allegiance.PLAYER)
        {
            AddActiveUnit();
        }
        ArrangePortraits(currentlyActive);

        static void AddActiveUnit()
        {
            TurnTakers[activeIndex].BeginTurn();
            currentlyActive.Add(TurnTakers[activeIndex]);
            activeIndex++;
        }
    }

    public static void EndTurn(TinyBot bot)
    {
        currentlyActive.Remove(bot);
        bot.availableForTurn = false;
        TinyBot.ClearActiveBot.Invoke();
        if(currentlyActive.Count == 0)
        {
            if(activeIndex == TurnTakers.Count) activeIndex = 0;
            GetActiveBots();
        }
    }

    static void ArrangePortraits(HashSet<TinyBot> active)
    {
        float currentX = 0f;
        foreach (TinyBot turnTaker in TurnTakers)
        {
            float width = cardWidth;
            float height = cardHeight;
            RectTransform portraitRect = activePortraits[turnTaker].GetComponent<RectTransform>();
            if (active.Contains(turnTaker))
            {
                height *= ActiveUnitScaleFactor;
                width *= ActiveUnitScaleFactor;
            }
            float currentY = -height / 2;
            portraitRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            portraitRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            Vector3 newPosition = new(currentX - -width / 2, currentY, 0);
            portraitRect.transform.localPosition = newPosition;
            currentX += width;
        }
    }
}
