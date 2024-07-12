using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ProcPhase
{
    TURNSTART,
    TURNEND,
    ROUNDSTART,
    ROUNDEND,
}
public class DurationModule : MonoBehaviour
{
    public int Duration;
    public ProcPhase Phase;
    int timePassed;
    UnityAction endCallback;
    UnityEvent tickEvent;
    public void SetDuration(TinyBot owner, UnityAction callback)
    {
        endCallback = callback;
        timePassed = 0;
        
        switch (Phase)
        {
            case ProcPhase.TURNSTART:
                tickEvent = owner.BeganTurn;
                break;
            case ProcPhase.TURNEND:
                tickEvent = owner.EndedTurn;
                break;
            case ProcPhase.ROUNDEND:
                tickEvent = TurnManager.RoundEnded;
                break;
            default: return;
        }
        tickEvent.AddListener(TickDuration);
    }

    void TickDuration()
    {
        timePassed++;
        if(timePassed < Duration) return;

        endCallback();
        tickEvent.RemoveListener(TickDuration);
    }
}
