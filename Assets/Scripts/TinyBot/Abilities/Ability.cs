using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [SerializeField] protected GameObject emissionPoint;
    public CursorType PreferredCursor;
    public int cost;
    public int range;
    public Sprite icon;

    public string[] blockingLayers;
    protected int blockingLayerMask;

    GameObject trackedTarget;
    protected Vector3 targetedPosition;
    
    [HideInInspector] public TinyBot owner;

    private void Awake()
    {
        blockingLayerMask = LayerMask.GetMask(blockingLayers);
    }
    public abstract IEnumerator ExecuteAbility();
    protected virtual void AimAt(GameObject target)
    {
        Vector3 sourcePosition = emissionPoint.transform.position;
        Vector3 targetPosition = target.transform.position;
        Vector3 direction = (targetPosition - sourcePosition).normalized;
        Vector3 modifiedTarget = sourcePosition + direction * range;
        targetedPosition = modifiedTarget;
        Vector3[] targets = GetTrajectory(sourcePosition, modifiedTarget);
        List<Vector3> points = CastAlongPoints(targets, blockingLayerMask, out _);
        LineMaker.DrawLine(points.ToArray());
    }

    protected abstract Vector3[] GetTrajectory(Vector3 source, Vector3 target);
    public virtual bool AbilityUsableAtCurrentTarget()
    {
        return true;
    }

    public virtual void LockOnTo(GameObject target)
    {
        trackedTarget = target;
    }

    public virtual void ReleaseLock()
    {
        trackedTarget = null;
        StartCoroutine(owner.PrimaryMovement.NeutralStance());
        LineMaker.HideLine();
    }

    readonly float overlapLength = .1f;
    protected virtual List<Vector3> CastAlongPoints(Vector3[] castTargets, int mask, out GameObject hit)
    {
        hit = null;
        List<Vector3> modifiedTargets = new()
        {
            castTargets[0]
        };
        for (int i = 0; i < castTargets.Length - 1; i++)
        {
            Vector3 direction = castTargets[i + 1] - castTargets[i];
            Ray ray = new(castTargets[i], direction);
            if (Physics.Raycast(ray, out var hitInfo, direction.magnitude + overlapLength, mask))
            {
                modifiedTargets.Add(hitInfo.point);
                hit = hitInfo.collider.gameObject;
                break;
            }
            else
            {
                modifiedTargets.Add(castTargets[i + 1]);
            }
        }
        return modifiedTargets;
    }
    protected IEnumerator LaunchAlongLine(GameObject launched, List<Vector3> trajectory, float travelTime, GameObject hit)
    {
        GameObject spawned = Instantiate(launched);
        float intervalTime = travelTime / trajectory.Count;
        float timeElapsed;
        spawned.transform.rotation = emissionPoint.transform.rotation;
        for(int i = 0; i < trajectory.Count - 1; i++)
        {
            Quaternion startRotation = spawned.transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(trajectory[i + 1] - trajectory[i]);
            timeElapsed = 0;
            while(timeElapsed < intervalTime)
            {
                yield return null;
                float interpolator = timeElapsed / intervalTime;
                spawned.transform.SetPositionAndRotation(Vector3.Lerp(trajectory[i], trajectory[i + 1], interpolator),
                    Quaternion.Slerp(startRotation, targetRotation, interpolator));
                timeElapsed += Time.deltaTime;
                
            }
        }
        CompleteTrajectory(trajectory.Last(), spawned, hit);
    }

    protected virtual void CompleteTrajectory(Vector3 position, GameObject launched, GameObject hit) { }
    void Update()
    {
        if (trackedTarget == null) return;
        AimAt(trackedTarget);
        owner.PrimaryMovement.RotateToTrackEntity(trackedTarget);
    }

} 
