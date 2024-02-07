using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [SerializeField] AbilityUI abilityUI;
    static AbilityUI AbilityUI;
    private void Awake()
    {
        Transform = transform;
        AbilityUI = abilityUI;
        activeCursorIndex = 0;
    }
    private void Update()
    {
        
        //clamp the cursor's position within the bounds of the map~~~~~~~~~~~~~~~~~~~~~
        if (Input.GetMouseButtonDown(0))
        {
            if (AbilityUI.Active != null)
            {
                AbilityUI.Active.ExecuteAbility(SelectedBot, transform.position);
                AbilityUI.Active = null;
                ClickableAbility.clearActive.Invoke();
                return;
            }
            else if (TargetedBot != null)
            {
                SelectBot(TargetedBot);
            }
        }
        
        if(Input.GetKeyDown(KeyCode.G)) CycleCursorMode();

        if (State != CursorState.FREE) return;
        cursorBehaviours[activeCursorIndex].ControlCursor();
    }

    public static void SelectBot(TinyBot bot)
    {
        if (!bot.availableForTurn) return;
        if (SelectedBot != null) SelectedBot.BecomeActiveUnit(false);
        SelectedBot = bot;
        SelectedBot.BecomeActiveUnit(true);
        AbilityUI.ShowControlForUnit(bot);
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
