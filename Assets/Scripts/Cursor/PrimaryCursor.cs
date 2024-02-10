using System.Collections;
using System.Collections.Generic;
using static UnitControl;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public enum CursorState
{
    FREE,
    SPACELOCKED,
    UNITSNAPPED,
}

public enum CursorType
{
    AIR,
    GROUND
}
public class PrimaryCursor : MonoBehaviour
{
    [SerializeField] CursorMapping[] mappings;

    static Dictionary<CursorType, CursorBehaviour> behaviours;
    static CursorBehaviour ActiveBehaviour;
    
    public static Transform Transform;
    public static CursorState State;
    public static TinyBot TargetedBot;

    [SerializeField] UnitControl abilityUI;
    [SerializeField] StatDisplay statDisplay;
    [SerializeField] LineRenderer pathingLine;
    [SerializeField] GameObject numRotator;
    [SerializeField] TMP_Text moveCostPreview;

    static StatDisplay StatDisplay;
    static UnitControl AbilityUI;

    static bool pathing = false;
    private void Awake()
    {
        TinyBot.ClearActiveBot.AddListener(() => pathingLine.positionCount = 0);
    }
    private void Start()
    {
        behaviours = new();
        Transform = transform;
        AbilityUI = abilityUI;
        StatDisplay = statDisplay;
        foreach(var mapping in mappings) behaviours.Add(mapping.type, mapping.behaviour);
        SetCursorMode(CursorType.AIR);
    }

    Vector3Int lastPosition;
    List<Vector3> currentPath;
    float currentDistance;
    private void Update()
    {

        //clamp the cursor's position within the bounds of the map~~~~~~~~~~~~~~~~~~~~~
        if (Input.GetMouseButtonDown(0))
        {
            if (ActiveSkill != null)
            {
                if (!ActiveBot.AttemptToSpendResource(ActiveSkill.cost, StatType.ACTION)) return;
                StatDisplay.SyncStatDisplay(ActiveBot);
                ActiveSkill.ExecuteAbility(ActiveBot, transform.position);
                ActiveSkill = null;
                ClickableAbility.clearActive.Invoke();
                return;
            }
            else if (TargetedBot != null)
            {
                SelectBot(TargetedBot);
            }
        }

        if (State != CursorState.FREE) return;
        ActiveBehaviour.ControlCursor();

        if (ActiveBot == null) return;

        if (ActiveSkill == null && !pathing)
        {
            Vector3Int currentPosition = Vector3Int.RoundToInt(transform.position);
            if (currentPosition != lastPosition)
            {
                lastPosition = currentPosition;
                currentPath = Pathfinder3D.FindVectorPath(currentPosition, out currentDistance);
                if (currentPath == null) return;
                pathingLine.positionCount = currentPath.Count;
                pathingLine.SetPositions(currentPath.ToArray());
                
            }
        }
        if(currentPath == null)
        {
            numRotator.SetActive(false);
            return;
        }
        ShowMovePreview();
        if (Input.GetMouseButtonDown(1))
        {
            if (!ActiveBot.AttemptToSpendResource(currentDistance, StatType.MOVEMENT)) return;
            StartCoroutine(TraversePath());
        }
    }

    void ShowMovePreview()
    {
        numRotator.SetActive(true);
        moveCostPreview.text = Mathf.RoundToInt(currentDistance).ToString() + " ft";
    }

    private IEnumerator TraversePath()
    {
        pathing = true;
        yield return StartCoroutine(ActiveBot.PrimaryMovement.PathToPoint(ActiveBot, currentPath));
        StatDisplay.SyncStatDisplay(ActiveBot);
        Pathfinder3D.GeneratePathingTree(ActiveBot.PrimaryMovement.MoveStyle, Vector3Int.RoundToInt(ActiveBot.transform.position));
        pathing = false;
    }

    public static void SelectBot(TinyBot bot)
    {
        if (!bot.availableForTurn) return;
        TinyBot.ClearActiveBot.Invoke();
        bot.BecomeActiveUnit();
        SetCursorMode(bot.PrimaryMovement.PreferredCursor);
        Pathfinder3D.GeneratePathingTree(bot.PrimaryMovement.MoveStyle, Vector3Int.RoundToInt(bot.transform.position));
        AbilityUI.ShowControlForUnit(bot);
        StatDisplay.SyncStatDisplay(bot);
    }

    public static void SetCursorMode(CursorType type)
    {
        CursorBehaviour.Reset.Invoke();
        ActiveBehaviour = behaviours[type];
        ActiveBehaviour.ActivateCursor();
    }

    static bool canUnitSnap = true;
    public static void ToggleUnitSnap(TinyBot unit = null)
    {
        if (!canUnitSnap) return;
        TargetedBot = unit;
        if(unit == null && State == CursorState.UNITSNAPPED)
        {
            Transform.SetParent(null, true);
            State = CursorState.FREE;
        }
        else if(State == CursorState.FREE)
        {
            State = CursorState.UNITSNAPPED;
            Transform.SetParent(unit.transform, false);
            Transform.localPosition = Vector3.zero;
        }
    }

    [System.Serializable]
    class CursorMapping
    {
        public CursorType type;
        public CursorBehaviour behaviour;
    }
}
