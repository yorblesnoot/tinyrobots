using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] List<TurnPortrait> turnPortraitList;
    [SerializeField] Camera headshotCam;
    [SerializeField] float activeUnitScaleFactor = 1.5f;

    float cardWidth;
    float cardHeight;

    List<TurnTaker> turnTakers = new();
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
        GetActiveBots(0);
    }

    void GetActiveBots(int index)
    {
        HashSet<TurnTaker> active = new();
        Allegiance sequenceAllegiance = turnTakers[index].Bot.allegiance;
        while (turnTakers[index].Bot.allegiance == sequenceAllegiance)
        {
            turnTakers[index].Bot.availableForTurn = true;
            active.Add(turnTakers[index]);
            index++;
        }
        ArrangePortraits(active);
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
