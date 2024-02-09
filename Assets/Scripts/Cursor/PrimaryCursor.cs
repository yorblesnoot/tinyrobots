using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CursorState
{
    FREE,
    SPACELOCKED,
    UNITSNAPPED,
}
public class PrimaryCursor : MonoBehaviour
{
    public static TinyBot SelectedBot;

    [SerializeField] CursorBehaviour[] cursorBehaviours;
    int activeCursorIndex;
    
    public static Transform Transform;
    public static CursorState State;
    public static TinyBot TargetedBot;

    [SerializeField] UnitControl abilityUI;
    [SerializeField] StatDisplay statDisplay;
    [SerializeField] LineRenderer pathingLine;

    static StatDisplay StatDisplay;
    static UnitControl AbilityUI;
    private void Awake()
    {
        Transform = transform;
        AbilityUI = abilityUI;
        StatDisplay = statDisplay;
        activeCursorIndex = 0;
    }

    Vector3Int lastPosition;
    List<Vector3> currentPath;
    private void Update()
    {

        //clamp the cursor's position within the bounds of the map~~~~~~~~~~~~~~~~~~~~~
        if (Input.GetMouseButtonDown(0))
        {
            if (UnitControl.ActiveSkill != null)
            {
                if (!SelectedBot.AttemptToSpendResource(UnitControl.ActiveSkill.cost, StatType.ACTION)) return;
                UnitControl.ActiveSkill.ExecuteAbility(SelectedBot, transform.position);
                UnitControl.ActiveSkill = null;
                ClickableAbility.clearActive.Invoke();
                return;
            }
            else if (TargetedBot != null)
            {
                SelectBot(TargetedBot);
            }
        }

        if (Input.GetKeyDown(KeyCode.G)) CycleCursorMode();

        if (State != CursorState.FREE) return;
        cursorBehaviours[activeCursorIndex].ControlCursor();

        if (SelectedBot == null) return;

        if (UnitControl.ActiveSkill == null)
        {
            Vector3Int currentPosition = Vector3Int.RoundToInt(transform.position);
            if (currentPosition != lastPosition)
            {
                lastPosition = currentPosition;
                currentPath = Pathfinder3D.FindVectorPath(currentPosition);
                if (currentPath == null) return;
                pathingLine.positionCount = currentPath.Count;
                pathingLine.SetPositions(currentPath.ToArray());
            }
            
        }
        if (Input.GetMouseButtonDown(1) && currentPath != null)
        {
            if (!SelectedBot.AttemptToSpendResource(UnitControl.ActiveSkill.cost, StatType.MOVEMENT)) return;
            StartCoroutine(SelectedBot.PrimaryMovement.PathToPoint(SelectedBot, currentPath));
        }
    }

    public static void SelectBot(TinyBot bot)
    {
        if (!bot.availableForTurn) return;
        if (SelectedBot != null) SelectedBot.BecomeActiveUnit(false);
        SelectedBot = bot;
        SelectedBot.BecomeActiveUnit(true);
        Pathfinder3D.GeneratePathingTree(bot.PrimaryMovement.MoveStyle, Vector3Int.RoundToInt(bot.transform.position));
        AbilityUI.ShowControlForUnit(bot);
        StatDisplay.SyncStatDisplay(bot);
    }

    void CycleCursorMode()
    {
        cursorBehaviours[activeCursorIndex].ToggleCursor();
        activeCursorIndex++;
        if(activeCursorIndex == cursorBehaviours.Length) activeCursorIndex = 0;
        cursorBehaviours[activeCursorIndex].ToggleCursor();
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


}
