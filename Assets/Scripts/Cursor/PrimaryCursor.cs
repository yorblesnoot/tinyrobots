using System.Collections;
using System.Collections.Generic;
using static UnitControl;
using UnityEngine;
using TMPro;

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

    public static bool actionInProgress = false;
    private void Awake()
    {
        actionInProgress = false;
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
        bool anAbilityIsActive = ClickableAbility.Active != null;

        //clamp the cursor's position within the bounds of the map~~~~~~~~~~~~~~~~~~~~~
        if (State == CursorState.FREE) ActiveBehaviour.ControlCursor();

        if (Input.GetMouseButtonDown(0))
        {
            if (anAbilityIsActive)
            {
                Ability skill = ClickableAbility.Active.Skill;
                if (skill.IsUsable(ActiveBot.transform.position)
                && ActiveBot.AttemptToSpendResource(skill.cost, StatType.ACTION))
                {
                    StatDisplay.SyncStatDisplay(ActiveBot);
                    StartCoroutine(skill.Execute());
                    ClickableAbility.Deactivate();
                }
            }
            else if (currentPath != null && ActiveBot != null && ActiveBot.AttemptToSpendResource(currentDistance, StatType.MOVEMENT)) StartCoroutine(TraversePath());
            else if (TargetedBot != null)
            {
                SelectBot(TargetedBot);
            }
        }
        else if (Input.GetMouseButtonDown(1)) ClickableAbility.Deactivate();

        
        if (!actionInProgress && ActiveBot != null)
        {
            Vector3Int currentPosition = Vector3Int.RoundToInt(transform.position);
            if (currentPosition != lastPosition)
            {
                lastPosition = currentPosition;
                currentPath = Pathfinder3D.FindVectorPath(currentPosition, out currentDistance);
                
            }
        }
        if (ActiveBot == null || currentPath == null || actionInProgress || anAbilityIsActive)
        {
            HideMovePreview();
        }
        else
        {
            ShowMovePreview();
        }
    }

    void ShowMovePreview()
    {
        numRotator.SetActive(true);
        moveCostPreview.text = Mathf.RoundToInt(currentDistance).ToString() + " ft";
        pathingLine.positionCount = currentPath.Count;
        pathingLine.SetPositions(currentPath.ToArray());
    }

    void HideMovePreview()
    {
        numRotator.SetActive(false);
        pathingLine.positionCount = 0;
    }

    private IEnumerator TraversePath()
    {
        actionInProgress = true;
        yield return StartCoroutine(ActiveBot.PrimaryMovement.PathToPoint(currentPath));
        StatDisplay.SyncStatDisplay(ActiveBot);
        Pathfinder3D.GeneratePathingTree(ActiveBot.PrimaryMovement.Style, Vector3Int.RoundToInt(ActiveBot.transform.position));
        actionInProgress = false;
    }

    public static void SelectBot(TinyBot bot)
    {
        if (!bot.availableForTurn) return;
        TinyBot.ClearActiveBot.Invoke();
        bot.BecomeActiveUnit();
        SetCursorMode(bot.PrimaryMovement.PreferredCursor);
        Pathfinder3D.GeneratePathingTree(bot.PrimaryMovement.Style, Vector3Int.RoundToInt(bot.transform.position));
        AbilityUI.ShowControlForUnit(bot);
        StatDisplay.SyncStatDisplay(bot);
    }

    public static void SetCursorMode(CursorType type)
    {
        CursorBehaviour.Reset.Invoke();
        ActiveBehaviour = behaviours[type];
        ActiveBehaviour.ActivateCursor();
    }
    public static void SnapToUnit(TinyBot unit)
    {
        if(State != CursorState.FREE) return;
        TargetedBot = unit;
        State = CursorState.UNITSNAPPED;
        Transform.position = unit.ChassisPoint.position;
    }
    public static void Unsnap()
    {
        if (State != CursorState.UNITSNAPPED) return;
        State = CursorState.FREE;
    }

    [System.Serializable]
    class CursorMapping
    {
        public CursorType type;
        public CursorBehaviour behaviour;
    }
}
