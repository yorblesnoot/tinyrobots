using System.Linq;
using UnityEngine;

public enum SlotType
{
    LATERAL,
    CORE,
    CHASSIS,
    UPPER,
    LOWER,
    REAR,
}

[CreateAssetMenu(fileName = "CraftPart", menuName = "ScriptableObjects/CraftPart")]
public class CraftablePart : SOWithGUID
{
    public StatValue[] PartStats;
    public SlotType Type;
    public int Weight = 20;
    public GameObject AttachableObject;
    public bool PrimaryLocomotion;
    public bool Collectible = true;
}
