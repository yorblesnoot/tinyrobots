using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraftAura : PassiveAbility
{
    [SerializeField] float graftAnimationDuration = 1f;
    [SerializeField] List<GameObject> graftSlots;
    List<GameObject> availableSlots;
    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        availableSlots = new(graftSlots);
        TinyBot.BotDied.AddListener(AttemptToGraft);
    }

    void AttemptToGraft(TinyBot bot)
    {
        if(Vector3.Distance(bot.transform.position, Owner.transform.position) > range) return;
        Owner.Heal(Mathf.RoundToInt(bot.Stats.Max[StatType.HEALTH] * EffectMagnitude/100));
        if(availableSlots.Count == 0) return;

        
        List<PartModifier> parts = bot.PartModifiers.Where(part => part.SourcePart.BasePart.Type == SlotType.LATERAL && part.Abilities.Count() > 0).ToList();
        PartModifier target = parts.GrabRandomly(false);
        target.GetComponent<Rigidbody>().isKinematic = true;
        GameObject slot = availableSlots.GrabRandomly();
        target.transform.SetParent(slot.transform.parent, true);
        Tween.LocalPosition(target.transform, slot.transform.localPosition, graftAnimationDuration);
        Tween.LocalRotation(target.transform, slot.transform.localRotation, graftAnimationDuration);
        slot.SetActive(false);
        
        foreach(var ability in target.Abilities)
        {
            Owner.AddAbility(ability);
        }
    }
}
