using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] GameObject staticAura;

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
        
        
        targeter.transform.localScale = 2 * range * Vector3.one;


        targeter.UnitEnteredZone.AddListener((target) => ApplyTriggeredEffect(target, true));
        targeter.UnitLeftZone.AddListener((target) => ApplyTriggeredEffect(target, false));
    }

    protected override void AddTo(TinyBot bot)
    {
        if (range == 0)
        {
            ApplyTriggeredEffect(Owner, true);
            return;
        }
        ToggleVisuals(true);
        targeter.gameObject.SetActive(true);
    }

    protected override void RemoveFrom(TinyBot bot)
    {
        ToggleVisuals(false);
        targeter.gameObject.SetActive(false);
    }

    void ToggleVisuals(bool toggle)
    {
        if (particleVisual != null)
        {
            //Debug.Log("played aura particle " + name);
            particleVisual.gameObject.SetActive(toggle);
            ParticleSystem.ShapeModule shape = particleVisual.shape;
            shape.radius = range;
        }
        if (staticAura != null)
        {
            staticAura.SetActive(toggle);
            staticAura.transform.localScale = Vector3.one * range;
            staticAura.transform.SetParent(Owner.transform, true);
        }
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
        foreach (EffectTrigger applier in triggers)
        {
            if (apply) applier.ApplyTo(bot);
            else applier.RemoveFrom(bot);
        }
    }
}
