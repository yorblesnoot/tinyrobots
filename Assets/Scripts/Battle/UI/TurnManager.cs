using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Singleton;

    [SerializeField] List<TurnPortrait> turnPortraitList;
    [SerializeField] PortraitGenerator portraitGenerator;
    [SerializeField] float activeUnitScaleFactor = 1.5f;
    [SerializeField] float summonScaleFactor = .7f;
    [SerializeField] BattleEnder battleEnder;
    
    static List<TurnPortrait> portraitStock;
    static float cardWidth;
    static float cardHeight;
    static int activeIndex = 0;

    public static List<TinyBot> TurnTakers;
    public static Mission Mission;
    static Dictionary<TinyBot, TurnPortrait> activePortraits;
    static List<TinyBot> currentlyActive;
    static HashSet<TinyBot> summoned;

    public static UnityEvent RoundEnded;
    private void Awake()
    {
        RoundEnded = new();
        summoned = new();
        TurnTakers = new();
        activeIndex = 0;
        TurnTakers = new(); activePortraits = new(); currentlyActive = new();
        portraitStock = turnPortraitList;
        Singleton = this;
        RectTransform rectTransform = turnPortraitList[0].GetComponent<RectTransform>();
        cardWidth = rectTransform.rect.width;
        cardHeight = rectTransform.rect.height;
    }
    public static void AddTurnTaker(TinyBot bot, int index = 0)
    {
        Singleton.portraitGenerator.AttachPortrait(bot);
        portraitStock[0].Become(bot);
        activePortraits.Add(bot, portraitStock[0]);
        portraitStock.RemoveAt(0);
        if(index > 0) TurnTakers.Insert(index, bot);
        else TurnTakers.Add(bot);
    }

    public static void RegisterSummon(TinyBot bot)
    {
        AddTurnTaker(bot, activeIndex);
        summoned.Add(bot);
        if (bot.Allegiance == Allegiance.PLAYER)
        {
            activeIndex++;
            currentlyActive.Add(bot);
            bot.BeginTurn();
        }
        ArrangePortraits(currentlyActive);
    }

    public static void RemoveTurnTaker(TinyBot bot)
    {
        currentlyActive.Remove(bot);
        summoned.Remove(bot);
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
        portraitStock.Add(removed);
        ArrangePortraits(currentlyActive);
    }

    public static void BeginTurnSequence(bool select = true)
    {
        TurnTakers = TurnTakers.OrderByDescending<TinyBot, int>(SceneGlobals.PlayerData.DevMode ? PlayerFirstOrder : BotInitOrder).ToList();
        StartNewRound();
        QueueNextTurnTaker(true);
    }

    static int BotInitOrder(TinyBot bot)
    {
        return bot.Stats.Max[StatType.INITIATIVE];
    }

    static int PlayerFirstOrder(TinyBot bot)
    {
        return bot.Allegiance == Allegiance.PLAYER ? 10 : 0;
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
        QueueNextTurnTaker();
    }

    

    private static void QueueNextTurnTaker(bool initial = true)
    {
        if (currentlyActive.Count == 0)
        {
            if (activeIndex == TurnTakers.Count) StartNewRound();
            GetActiveBots();
        }
        TinyBot next = currentlyActive.First();
        MainCameraControl.CutToEntity(next.TargetPoint);
        if (initial) PrimaryCursor.SelectBot(next);
    }

    private static void StartNewRound()
    {
        RoundEnded.Invoke();
        activeIndex = 0;
        foreach(var portrait in activePortraits.Values) portrait.ToggleGrayOut(false);
    }

    static void ArrangePortraits(List<TinyBot> active)
    {
        float currentX = 0f;
        foreach(var portrait in portraitStock) portrait.gameObject.SetActive(false);
        foreach (TinyBot turnTaker in TurnTakers)
        {
            float width = cardWidth;
            float height = cardHeight;
            RectTransform portraitRect = activePortraits[turnTaker].GetComponent<RectTransform>();
            portraitRect.gameObject.SetActive(true);
            if (active.Contains(turnTaker))
            {
                height *= Singleton.activeUnitScaleFactor;
                width *= Singleton.activeUnitScaleFactor;
            }
            if(summoned.Contains(turnTaker))
            {
                height *= Singleton.summonScaleFactor;
                width *= Singleton.summonScaleFactor;
            }
            
            float currentY = -height / 2;
            portraitRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            portraitRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            Vector3 newPosition = new(currentX + width / 2, currentY, 0);
            portraitRect.transform.localPosition = newPosition;
            currentX += width;
        }
    }

    
}
