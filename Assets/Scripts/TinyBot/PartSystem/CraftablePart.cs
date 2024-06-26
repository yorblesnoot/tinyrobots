using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PartType
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
    [SerializeField] Stat[] partStats;
    public PartType type;
    public int weight = 20;
    public GameObject attachableObject;
    public bool primaryLocomotion;
    public Dictionary<StatType, int> Stats;
    //placement logic

    public void DeriveAttachmentAttributes()
    {
        InitializeStats();
    }

    public void InitializeStats()
    {
        Stats = partStats.ToDictionary(entry => entry.type, entry => entry.bonus);
    }

    [Serializable]
    class Stat
    {
        public StatType type;
        public int bonus;
    }
}
