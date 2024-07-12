using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "PartMutator", menuName = "ScriptableObjects/PartMutator")]
public class PartMutator : SOWithGUID
{
    public StatValue[] Stats;
    public ModValue[] Mods;
}
