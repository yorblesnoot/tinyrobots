using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PartType
{
    CHASSIS,
    MOBILITY,
    ATTACHMENT,
    MOD
}

[CreateAssetMenu(fileName = "CraftPart", menuName = "ScriptableObjects/CraftPart")]
public class CraftablePart : ScriptableObject
{
    public List<Ability> abilities;
    public GameObject attachableObject;
    [HideInInspector] public AttachmentPoint[] attachmentPoints;
    [HideInInspector] public Vector3[] slotPositions;
    //placement logic

    public void DeriveAttachmentAttributes()
    {
        GameObject.Instantiate(attachableObject);
        attachmentPoints = attachableObject.GetComponentsInChildren<AttachmentPoint>();
        Create2DSlotLayout();
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
}
