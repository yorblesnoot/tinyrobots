using UnityEngine;

public class GroundCursor : CursorBehaviour
{
    [SerializeField] int castLength = 200;
    public override void ControlCursor()
    {
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, castLength, LayerMask.GetMask("Terrain"));
        if (hit.point == default) return;
        transform.position = hit.point;
    }

    protected override void Initialize()
    {
        
    }
}
