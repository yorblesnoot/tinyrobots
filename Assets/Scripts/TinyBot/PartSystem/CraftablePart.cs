using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PartType
{
    CORE,
    CHASSIS,
    UPPER,
    LOWER,
    REAR,
    LATERAL
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
    [HideInInspector] public AttachmentPoint[] attachmentPoints;
    [HideInInspector] public Vector3[] slotPositions;
    //placement logic

    public void DeriveAttachmentAttributes()
    {
        GameObject spawned = Instantiate(attachableObject);
        SetSlotTypes();
        Create2DSlotLayout();
        Initialize();
        spawned.SetActive(false);
    }

    void SetSlotTypes()
    {
        attachmentPoints = attachableObject.GetComponentsInChildren<AttachmentPoint>();
        if (type == PartType.CORE)
        {
            attachmentPoints[0].SlotType = PartType.CHASSIS;
        }
        else if(type == PartType.CHASSIS)
        {
            AssignChassisSlots();
        }
        else
        {
            AssignAllLateral(attachmentPoints);
        }
    }

    void AssignChassisSlots()
    {
        List<AttachmentPoint> sortable = attachmentPoints.OrderBy(x => x.transform.localPosition.y).ToList();
        AssignRemove(PartType.LOWER, 0);
        AssignRemove(PartType.UPPER, sortable.Count - 1);
        sortable = sortable.OrderBy(x => x.transform.localPosition.z).ToList();
        AssignRemove(PartType.REAR, 0);
        AssignAllLateral(sortable);

        void AssignRemove(PartType type, int index)
        {
            sortable[index].SlotType = type;
            sortable.RemoveAt(index);
        }
    }

    void AssignAllLateral(IEnumerable<AttachmentPoint> sortable)
    {
        foreach (AttachmentPoint attachmentPoint in sortable)
        {
            attachmentPoint.SlotType = PartType.LATERAL;
        }
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
