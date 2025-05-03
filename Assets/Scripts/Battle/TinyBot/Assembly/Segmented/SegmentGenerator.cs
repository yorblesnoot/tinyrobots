using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SegmentGenerator : MonoBehaviour
{
    [SerializeField] List<CraftablePart> segmentAttachments;
    [SerializeField] int segmentNumber = 5;
    [SerializeField] float segmentLength = 1f;  
    [SerializeField] int baseHealth = 20;
    [SerializeField] GameObject segmentPrefab;
    [SerializeField] Allegiance allegiance;
    public void SpawnUnit()
    {
        List<BotSegment> segments = new();
        int segmentCount = 0;
        RecursiveSegment();
        PlaceSegments(segments);



        void RecursiveSegment(BotSegment lastSegment = null, SegmentedMovement lastMovement = null)
        {
            if(segmentCount > segmentNumber) return;
            segmentCount++;

            GameObject spawned = Instantiate(segmentPrefab);
            BotSegment segment = spawned.GetComponent<BotSegment>();
            segments.Add(segment);
            SegmentedMovement movement = spawned.GetComponent<SegmentedMovement>();
            CraftablePart attachment = segmentAttachments.GrabRandomly(false);

            ModdedPart modAttachment = new(attachment);

            modAttachment.Sample.transform.SetParent(segment.AttachmentPoint);
            modAttachment.Sample.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            UnitStats stats = new();
            stats.Max[StatType.HEALTH] = baseHealth;
            stats.AddPartStats(modAttachment);
            stats.Max[StatType.MOVEMENT] = lastMovement != null ? 0 : 20;
          
            stats.MaxAll();
            segment.Initialize(modAttachment.Sample.Abilities.ToList(), new List<PartModifier>() { modAttachment.Sample }, movement, false, stats, allegiance);


            if (lastMovement == null) lastMovement.ChildSegment = movement;
            RecursiveSegment(segment, movement);
        }
    }

    void PlaceSegments(List<BotSegment> segments)
    {
        SpawnZone.PlaceBot(segments[0]);
        Pathfinder3D.GeneratePathingTree(MoveStyle.CRAWL, segments[0].transform.position);
        List<Vector3Int> pathableLocations = Pathfinder3D.GetPathableLocations(Mathf.FloorToInt(segmentNumber * segmentLength));
        Vector3Int segmentTarget = pathableLocations[0];
        List<Vector3> path = Pathfinder3D.FindVectorPath(segmentTarget, out _);
        for(int i = 1; i < segmentNumber; i++)
        {
            segments[i].transform.position = path[i];
            segments[i].transform.rotation = Quaternion.LookRotation(path[i-1] - path[i], Pathfinder3D.GetCrawlOrientation(path[i]));
        }
    }
}
