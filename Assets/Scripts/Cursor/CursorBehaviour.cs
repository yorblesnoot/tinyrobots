using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CursorBehaviour : MonoBehaviour
{
    [SerializeField] protected GameObject cursorLoadout;
    public static UnityEvent Reset = new();
    private void Awake()
    {
        Reset.AddListener(DeactivateCursor);
        Initialize();
    }

    protected abstract void Initialize();

    public abstract void ControlCursor();

    public virtual void ActivateCursor()
    {
        if (cursorLoadout == null) return;
        cursorLoadout.SetActive(true);
    }

    public void DeactivateCursor()
    {
        if (cursorLoadout == null) return;
        cursorLoadout.SetActive(true);
    }
}
