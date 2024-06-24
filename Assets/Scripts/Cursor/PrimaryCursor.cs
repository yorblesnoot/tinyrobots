using System.Collections;
using System.Collections.Generic;
using static UnitControl;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

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
    [SerializeField] TurnResourceCounter statDisplay;
    [SerializeField] LineRenderer pathingLine;
    [SerializeField] LineRenderer redLine;
    [SerializeField] GameObject numRotator;
    [SerializeField] TMP_Text moveCostPreview;

    static TurnResourceCounter StatDisplay;
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
    float currentPathCost;
    private void LateUpdate()
    {
        bool abilityActive = ClickableAbility.Active != null;

        //clamp the cursor's position within the bounds of the map~~~~~~~~~~~~~~~~~~~~~
        if (State == CursorState.FREE) ActiveBehaviour.ControlCursor();
        if (actionInProgress) 
        {
            HideMovePreview();
            return; 
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            ProcessClick(abilityActive);
        }
        else if (Input.GetMouseButtonDown(1)) ClickableAbility.Cancel();


        if (PlayerControlledBot != null)
        {
            GenerateMovePreview();
        }
        if (PlayerControlledBot == null || abilityActive)
        {
            HideMovePreview();
        }
    }

    private void GenerateMovePreview()
    {
        bool foundValidSpot = Pathfinder3D.GetLandingPointBy(transform.position, PlayerControlledBot.PrimaryMovement.Style, out Vector3Int currentPosition);
        //Vector3Int currentPosition = Vector3Int.RoundToInt(transform.position);
        if (!foundValidSpot || currentPosition == lastPosition) return;

        lastPosition = currentPosition;
        List<Vector3> possiblePath = Pathfinder3D.FindVectorPath(currentPosition, out List<float> distances);
        if (possiblePath == null || possiblePath.Count == 0) return;
        ProcessAndPreviewPath(possiblePath, distances);
    }

    private void ProcessClick(bool anAbilityIsActive)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (anAbilityIsActive)
        {
            Ability skill = ClickableAbility.Active.Skill;
            if (skill.IsUsable(transform.position)
            && skill.cost <= PlayerControlledBot.Stats.Current[StatType.ACTION])
            {
                PlayerControlledBot.SpendResource(skill.cost, StatType.ACTION);
                StatDisplay.SyncStatDisplay(PlayerControlledBot);

                StartCoroutine(UseSkill(skill));
                ClickableAbility.Active.UpdateCooldowns();
                ClickableAbility.DeactivateSelectedAbility();
            }
        }
        else if (TargetedBot != null)
        {
            SelectBot(TargetedBot);
            HideMovePreview();
            Unsnap();
        }
        //traverse a confirmed path
        else if (currentPath != null && PlayerControlledBot != null)
        {
            if (currentPath.Count == 0) return;
            PlayerControlledBot.SpendResource(Mathf.CeilToInt(currentPathCost), StatType.MOVEMENT);
            TurnResourceCounter.Update.Invoke();
            StartCoroutine(TraversePath());
        }
    }

    IEnumerator UseSkill(Ability ability)
    {
        yield return StartCoroutine(ability.Execute());
        ClickableAbility.playerUsedAbility?.Invoke();
    }

    void ProcessAndPreviewPath(List<Vector3> possiblePath, List<float> distances)
    {
        numRotator.SetActive(true);
        numRotator.transform.SetParent(null);
        numRotator.transform.position = possiblePath[^1];
        moveCostPreview.text = Mathf.CeilToInt(distances[^1]).ToString() + " ft";
        currentPath = new();
        List<Vector3> redPath = new();
        int pathIndex = 0;
        float currentMove = PlayerControlledBot.Stats.Current[StatType.MOVEMENT];

        while (pathIndex < possiblePath.Count && distances[pathIndex] < currentMove)
        {
            currentPath.Add(possiblePath[pathIndex]);
            pathIndex++;            
        }
        if(pathIndex > 0 && pathIndex < possiblePath.Count)
        {
            float extraMove = distances[pathIndex] - currentMove;
            Vector3 offset = possiblePath[pathIndex] - possiblePath[pathIndex - 1]; 
            offset.Normalize();
            Vector3 midPoint = possiblePath[pathIndex - 1] + offset * extraMove;
            currentPath.Add(midPoint);
            redPath.Add(midPoint);
        }
        while (pathIndex < possiblePath.Count && distances[pathIndex] > currentMove)
        {
            redPath.Add(possiblePath[pathIndex]);
            pathIndex++;
        }

        pathingLine.positionCount = currentPath.Count;
        pathingLine.SetPositions(currentPath.ToArray());
        
        currentPathCost = currentPath.Count > 0 ? distances[^1] : 0;
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
        yield return StartCoroutine(PlayerControlledBot.PrimaryMovement.TraversePath(currentPath));
        Pathfinder3D.GeneratePathingTree(PlayerControlledBot);
        actionInProgress = false;
    }

    public static void SelectBot(TinyBot bot)
    {
        if (!bot.availableForTurn) return;
        TinyBot.ClearActiveBot.Invoke();
        bot.BecomeActiveUnit();
        SetCursorMode(bot.PrimaryMovement.PreferredCursor);
        Pathfinder3D.GeneratePathingTree(bot);
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
