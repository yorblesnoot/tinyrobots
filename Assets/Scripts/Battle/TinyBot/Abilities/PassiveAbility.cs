using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAbility : Ability
{
    public override bool IsActive => false;
    [SerializeField] List<TriggerApplier> triggers;

    [SerializeField] SpatialSensor targeter;
    [SerializeField] ParticleSystem particleVisual;

    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        if(range == 0)
        {
            ApplyTriggeredEffect(Owner, true);
            return;
        }
        particleVisual.Play();
        ParticleSystem.ShapeModule shape = particleVisual.shape;
        shape.radius = range;
        targeter.transform.localScale = 2 * range * Vector3.one;


        targeter.UnitEnteredZone.AddListener((target) => ApplyTriggeredEffect(target, true));
        targeter.UnitLeftZone.AddListener((target) => ApplyTriggeredEffect(target, false));
    }

    public void ApplyTriggeredEffect(Targetable targetable, bool apply)
    {
        TinyBot bot = targetable as TinyBot;
        if (bot == null) return;
        foreach (var applier in triggers)
        {
            if (apply) applier.ApplyTo(bot);
            else applier.RemoveFrom(bot);
        }
    }
}
