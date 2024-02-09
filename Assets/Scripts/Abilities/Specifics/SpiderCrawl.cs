using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCrawl : PrimaryMovement
{
    [SerializeField] SphereCollider detector;
    [SerializeField] float pathStepDuration = .2f;
    [SerializeField] float legStepDuration = .1f;
    [SerializeField] float lookHeightModifier = 1f;

    [SerializeField] Anchor[] anchors;
    [SerializeField] float anchorZoneRadius = .1f;

    [SerializeField] Transform legModel;


    bool stepping;
    private void Awake()
    {
        PreferredCursor = CursorType.GROUND;
        MoveStyle = MoveStyle.CRAWL;
        InitializeAnchors();
    }
    private void InitializeAnchors()
    {
        foreach (var anchor in anchors)
        {
            anchor.Initialize();
        }
    }

    public override IEnumerator PathToPoint(TinyBot user, List<Vector3> path)
    {
        foreach (var target in path)
        {
            Vector3 surfaceNormal = GetMeshFacingAt(target);
            yield return StartCoroutine(InterpolatePositionAndRotation(user.transform, target, surfaceNormal));
        }

        foreach (var anchor in anchors)
        {
            yield return StartCoroutine(StepToBase(anchor, true));
        }
    }

    private Vector3 GetMeshFacingAt(Vector3 target)
    {
        Collider[] colliders = Physics.OverlapSphere(target, 1f, LayerMask.GetMask("Terrain"));
        detector.transform.SetParent(null);
        detector.transform.position = target;
        CheckSphereExtra(colliders[0], detector, out Vector3 closestPoint, out Vector3 surfaceNormal);
        return surfaceNormal;
    }

    IEnumerator InterpolatePositionAndRotation(Transform unit, Vector3 target, Vector3 targetNormal)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = GetRotationForMeshPosition(target, targetNormal);

        Vector3 startPosition = unit.position;
        float timeElapsed = 0;
        while (timeElapsed < pathStepDuration)
        {
            unit.SetPositionAndRotation(Vector3.Lerp(startPosition, target, timeElapsed / pathStepDuration),
                Quaternion.Slerp(startRotation, targetRotation, timeElapsed / pathStepDuration));
            timeElapsed += Time.deltaTime;


            CheckAnchorPositions();
            yield return null;
        }
    }

    private Quaternion GetRotationForMeshPosition(Vector3 moveTarget, Vector3 targetNormal)
    {
        Vector3 lookTarget = moveTarget + targetNormal * lookHeightModifier;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position, targetNormal);
        return targetRotation;
    }

    private void CheckAnchorPositions()
    {
        Anchor farthestFromBase = null;
        foreach (var anchor in anchors)
        {
            farthestFromBase ??= anchor;
            anchor.GluePosition();
            if(anchor.distanceFromBase > farthestFromBase.distanceFromBase) farthestFromBase = anchor;
        }

        if(farthestFromBase.distanceFromBase >= anchorZoneRadius && !stepping)
        {
            StartCoroutine(StepToBase(farthestFromBase));
        }
    }
    IEnumerator StepToBase(Anchor anchor, bool goToNeutral = false)
    {
        stepping = true;
        anchor.stepping = true;
        Vector3 localStartPosition = anchor.ikTarget.localPosition;
        Vector3 direction =  anchor.localBasePosition - localStartPosition;
        direction.Normalize();

        Vector3 initialPosition = anchor.localBasePosition + (goToNeutral ? Vector3.zero : direction * anchorZoneRadius);
        Vector3 rayPosition = initialPosition;
        rayPosition.y += 2;
        rayPosition = legModel.TransformPoint(rayPosition);
        Ray ray = new(rayPosition, -transform.up);
        Vector3 finalPosition;
        if (Physics.Raycast(ray, out var hitInfo, 3f, LayerMask.GetMask("Terrain")))
        {
            finalPosition = hitInfo.point;
            finalPosition = legModel.InverseTransformPoint(finalPosition);
        }
        else
        {
            finalPosition = initialPosition;
        }

        float timeElapsed = 0;
        while (timeElapsed < legStepDuration)
        {
            anchor.ikTarget.localPosition = Vector3.Lerp(localStartPosition, finalPosition, timeElapsed / legStepDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        anchor.UpdateGluedPosition();
        stepping = false;
        anchor.stepping = false;
    }
    static bool CheckSphereExtra(Collider target_collider, SphereCollider sphere_collider, out Vector3 closestPoint, out Vector3 surfaceNormal)
    {
        closestPoint = Vector3.zero;
        Vector3 sphere_pos = sphere_collider.transform.position;
        if (Physics.ComputePenetration(target_collider, target_collider.transform.position, target_collider.transform.rotation, sphere_collider, sphere_pos, Quaternion.identity, out surfaceNormal, out float surfacePenetrationDepth))
        {
            closestPoint = sphere_pos + (surfaceNormal * (sphere_collider.radius - surfacePenetrationDepth));

            surfaceNormal = -surfaceNormal;

            return true;
        }
        return false;
    }
    public override void SpawnOrientation(Transform unit)
    {
        
        Vector3 normal = GetMeshFacingAt(unit.position);
        Vector3 displace = Vector3.Cross(normal, Vector3.up);
        //look position and normal cant be the same?
        unit.rotation = GetRotationForMeshPosition(unit.position + displace, normal);
        foreach (var anchor in anchors)
        {
            StartCoroutine(StepToBase(anchor, true));
        }
    }

    public override IEnumerator RotateInPlace()
    {
        yield return null;
    }

    [Serializable]
    class Anchor
    {
        public bool stepping;
        public Transform ikTarget;
        [HideInInspector] public Vector3 localBasePosition;
        [HideInInspector] public float distanceFromBase;

        Vector3 gluedWorldPosition;
        public void Initialize()
        {
            localBasePosition = ikTarget.localPosition;
        }

        public void UpdateGluedPosition()
        {
            gluedWorldPosition = ikTarget.position;
        }

        public void GluePosition()
        {
            if (stepping) return;
            ikTarget.position = gluedWorldPosition;
            distanceFromBase = Vector3.Distance(ikTarget.localPosition, localBasePosition);
        }
    }
}