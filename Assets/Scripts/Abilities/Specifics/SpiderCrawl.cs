using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCrawl : UnrestrictedAbility
{
    [SerializeField] SphereCollider detector;
    [SerializeField] float pathStepDuration = .2f;
    [SerializeField] float legStepDuration = .1f;
    [SerializeField] float lookHeightModifier = 1f;

    [SerializeField] Anchor[] anchors;
    [SerializeField] float anchorZoneRadius = .1f;

    bool stepping;
    private void Awake()
    {
        InitializeAnchors();
    }
    private void InitializeAnchors()
    {
        foreach (var anchor in anchors)
        {
            anchor.Initialize();
        }
    }

    public override void ExecuteAbility(TinyBot user, Vector3 target)
    {
        var path = Pathfinder3D.FindVectorPath(MoveStyle.CRAWL, Vector3Int.RoundToInt(user.transform.position), Vector3Int.RoundToInt(target));
        if (path == null) return;
        StartCoroutine(CrawlPath(user, path));
    }

    IEnumerator CrawlPath(TinyBot user, List<Vector3> path)
    {
        foreach (var target in path)
        {
            Collider[] colliders = Physics.OverlapSphere(target, 2f, LayerMask.GetMask("Terrain"));
            detector.transform.SetParent(null);
            detector.transform.position = target;
            CheckSphereExtra(colliders[0], detector, out Vector3 closestPoint, out Vector3 surfaceNormal);
            yield return StartCoroutine(InterpolatePositionAndRotation(user.transform, target, surfaceNormal));
        }
    }

    IEnumerator InterpolatePositionAndRotation(Transform unit, Vector3 target, Vector3 targetNormal)
    {
        Quaternion startRotation = transform.rotation;
        Vector3 startPosition = unit.position;
        Vector3 lookTarget = target + targetNormal * lookHeightModifier;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position, targetNormal);
        float timeElapsed = 0;
        while(timeElapsed < pathStepDuration)
        {
            unit.SetPositionAndRotation(Vector3.Lerp(startPosition, target, timeElapsed/pathStepDuration), 
                Quaternion.Slerp(startRotation, targetRotation, timeElapsed/pathStepDuration));
            timeElapsed += Time.deltaTime;


            CheckAnchorPositions();
            yield return null;
        }
    }

    private void CheckAnchorPositions()
    {
        Anchor farthestFromBase = null;
        foreach (var anchor in anchors)
        {
            farthestFromBase ??= anchor;
            anchor.GluePosition();
            anchor.CalculateDisplacementFromBase();
            if(anchor.distanceFromBase > farthestFromBase.distanceFromBase) farthestFromBase = anchor;
        }

        if(farthestFromBase.distanceFromBase >= anchorZoneRadius && !stepping)
        {
            StartCoroutine(StepPastBasePosition(farthestFromBase, anchorZoneRadius, legStepDuration));
        }
    }

    IEnumerator StepPastBasePosition(Anchor anchor, float distance, float legMoveDuration)
    {
        stepping = true;
        anchor.stepping = true;
        Vector3 startPosition = anchor.ikTarget.localPosition;
        Vector3 direction =  anchor.basePosition - startPosition;
        direction.Normalize();
        Vector3 finalPosition = anchor.basePosition + direction * distance;
        float timeElapsed = 0;
        while (timeElapsed < legMoveDuration)
        {
            anchor.ikTarget.localPosition = Vector3.Lerp(startPosition, finalPosition, timeElapsed / legMoveDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        anchor.UpdateGluedPosition();
        stepping = false;
        anchor.stepping = false;
    }

    public static bool CheckSphereExtra(Collider target_collider, SphereCollider sphere_collider, out Vector3 closestPoint, out Vector3 surfaceNormal)
    {
        closestPoint = Vector3.zero;
        surfaceNormal = Vector3.zero;
        Vector3 sphere_pos = sphere_collider.transform.position;
        if (Physics.ComputePenetration(target_collider, target_collider.transform.position, target_collider.transform.rotation, sphere_collider, sphere_pos, Quaternion.identity, out surfaceNormal, out float surfacePenetrationDepth))
        {
            closestPoint = sphere_pos + (surfaceNormal * (sphere_collider.radius - surfacePenetrationDepth));

            surfaceNormal = -surfaceNormal;

            return true;
        }
        return false;
    }

    [Serializable]
    class Anchor
    {
        public bool stepping;
        public Transform ikTarget;
        [HideInInspector] public Vector3 basePosition;
        [HideInInspector] public float distanceFromBase;

        Vector3 gluedWorldPosition;
        public void Initialize()
        {
            basePosition = ikTarget.localPosition;
            UpdateGluedPosition();
        }

        public void UpdateGluedPosition()
        {
            gluedWorldPosition = ikTarget.position;
        }

        public void CalculateDisplacementFromBase()
        {
            distanceFromBase = Vector3.Distance(ikTarget.localPosition, basePosition);
        }

        public void GluePosition()
        {
            if (stepping) return;
            ikTarget.position = gluedWorldPosition;
        }
    }
}