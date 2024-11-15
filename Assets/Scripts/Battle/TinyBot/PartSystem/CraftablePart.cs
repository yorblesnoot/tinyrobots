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
    ALL
}

[CreateAssetMenu(fileName = "CraftPart", menuName = "ScriptableObjects/CraftablePart")]
public class CraftablePart : SOWithGUID
{
    public StatValue[] PartStats;
    public SlotType Type;
    public int EnergyCost = 20;
    public GameObject AttachableObject;
    public bool PrimaryLocomotion;
    public bool Collectible = true;
}
