using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PrimaryCursor : MonoBehaviour
{
    public static TinyBot ActiveBot;
    int activeIndex;
    [SerializeField] CursorBehaviour[] cursorBehaviours;
    public static Transform Transform;
    public static bool Locked;
    private void Awake()
    {
        Transform = transform;
        activeIndex = 0;
    }
    private void Update()
    {
        if (Locked) return;
        cursorBehaviours[activeIndex].ControlCursor();
        //clamp the cursor's position within the bounds of the map~~~~~~~~~~~~~~~~~~~~~
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
