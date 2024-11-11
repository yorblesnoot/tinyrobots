using System.Collections;
using System.Collections.Generic;
using static UnitControl;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Linq;

public enum CursorState
{
    FREE,
    SPACELOCKED,
    UNITSNAPPED,
}
public class PrimaryCursor : MonoBehaviour
{

    public static PrimaryCursor Instance;
    static CursorBehaviour activeBehaviour;
    
    
    public static Transform Transform;
    public static CursorState State;
    public static TinyBot TargetedBot;

    [SerializeField] int pathSearchRadius = 10;
    [SerializeField] Material validMaterial;
    [SerializeField] Material invalidMaterial;

    [Header("Components")]
    [SerializeField] CursorBehaviour cursorBehaviour;
    [SerializeField] TurnResourceCounter statDisplay;
    [SerializeField] LineRenderer pathingLine;
    [SerializeField] LineRenderer redLine;
    [SerializeField] GameObject numRotator;
    [SerializeField] TMP_Text moveCostPreview;
    [SerializeField] Renderer selectBubble;

    public static bool ActionInProgress = false;

    public static UnityEvent<TinyBot> PlayerSelectedBot = new();
    private void Awake()
    {
        RestrictCursor(false);
        Instance = this;
        ActionInProgress = false;
        TinyBot.ClearActiveBot.AddListener(InvalidatePath);
        activeBehaviour = cursorBehaviour;
        Transform = transform;
    }

    Vector3Int lastPosition;
    List<Vector3> currentPath;
    float currentPathCost;
    private void LateUpdate()
    {
        bool abilityActive = ClickableAbility.Activated != null;

        //clamp the cursor's position within the bounds of the map~~~~~~~~~~~~~~~~~~~~~
        if (State == CursorState.FREE) activeBehaviour.ControlCursor();
        ToggleInvalidIndicator();
        if (ActionInProgress) 
        {
            InvalidatePath();
            return; 
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            ProcessClick(abilityActive);
        }
        else if (Input.GetMouseButtonDown(1)) ClickableAbility.CancelAbility();


        if (PlayerControlledBot != null && !abilityActive && PlayerControlledBot.Stats.Max[StatType.MOVEMENT] > 0)
        {
            GenerateMovePreview();
        }
        else
        {
            InvalidatePath();
        }

    }

    void ToggleInvalidIndicator()
    {
        if(ClickableAbility.Activated == null || ClickableAbility.Activated.Ability.IsUsable())
        {
            selectBubble.material = validMaterial;
        }
        else
        {
            selectBubble.material = invalidMaterial;
        }
    }

    private void GenerateMovePreview()
    {
        Vector3Int cursorPosition = Vector3Int.RoundToInt(transform.position);
        if (cursorPosition == lastPosition) return;

        if (!Pathfinder3D.GetLandingPointBy(cursorPosition, PlayerControlledBot.MoveStyle, out Vector3Int targetPosition)
            && !Pathfinder3D.GetBestApproachPath(cursorPosition, pathSearchRadius, PlayerControlledBot.MoveStyle, out targetPosition)) 
            return;

        lastPosition = cursorPosition;
        List<Vector3> possiblePath = Pathfinder3D.FindVectorPath(targetPosition, out List<float> distances);
        if (possiblePath == null || possiblePath.Count == 0) return;
        ProcessAndPreviewPath(possiblePath);
    }

    private void ProcessClick(bool anAbilityIsActive)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (anAbilityIsActive)
        {
            ActiveAbility skill = ClickableAbility.Activated.Ability;
            if (skill.IsUsable()
            && skill.cost <= PlayerControlledBot.Stats.Current[StatType.ACTION])
            {
                PlayerControlledBot.SpendResource(skill.cost, StatType.ACTION);
                Instance.statDisplay.SyncStatDisplay(PlayerControlledBot);

                StartCoroutine(UseSkill(skill));
                ClickableAbility.EndUsableAbilityState();
            }
        }
        else if (TargetedBot != null && TargetedBot.Allegiance == Allegiance.PLAYER)
        {
            SelectBot(TargetedBot);
            InvalidatePath();
            Unsnap();
        }
        //traverse a confirmed path
        else if (currentPath != null && PlayerControlledBot != null)
        {
            if (currentPath.Count == 0) return;
            PlayerControlledBot.SpendResource(Mathf.CeilToInt(currentPathCost), StatType.MOVEMENT);
            StartCoroutine(TraversePath());
        }
    }

    IEnumerator UseSkill(ActiveAbility ability)
    {
        InvalidatePath();
        yield return StartCoroutine(ability.Execute());
        ClickableAbility.PlayerUsedAbility?.Invoke();
    }

    void ProcessAndPreviewPath(List<Vector3> possiblePath)
    {
        numRotator.SetActive(true);
        numRotator.transform.SetParent(null);
        possiblePath = PlayerControlledBot.PrimaryMovement.SanitizePath(possiblePath);
        //get the indices in the raw path of the new path points
        //then replace the old distance list with 
        List<float> distances = GetPathDistances(possiblePath);
        
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
        while (pathIndex < possiblePath.Count && distances[pathIndex] >= currentMove)
        {
            redPath.Add(possiblePath[pathIndex]);
            pathIndex++;
        }
        
        int validCount = currentPath.Count;
        pathingLine.positionCount = validCount;
        pathingLine.SetPositions(currentPath.ToArray());
        
        currentPathCost = validCount > 0 ? distances[validCount-1] : 0;
        redLine.positionCount = redPath.Count;
        redLine.SetPositions(redPath.ToArray());
        moveCostPreview.color = redPath.Count > 0 ? Color.red : Color.white;
    }

    List<float> GetPathDistances(List<Vector3> points)
    {
        List<float> output = new() { 0 };
        for(int i = 1; i < points.Count; i++)
        {
            output.Add(Vector3.Distance(points[i-1], points[i]));
        }
        return output;
    }

    void InvalidatePath()
    {
        currentPath = null;
        numRotator.SetActive(false);
        pathingLine.positionCount = 0;
        redLine.positionCount = 0;
    }

    private IEnumerator TraversePath()
    {
        ActionInProgress = true;
        yield return StartCoroutine(PlayerControlledBot.PrimaryMovement.TraversePath(currentPath));
        InvalidatePath();
        Pathfinder3D.GeneratePathingTree(PlayerControlledBot.PrimaryMovement.Style, PlayerControlledBot.transform.position);
        ActionInProgress = false;
    }

    public static void SelectBot(TinyBot bot)
    {
        if (!bot.AvailableForTurn) return;
        TinyBot.ClearActiveBot.Invoke();
        PlayerSelectedBot.Invoke(bot);
        
        MoveStyle botStyle = bot.PrimaryMovement.Style;
        Pathfinder3D.GeneratePathingTree(botStyle, bot.transform.position);
    }

    public static void SnapToUnit(TinyBot unit)
    {
        if(State != CursorState.FREE) return;
        TargetedBot = unit;
        State = CursorState.UNITSNAPPED;
        Transform.position = unit.TargetPoint.position;
        activeBehaviour.SnapToPosition(unit.TargetPoint.position);
    }
    public static void Unsnap()
    {
        if (State != CursorState.UNITSNAPPED) return;
        TargetedBot = null;
        State = CursorState.FREE;
    }

    public static void RestrictCursor(bool on = true)
    {
        Unsnap();
        State = on ? CursorState.SPACELOCKED : CursorState.FREE;
    }
}
