using UnityEngine;

public class GroundCursor : CursorBehaviour
{
    [SerializeField] int castLength = 200;
    public override void ControlCursor()
    {
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, castLength);
        if (hit.point == default) transform.position = new Vector3(-1000, -1000, -1000);
        transform.position = hit.point;
    }
}
