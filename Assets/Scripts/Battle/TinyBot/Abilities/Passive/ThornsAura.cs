using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornsAura : PassiveAbility
{
    [SerializeField] SpatialSensor targeter;
    [SerializeField] ParticleSystem thornsVisual;
    List<Targetable> affectedUnits = new();
    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        thornsVisual.Play();
        ParticleSystem.ShapeModule shape = thornsVisual.shape;
        shape.radius = range;
        targeter.transform.localScale = 2 * range * Vector3.one;
        targeter.UnitTargeted.AddListener(DealSpikeDamage);
        TurnManager.RoundEnded.AddListener(ResetSpiked);
    }

    void DealSpikeDamage(Targetable targetable)
    {
        if (targetable.Allegiance == Owner.Allegiance || affectedUnits.Contains(targetable)) return;
        targetable.ReceiveHit(EffectMagnitude, Owner.TargetPoint.position, targetable.TargetPoint.position, false);
        affectedUnits.Add(targetable);
    }

    void ResetSpiked()
    {
        affectedUnits.Clear();
    }
}
