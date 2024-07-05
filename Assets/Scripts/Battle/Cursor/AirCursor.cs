using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirCursor : CursorBehaviour
{
    [SerializeField] float scrollRate;
    [SerializeField] float minimumCameraDistance = 10;
    [SerializeField] float maximumCameraDistance = 100;
    int layerMask;
    protected override void Initialize()
    {
        layerMask = LayerMask.GetMask("CursorPlanes");
    }

    public override void ActivateCursor()
    {
        base.ActivateCursor();
        cursorLoadout.transform.position = UnitControl.PlayerControlledBot != null ? 
            UnitControl.PlayerControlledBot.transform.position : PrimaryCursor.Transform.position;
    }

    public override void ControlCursor()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 newPosition = cursorLoadout.transform.localPosition;
        newPosition.z += scroll * scrollRate;
        newPosition.z = Mathf.Clamp(newPosition.z, minimumCameraDistance, maximumCameraDistance);

        cursorLoadout.transform.localPosition = newPosition;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 99999, layerMask);
        transform.position = hit.point;
    }
}
