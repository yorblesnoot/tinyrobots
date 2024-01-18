using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryCursor : MonoBehaviour
{
    public static TinyBot ActiveBot;
    int activeIndex;
    [SerializeField] CursorBehaviour[] cursorBehaviours;
    private void Awake()
    {
        activeIndex = 0;
    }
    private void Update()
    {
        cursorBehaviours[activeIndex].ControlCursor();
        if (ClickableAbility.Active != null && Input.GetMouseButtonDown(0))
        {
            ClickableAbility.Active.ActivateAbility(ActiveBot, transform.position);
            return;
        }
        if(Input.GetKeyDown(KeyCode.G)) CycleCursorMode();
    }

    void CycleCursorMode()
    {
        cursorBehaviours[activeIndex].ToggleCursor();
        activeIndex++;
        if(activeIndex == cursorBehaviours.Length) activeIndex = 0;
        cursorBehaviours[activeIndex].ToggleCursor();
    }
}
