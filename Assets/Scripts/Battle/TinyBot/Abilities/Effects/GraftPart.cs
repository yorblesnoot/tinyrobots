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
        target.GetComponent<Rigidbody>().isKinematic = true;
        GameObject slot = availableSlots.GrabRandomly();
        target.transform.SetParent(slot.transform.parent, true);
        Tween.LocalPosition(target.transform, slot.transform.localPosition, graftAnimationDuration);
        Tween.LocalRotation(target.transform, slot.transform.localRotation, graftAnimationDuration);
        slot.SetActive(false);

        foreach (var ability in target.Abilities)
        {
            owner.AddAbility(ability);
        }
    }
}