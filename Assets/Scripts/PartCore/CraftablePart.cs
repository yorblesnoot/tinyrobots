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
public class CraftablePart : SOWithGUID
{
    public GameObject attachableObject;

    public bool primaryLocomotion;
    [HideInInspector] public AttachmentPoint[] attachmentPoints;
    [HideInInspector] public Vector3[] slotPositions;
    //placement logic

    public void DeriveAttachmentAttributes()
    {
        GameObject spawned = GameObject.Instantiate(attachableObject);
        attachmentPoints = attachableObject.GetComponentsInChildren<AttachmentPoint>();
        Create2DSlotLayout();
        spawned.SetActive(false);
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
