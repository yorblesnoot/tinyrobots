using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirCursor : CursorBehaviour
{
    [SerializeField] float scrollRate;
    int layerMask;
    protected override void Initialize()
    {
        layerMask = LayerMask.GetMask("Ignore Raycast");
    }

    public override void ActivateCursor()
    {
        base.ActivateCursor();
        cursorLoadout.transform.position = PrimaryCursor.Transform.position;
    }

    public override void ControlCursor()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = cursorLoadout.transform.localPosition;
        pos.z += scroll * scrollRate;
        cursorLoadout.transform.localPosition = pos;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 99999, layerMask);
        transform.position = hit.point;
    }
}
