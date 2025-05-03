using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SegmentGenerator : MonoBehaviour
{
    [SerializeField] List<CraftablePart> segmentAttachments;
    int segmentNumber = 5;
    [SerializeField] int baseHealth = 20;
    [SerializeField] GameObject segmentPrefab;
    [SerializeField] Allegiance allegiance;
    void SpawnUnit()
    {
        int segmentCount = 0;
        RecursiveSegment();
        void RecursiveSegment(BotSegment lastSegment = null, SegmentedMovement lastMovement = null)
        {
            if(segmentCount > segmentNumber) return;
            segmentCount++;

            GameObject spawned = Instantiate(segmentPrefab);
            BotSegment segment = spawned.GetComponent<BotSegment>();
            SegmentedMovement movement = spawned.GetComponent<SegmentedMovement>();
            CraftablePart attachment = segmentAttachments.GrabRandomly(false);

            ModdedPart modAttachment = new(attachment);

            modAttachment.Sample.transform.SetParent(segment.AttachmentPoint);
            modAttachment.Sample.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            UnitStats stats = new();
            stats.Max[StatType.HEALTH] = baseHealth;
            stats.AddPartStats(modAttachment);
            if (lastMovement != null) stats.Max[StatType.MOVEMENT] = 0;
            stats.MaxAll();
            segment.Initialize(modAttachment.Sample.Abilities.ToList(), new List<PartModifier>() { modAttachment.Sample }, movement, false, stats, allegiance);


            if (lastMovement == null) lastMovement.ChildSegment = movement;
            RecursiveSegment(segment, movement);
        }
    }
}
