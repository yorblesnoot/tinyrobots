using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PassiveAbility : Ability
{
    public override bool IsActive => false;

    protected override AbilityEffect[] Effects { get
        {
            effects ??= GetFinalEffects();
            return effects;
        } }
    AbilityEffect[] effects;

    [SerializeField] List<EffectTrigger> triggers;

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

    AbilityEffect[] GetFinalEffects()
    {
        List<AbilityEffect> effects = new();
        foreach (var applier in triggers)
        {
            effects.AddRange(applier.OutputEffect);
        }
        return effects.ToArray();
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
