using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
class SmartFilter : FilterButton
{
    [SerializeField] PartSlot originSlot;
    public override List<ModdedPart> FilterParts(List<ModdedPart> parts)
    {
        HashSet<SlotType> slots = new();
        originSlot.Traverse(SubmitType);
        return parts.Where(p => (!p.BasePart.PrimaryLocomotion || !PartSlot.PrimaryLocomotionSlotted) 
        && slots.Contains(p.BasePart.Type)).ToList();

        void SubmitType(PartSlot slot)
        {
            if(slot.PartIdentity == null) slots.Add(slot.SlotType);
        }
    }
}
