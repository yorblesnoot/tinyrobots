using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CursorBehaviour : MonoBehaviour
{
    [SerializeField] protected GameObject cursorLoadout;
    public abstract void ControlCursor();

    public void ToggleCursor()
    {
        if (cursorLoadout == null) return;
        cursorLoadout.SetActive(!cursorLoadout.activeSelf);
    }
}
