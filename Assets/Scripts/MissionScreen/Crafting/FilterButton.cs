using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[Serializable]
class FilterButton
{
    public Button Button;
    public SlotType Type;
    public virtual List<ModdedPart> FilterParts(List<ModdedPart> parts)
    {
        return parts.Where(part => part.BasePart.Type == Type).ToList();
    }
}
