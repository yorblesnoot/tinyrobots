using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirCursor : CursorBehaviour
{
    [SerializeField] float scrollRate;
    int layerMask;
    private void Awake()
    {
        layerMask = LayerMask.GetMask("SelectPlane");
    }

    public override void ControlCursor()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 pos = cursorLoadout.transform.position;
        pos.y += scroll * scrollRate;
        cursorLoadout.transform.position = pos;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 99999, layerMask);
        transform.position = hit.point;
    }
}
