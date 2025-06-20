using UnityEngine.Events;

public class NoEvent : ZoneEvent
{
    public override void Activate(TowerNavZone zone, UnityAction eventComplete)
    {
        eventComplete(); 
    }

    public override void Visualize(TowerNavZone zone)
    {
        
    }

    public override void Clear(TowerNavZone zone)
    {
        
    }
}
