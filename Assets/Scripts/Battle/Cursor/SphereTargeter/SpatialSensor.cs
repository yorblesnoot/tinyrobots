using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class SpatialSensor : MonoBehaviour
{
    List<Targetable> intersectingTargets = new();
    [HideInInspector] public UnityEvent<Targetable> UnitEnteredZone = new();
    [HideInInspector] public UnityEvent<Targetable> UnitLeftZone = new();
    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ToggleVisual(bool toggle)
    {
        meshRenderer.enabled = toggle;
    }
    public void ResetIntersecting()
    {
        intersectingTargets = new();
    }

    public List<Targetable> GetIntersectingTargets()
    {
        return new(intersectingTargets.Where(bot => bot != null));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent(out Targetable target)) return;
        intersectingTargets.Add(target);
        UnitEnteredZone.Invoke(target);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Targetable target))
        {
            intersectingTargets.Remove(target);
        }
        UnitLeftZone.Invoke(target);
    }
}
