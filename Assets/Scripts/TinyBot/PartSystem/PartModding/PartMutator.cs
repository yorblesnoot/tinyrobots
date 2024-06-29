using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "PartMutator", menuName = "ScriptableObjects/PartMutator")]
public class PartMutator : SOWithGUID
{
    [SerializeField] SlotType[] compatibleSlots;
    public StatValue[] Stats;
    public ModValue[] Mods;

    public bool IsCompatibleWith(SlotType type)
    {
        if(compatibleSlots.Contains(type)) return true;
        return false;
    }
}
