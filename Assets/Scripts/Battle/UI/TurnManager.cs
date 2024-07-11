using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Singleton;

    [SerializeField] List<TurnPortrait> turnPortraitList;
    [SerializeField] Camera headshotCam;
    [SerializeField] float activeUnitScaleFactor = 1.5f;
    [SerializeField] BattleEnder battleEnder;
    
    static float ActiveUnitScaleFactor = 1.5f;
    static List<TurnPortrait> PortraitStock;
    static float cardWidth;
    static float cardHeight;
    static int activeIndex = 0;

    public static List<TinyBot> TurnTakers;
    public static Mission Mission;
    static Dictionary<TinyBot, TurnPortrait> activePortraits;
    static List<TinyBot> currentlyActive;

    public static UnityEvent RoundEnded = new();
    private void Awake()
    {
        activeIndex = 0;
        ActiveUnitScaleFactor = activeUnitScaleFactor;
        TurnTakers = new(); activePortraits = new(); currentlyActive = new();
        PortraitStock = turnPortraitList;
        Singleton = this;
        RectTransform rectTransform = turnPortraitList[0].GetComponent<RectTransform>();
        cardWidth = rectTransform.rect.width;
        cardHeight = rectTransform.rect.height;
    }
    public static void AddTurnTaker(TinyBot bot)
    {
        PortraitStock[0].Become(bot);
        activePortraits.Add(bot, PortraitStock[0]);
        PortraitStock.RemoveAt(0);       
        TurnTakers.Add(bot);
    }

    public static void RemoveTurnTaker(TinyBot bot)
    {
        currentlyActive.Remove(bot);
        if (TurnTakers.IndexOf(bot) < activeIndex) activeIndex--;
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
        ArrangePortraits(currentlyActive);
    }

    public static void BeginTurnSequence()
    {
        TurnTakers.OrderByDescending(bot => bot.Stats.Max[StatType.INITIATIVE]);
        GetActiveBots();
    }

    public static void UpdateHealth(TinyBot bot)
    {
        if(activePortraits.TryGetValue(bot, out var portrait)) portrait.UpdateHealth();
    }

    static void GetActiveBots()
    {
        AddActiveUnit();
        ArrangePortraits(currentlyActive);

        static void AddActiveUnit()
        {
            TinyBot turnTaker = TurnTakers[activeIndex];
            currentlyActive.Add(TurnTakers[activeIndex]);
            turnTaker.BeginTurn();
            activeIndex++;
            if (turnTaker.Allegiance == Allegiance.PLAYER
                && activeIndex < TurnTakers.Count
                && TurnTakers[activeIndex].Allegiance == Allegiance.PLAYER)
            {
                AddActiveUnit();
            }
        }
    }

    public static void EndTurn(TinyBot bot)
    {
        currentlyActive.Remove(bot);
        activePortraits[bot].ToggleGrayOut(true);
        bot.AvailableForTurn = false;
        TinyBot.ClearActiveBot.Invoke();
        bot.EndedTurn.Invoke();

        if (Mission.MetEndCondition(Singleton, Singleton.battleEnder)) return;
        QueueNextTurnTaker();
    }

    private static void QueueNextTurnTaker()
    {
        if (currentlyActive.Count == 0)
        {
            if (activeIndex == TurnTakers.Count) StartNewRound();
            GetActiveBots();
        }

        TinyBot next = currentlyActive.First();
        MainCameraControl.CutToUnit(next);
        PrimaryCursor.SelectBot(next);
    }

    private static void StartNewRound()
    {
        RoundEnded.Invoke();
        Mission.RoundEnd();
        activeIndex = 0;
        foreach(var portrait in activePortraits.Values) portrait.ToggleGrayOut(false);
    }

    static void ArrangePortraits(List<TinyBot> active)
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
