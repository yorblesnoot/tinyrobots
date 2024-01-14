using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryCursor : MonoBehaviour
{
    public static GameObject globalCursor;
    public static TinyBot ActiveBot;
    public static Vector3 Position { get { return globalCursor.transform.position; } }
    private void Awake()
    {
        globalCursor = gameObject;
    }
    private void Update()
    {
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit);
        transform.position = hit.point;
        if (ClickableAbility.Active != null && Input.GetMouseButtonDown(0))
        {
            ClickableAbility.Active.ActivateAbility(ActiveBot, Position);
        }
    }

    
}
