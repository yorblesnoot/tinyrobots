using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornsAura : PassiveAbility
{
    [SerializeField] SpatialTargeter targeter;
    [SerializeField] ParticleSystem thornsVisual;
    List<Targetable> affectedUnits = new();
    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        thornsVisual.gameObject.SetActive(true);
        ParticleSystem.ShapeModule shape = thornsVisual.shape;
        shape.radius = range;
        targeter.transform.localScale = Vector3.one * range;
        targeter.UnitTargeted.AddListener(DealSpikeDamage);
        TurnManager.RoundEnded.AddListener(ResetSpiked);
    }

    void DealSpikeDamage(Targetable targetable)
    {
        if (targetable.Allegiance == Owner.Allegiance || affectedUnits.Contains(targetable)) return;
        targetable.ReceiveHit(damage, Owner.TargetPoint.position, targetable.TargetPoint.position, false);
        affectedUnits.Add(targetable);
    }

    void ResetSpiked()
    {
        affectedUnits.Clear();
    }
}
