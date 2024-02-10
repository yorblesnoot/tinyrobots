using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] List<TurnPortrait> turnPortraitList;
    [SerializeField] Camera headshotCam;
    [SerializeField] float activeUnitScaleFactor = 1.5f;

    float cardWidth;
    float cardHeight;

    int activeIndex = 0;

    List<TurnTaker> turnTakers = new();
    HashSet<TurnTaker> currentlyActive = new();
    private void Awake()
    {
        RectTransform rectTransform = turnPortraitList[0].GetComponent<RectTransform>();
        cardWidth = rectTransform.rect.width;
        cardHeight = rectTransform.rect.height;
    }
    public void AddTurnTaker(TinyBot bot)
    {
        TurnTaker turnTaker = new() { Bot = bot };
        turnPortraitList[0].Become(bot);
        turnTaker.turnPortrait = turnPortraitList[0];
        turnPortraitList.RemoveAt(0);
        
        turnTakers.Add(turnTaker);
    }

    public void BeginTurnSequence()
    {
        GetActiveBots();
    }

    void GetActiveBots()
    {
        Allegiance sequenceAllegiance = turnTakers[activeIndex].Bot.allegiance;
        while (turnTakers[activeIndex].Bot.allegiance == sequenceAllegiance)
        {
            turnTakers[activeIndex].Bot.BeginTurn();
            currentlyActive.Add(turnTakers[activeIndex]);
            activeIndex++;
        }
        ArrangePortraits(currentlyActive);
    }

    public void EndTurn(TinyBot bot)
    {
        TurnTaker botTurn = turnTakers.Where(taker => taker.Bot == bot).FirstOrDefault();
        currentlyActive.Remove(botTurn);
        bot.availableForTurn = false;
        TinyBot.ClearActiveBot.Invoke();
        if(currentlyActive.Count == 0)
        {
            if(activeIndex == turnTakers.Count) activeIndex = 0;
            GetActiveBots();
        }
    }

    void ArrangePortraits(HashSet<TurnTaker> active)
    {
        float currentX = 0f;
        foreach (TurnTaker turnTaker in turnTakers)
        {
            float width = cardWidth;
            float height = cardHeight;
            RectTransform portraitRect = turnTaker.turnPortrait.GetComponent<RectTransform>();
            if (active.Contains(turnTaker))
            {
                height *= activeUnitScaleFactor;
                width *= activeUnitScaleFactor;
            }
            float currentY = -height / 2;
            portraitRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            portraitRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            Vector3 newPosition = new(currentX - -width / 2, currentY, 0);
            portraitRect.transform.localPosition = newPosition;
            currentX += width;
        }
    }

    class TurnTaker
    {
        public TurnPortrait turnPortrait;
        public TinyBot Bot;
    }
}
