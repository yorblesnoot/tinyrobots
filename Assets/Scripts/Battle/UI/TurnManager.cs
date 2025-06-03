using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [SerializeField] GameObject turnPortrait;
    [SerializeField] PortraitGenerator portraitGenerator;
    [SerializeField] float activeUnitScaleFactor = 1.5f;
    [SerializeField] float summonScaleFactor = .7f;
    [SerializeField] BattleEnder battleEnder;
    [SerializeField] float cardWidth = 60;
    [SerializeField] float cardHeight = 60;
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
        Instance = this;
    }
    public static void AddTurnTaker(TinyBot bot, int index = 0)
    {
        Instance.portraitGenerator.AttachPortrait(bot);
        TurnPortrait portrait = Instantiate(Instance.turnPortrait, Instance.transform).GetComponent<TurnPortrait>();
        portrait.Become(bot);
        activePortraits.Add(bot, portrait);
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
            bot.BecomeAvailableForTurn();
        }
        Instance.StartCoroutine(DelayArrange());
        //ArrangePortraits(currentlyActive);
    }

    //this is necessary because of some weirdness with the layout group
    static IEnumerator DelayArrange()
    {
        yield return null;
        ArrangePortraits(currentlyActive);
    }

    public static void RemoveTurnTaker(TinyBot bot)
    {
        if (currentlyActive.Contains(bot)) EndTurn(bot);
        summoned.Remove(bot);
        if (TurnTakers.IndexOf(bot) < activeIndex) activeIndex--;
        TurnTakers.Remove(bot);
        TurnPortrait removed = activePortraits[bot];
        removed.Die();
        activePortraits.Remove(bot);
        Instance.StartCoroutine(RecyclePortrait(removed, 1.5f));
        
    }

    static IEnumerator RecyclePortrait(TurnPortrait removed, float wait)
    {
        yield return new WaitForSeconds(wait);
        removed.Clear();
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
            AddActiveUnit();
            ArrangePortraits(currentlyActive);
            foreach(var active in currentlyActive) active.BecomeAvailableForTurn();
        }
        currentlyActive.First().Select(true);

        static void AddActiveUnit()
        {
            TinyBot turnTaker = TurnTakers[activeIndex];
            currentlyActive.Add(TurnTakers[activeIndex]);
            activeIndex++;
            if (turnTaker.Allegiance == Allegiance.PLAYER
                && activeIndex < TurnTakers.Count
                && TurnTakers[activeIndex].Allegiance == Allegiance.PLAYER)
            {
                AddActiveUnit();
            }
        }
    }

    private static void StartNewRound()
    {
        RoundEnded.Invoke();
        activeIndex = 0;
        foreach(var portrait in activePortraits.Values) portrait.ToggleGrayOut(false);
    }

    static void ArrangePortraits(List<TinyBot> active)
    {
        foreach (TinyBot turnTaker in TurnTakers)
        {
            float width = Instance.cardWidth;
            float height = Instance.cardHeight;
            RectTransform portraitRect = activePortraits[turnTaker].GetComponent<RectTransform>();
            
            if (active.Contains(turnTaker))
            {
                height *= Instance.activeUnitScaleFactor;
                width *= Instance.activeUnitScaleFactor;
            }
            if(summoned.Contains(turnTaker))
            {
                height *= Instance.summonScaleFactor;
                width *= Instance.summonScaleFactor;
            }

            portraitRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            portraitRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            portraitRect.transform.SetAsLastSibling();
        }
    }

    
}
