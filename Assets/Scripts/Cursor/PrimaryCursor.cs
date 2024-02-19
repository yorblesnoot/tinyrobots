using System.Collections;
using System.Collections.Generic;
using static UnitControl;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

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
    [SerializeField] LineRenderer redLine;
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
    int currentPathCost;
    private void Update()
    {
        bool anAbilityIsActive = ClickableAbility.Active != null;

        //clamp the cursor's position within the bounds of the map~~~~~~~~~~~~~~~~~~~~~
        if (State == CursorState.FREE) ActiveBehaviour.ControlCursor();

        
        if (Input.GetMouseButtonDown(0))
        {
            //ability use
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
            else if (TargetedBot != null)
            {
                SelectBot(TargetedBot);
                HideMovePreview();
            }
            //traverse a confirmed path
            else if (currentPath != null && ActiveBot != null
                && currentPathCost <= ActiveBot.Stats.Current[StatType.MOVEMENT]
                && !EventSystem.current.IsPointerOverGameObject()
                && ActiveBot.AttemptToSpendResource(currentPathCost, StatType.MOVEMENT)) 
                StartCoroutine(TraversePath());
            
        }
        else if (Input.GetMouseButtonDown(1)) ClickableAbility.Deactivate();

        //generate new path
        if (!actionInProgress && ActiveBot != null)
        {
            Vector3Int currentPosition = Vector3Int.RoundToInt(transform.position);
            if (currentPosition != lastPosition)
            {
                lastPosition = currentPosition;
                List<Vector3> possiblePath = Pathfinder3D.FindVectorPath(currentPosition, out List<float> distances);
                if(possiblePath != null && possiblePath.Count > 0)
                {
                    ProcessAndPreviewPath(possiblePath, distances);
                }                
            }
        }
        //toggle path preview
        if (ActiveBot == null || actionInProgress || anAbilityIsActive)
        {
            HideMovePreview();
        }
    }

    void ProcessAndPreviewPath(List<Vector3> possiblePath, List<float> distances)
    {
        numRotator.SetActive(true);
        numRotator.transform.SetParent(null);
        numRotator.transform.position = possiblePath[^1];
        moveCostPreview.text = Mathf.RoundToInt(distances[^1]).ToString() + " ft";
        currentPath = new();
        List<Vector3> redPath = new();
        int pathIndex = 0;
        float currentMove = ActiveBot.Stats.Current[StatType.MOVEMENT];
        while (pathIndex < possiblePath.Count)
        {
            float distance = distances[pathIndex];
            
            if (distance <= currentMove + 1)
                currentPath.Add(possiblePath[pathIndex]);
            if (distance >= currentMove - 1)
                redPath.Add(possiblePath[pathIndex]);
            pathIndex++;
        }
        
        pathingLine.positionCount = currentPath.Count;
        pathingLine.SetPositions(currentPath.ToArray());
        
        currentPathCost = currentPath.Count > 0 ? Mathf.RoundToInt(distances[currentPath.Count-1]) : 0;
        redLine.positionCount = redPath.Count;
        redLine.SetPositions(redPath.ToArray());
        moveCostPreview.color = redPath.Count > 0 ? Color.red : Color.white;
    }

    void HideMovePreview()
    {
        numRotator.SetActive(false);
        pathingLine.positionCount = 0;
        redLine.positionCount = 0;
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
        TargetedBot = null;
        State = CursorState.FREE;
    }

    [System.Serializable]
    class CursorMapping
    {
        public CursorType type;
        public CursorBehaviour behaviour;
    }
}
