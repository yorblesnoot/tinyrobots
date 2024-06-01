using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PartType
{
    CHASSIS,
    MOBILITY,
    ATTACHMENT,
    MOD
}

[CreateAssetMenu(fileName = "CraftPart", menuName = "ScriptableObjects/CraftPart")]
public class CraftablePart : SOWithGUID
{
    [SerializeField] Stat[] partStats;
    public int weight = 20;
    public GameObject attachableObject;
    public bool primaryLocomotion;
    public Dictionary<StatType, int> Stats;
    [HideInInspector] public AttachmentPoint[] attachmentPoints;
    [HideInInspector] public Vector3[] slotPositions;
    //placement logic

    public void DeriveAttachmentAttributes()
    {
        GameObject spawned = Instantiate(attachableObject);
        attachmentPoints = attachableObject.GetComponentsInChildren<AttachmentPoint>();
        Create2DSlotLayout();
        Initialize();
        spawned.SetActive(false);
    }

    public void Initialize()
    {
        Stats = partStats.ToDictionary(entry => entry.type, entry => entry.bonus);
    }
    void Create2DSlotLayout()
    {
        slotPositions = new Vector3[attachmentPoints.Length];
        for (int i = 0; i < attachmentPoints.Length; i++)
        {
            Vector3 modelPosition = attachmentPoints[i].transform.localPosition;
            modelPosition.z = 0;
            slotPositions[i] = modelPosition;
        }
    }

    [Serializable]
    class Stat
    {
        public StatType type;
        public int bonus;
    }
}
