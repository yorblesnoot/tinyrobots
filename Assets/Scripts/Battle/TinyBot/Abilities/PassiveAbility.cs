using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAbility : Ability
{
    public override bool IsActive => false;
    [SerializeField] List<TriggerController> triggers;

    [SerializeField] SpatialSensor targeter;
    [SerializeField] ParticleSystem particleVisual;

    enum ValidTarget
    {
        ANY,
        ALLY,
        ENEMY,
    }
    [SerializeField] ValidTarget validTarget;

    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        foreach (var trigger in triggers) trigger.Initialize(botUnit, this);
        if(range == 0)
        {
            ApplyTriggeredEffect(Owner, true);
            return;
        }
        if(particleVisual != null)
        {
            particleVisual.Play();
            ParticleSystem.ShapeModule shape = particleVisual.shape;
            shape.radius = range;
        }
        targeter.transform.localScale = 2 * range * Vector3.one;


        targeter.UnitEnteredZone.AddListener((target) => ApplyTriggeredEffect(target, true));
        targeter.UnitLeftZone.AddListener((target) => ApplyTriggeredEffect(target, false));
    }

    public void ApplyTriggeredEffect(Targetable targetable, bool apply)
    {
        TinyBot bot = targetable as TinyBot;
        if (bot == null) return;
        if (validTarget == ValidTarget.ALLY && bot.Allegiance != Owner.Allegiance) return;
        if (validTarget == ValidTarget.ENEMY && bot.Allegiance == Owner.Allegiance) return;
        foreach (var applier in triggers)
        {
            if (apply) applier.ApplyTo(bot);
            else applier.RemoveFrom(bot);
        }
    }
}
