using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraftPart : AbilityEffect
{
    [SerializeField] float graftAnimationDuration = 1f;
    [SerializeField] List<GameObject> graftSlots;
    List<GameObject> availableSlots;
    public override void Initialize(Ability ability)
    {
        base.Initialize(ability);
        availableSlots = new(graftSlots);
    }
    public override IEnumerator PerformEffect(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets)
    {
        TinyBot target = targets[0] as TinyBot;
        AttemptToGraft(owner, target);
        yield break;
    }

    void AttemptToGraft(TinyBot owner, TinyBot bot)
    {
        if (availableSlots.Count == 0) return;

        List<PartModifier> parts = bot.PartModifiers.Where(part => part.SourcePart.BasePart.Type == SlotType.LATERAL && part.Abilities.Count() > 0).ToList();
        PartModifier target = parts.GrabRandomly(false);
        GraftPartToUnit(owner, target, graftAnimationDuration);
    }

    private void GraftPartToUnit(TinyBot owner, PartModifier part, float duration)
    {
        if(part.TryGetComponent<Rigidbody>(out var body)) body.isKinematic = true;
        GameObject slot = availableSlots.GrabRandomly();
        part.transform.SetParent(slot.transform.parent, true);
        Tween.LocalPosition(part.transform, slot.transform.localPosition, duration)
            .Group(Tween.LocalRotation(part.transform, slot.transform.localRotation, duration)).OnComplete(() => AddAbilities(part, owner));
        slot.SetActive(false);

        
    }

    void AddAbilities(PartModifier target, TinyBot owner)
    {
        foreach (var ability in target.Abilities)
        {
            ability.Initialize(owner);
            ability.ModifyOn(owner, true);
        }
    }
}
