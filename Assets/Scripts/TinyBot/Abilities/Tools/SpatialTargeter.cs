using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SpatialTargeter : MonoBehaviour
{
    List<Targetable> intersectingTargets = new();
    public UnityEvent<Targetable> UnitTargeted = new();
    public void ResetIntersecting()
    {
        intersectingTargets = new();
    }

    public List<Targetable> GetIntersectingBots()
    {
        return new(intersectingTargets.Where(bot => bot != null));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent(out Targetable target)) return;
        intersectingTargets.Add(target);
        UnitTargeted.Invoke(target);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Targetable target))
        {
            intersectingTargets.Remove(target);
        }

    }
}
