using System.Collections;
using System.Collections.Generic;
using static UnitControl;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum CursorState
{
    FREE,
    SPACELOCKED,
    UNITSNAPPED,
}
public class PrimaryCursor : MonoBehaviour
{

    public static PrimaryCursor Instance;


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

    public static bool LockoutPlayer { get; private set; }

    public static UnityEvent<TinyBot> PlayerSelectedBot = new();

    bool skillActive { get { return PlayerControlledBot != null && PlayerControlledBot.Caster.Ability != null; } }

    TinyBot activeEcho;
    private void Awake()
    {
        RestrictCursor(false);
        Instance = this;
        TinyBot.ClearActiveBot.AddListener(InvalidatePath);
        Transform = transform;
    }

    private void OnDestroy()
    {
        PlayerSelectedBot.RemoveAllListeners();
    }

    Vector3Int lastPosition;
    List<Vector3> currentPath;
    float currentPathCost;
    private void LateUpdate()
    {
        if (LockoutPlayer || MainCameraControl.CameraAnimating) 
        {
            InvalidatePath();
            Instance.cursorBehaviour.Hide();
            return; 
        }

        bool abilityActive = PlayerControlledBot.Caster.Ability != null;
        if (State == CursorState.FREE) Instance.cursorBehaviour.ControlCursor();
        ToggleInvalidIndicator();

        if (Input.GetMouseButtonDown(0)) ProcessClick();
        else if (Input.GetMouseButtonDown(1)) BotCaster.ClearCasting.Invoke();


        if (PlayerControlledBot != null && !abilityActive && PlayerControlledBot.Stats.Max[StatType.MOVEMENT] > 0)
        {
            GenerateMovePreview(Vector3Int.RoundToInt(transform.position));
        }
    }

    void ToggleInvalidIndicator()
    {
        
    }

    public void GenerateMovePreview(Vector3Int pathPosition, Vector3 echoFacing = default)
    {
        if (pathPosition == lastPosition) return;

        if (!Pathfinder3D.GetLandingPointBy(pathPosition, PlayerControlledBot.MoveStyle, out Vector3Int targetPosition)
            && !Pathfinder3D.GetBestApproachPath(pathPosition, pathSearchRadius, PlayerControlledBot.MoveStyle, out targetPosition)) 
            return;

        lastPosition = pathPosition;
        List<Vector3> possiblePath = Pathfinder3D.FindVectorPath(targetPosition, out List<float> distances);
        if (possiblePath == null || possiblePath.Count == 0) return;
        ProcessAndPreviewPath(possiblePath);
        activeEcho = PlayerControlledBot.BotEcho;
        if (currentPath.Count == 0)
        {
            activeEcho.gameObject.SetActive(false);
            return;
        }
        if (echoFacing == default) echoFacing = currentPath[^1] - (currentPath.Count > 1 ? currentPath[^2] : PlayerControlledBot.transform.position);
        activeEcho.PlaceAt(currentPath[^1], echoFacing);
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


        while (pathIndex < possiblePath.Count)
        {
            if (distances[pathIndex] < currentMove)
            {
                currentPath.Add(possiblePath[pathIndex]);
            }
            else if (redPath.Count == 0 && pathIndex > 0)
            {
                Vector3 direction = possiblePath[pathIndex] - possiblePath[pathIndex - 1];
                float extraMove = currentMove - distances[pathIndex - 1];
                direction.Normalize();
                Vector3 midPoint = possiblePath[pathIndex - 1] + direction * extraMove;
                currentPath.Add(midPoint);
                redPath.Add(midPoint);
                redPath.Add(possiblePath[pathIndex]);
            }
            else
            {
                redPath.Add(possiblePath[pathIndex]);

            }
            pathIndex++;
        }

        DrawPaths(distances, redPath);
    }

    private void DrawPaths(List<float> distances, List<Vector3> redPath)
    {
        int validCount = currentPath.Count;
        pathingLine.positionCount = validCount;
        pathingLine.SetPositions(currentPath.ToArray());

        currentPathCost = validCount > 0 ? distances[validCount - 1] : 0;
        redLine.positionCount = redPath.Count;
        redLine.SetPositions(redPath.ToArray());
        moveCostPreview.color = redPath.Count > 0 ? Color.red : Color.white;
    }

    private void ProcessClick()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        //if there is an ability active, 


        if (TargetedBot != null && TargetedBot.Allegiance == Allegiance.PLAYER && !skillActive)
        {
            TargetedBot.Select();
            InvalidatePath();
            Unsnap();
        }
        else StartCoroutine(MoveAndCast());
    }

    IEnumerator MoveAndCast()
    {
        if (skillActive)
        {
            if (!PlayerControlledBot.Caster.IsCastValid()) yield break;
            PlayerControlledBot.Caster.EndTracking();
        } 
        if (currentPath != null && PlayerControlledBot != null && currentPath.Count > 0)
        {
            PlayerControlledBot.SpendResource(Mathf.CeilToInt(currentPathCost), StatType.MOVEMENT);
            yield return TraversePath();
        }
        if (skillActive)
        {
            PlayerCastAbility();
        }
    }

    void PlayerCastAbility()
    {
        InvalidatePath();
        PlayerControlledBot.Caster.CastLoadedSkill();
        Instance.statDisplay.SyncStatDisplay(PlayerControlledBot);
        ClickableAbility.PlayerUsedAbility.Invoke();
    }

    List<float> GetPathDistances(List<Vector3> points)
    {
        List<float> output = new() { 0 };
        for(int i = 1; i < points.Count; i++)
        {
            output.Add(output[^1] + Vector3.Distance(points[i-1], points[i]));
        }
        return output;
    }

    public static void InvalidatePath()
    {
        if (Instance.activeEcho != null)
        {
            Instance.activeEcho.gameObject.SetActive(false);
            Instance.activeEcho = null;
        }

        Instance.currentPath = null;
        Instance.numRotator.SetActive(false);
        Instance.pathingLine.positionCount = 0;
        Instance.redLine.positionCount = 0;
    }

    private IEnumerator TraversePath()
    {
        TogglePlayerLockout(true);
        yield return StartCoroutine(PlayerControlledBot.PrimaryMovement.TraversePath(currentPath));
        InvalidatePath();
        Pathfinder3D.GeneratePathingTree(PlayerControlledBot.PrimaryMovement.Style, PlayerControlledBot.transform.position);
        TogglePlayerLockout(false);
    }

    public static void SnapToUnit(TinyBot unit)
    {
        if(State != CursorState.FREE) return;
        TargetedBot = unit;
        State = CursorState.UNITSNAPPED;
        Transform.position = unit.TargetPoint.position;
        Instance.cursorBehaviour.SnapToPosition(unit.TargetPoint.position);
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

    public static void TogglePlayerLockout(bool on)
    {
        LockoutPlayer = on;
    }
}
