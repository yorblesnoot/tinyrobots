using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCrawl : UnrestrictedAbility
{
    Vector3 lookTarget;
    [SerializeField] SphereCollider detector;
    Collider targetCollider;
    Transform owner;
    public override void ExecuteAbility(TinyBot user, Vector3 target)
    {
        var path = Pathfinder3D.FindVectorPath(MoveStyle.CRAWL, Vector3Int.RoundToInt(user.transform.position), Vector3Int.RoundToInt(target));
        if (path == null) return;
        owner = user.transform;
        StartCoroutine(CrawlPath(user, path));
    }

    IEnumerator CrawlPath(TinyBot user, List<Vector3> path)
    {
        foreach (var target in path)
        {
            Collider[] colliders = Physics.OverlapSphere(target, 2f, LayerMask.GetMask("Terrain"));
            targetCollider = colliders[0];
            lookTarget = target;
            yield return StartCoroutine(user.gameObject.LerpTo(target, .3f));
        }
    }

    private void Update()
    {
        if (targetCollider == null) return;
        if (CheckSphereExtra(targetCollider, detector, out Vector3 closest_point, out Vector3 surface_normal)) owner.LookAt(lookTarget, surface_normal);
    }

    public static bool CheckSphereExtra(Collider target_collider, SphereCollider sphere_collider, out Vector3 closest_point, out Vector3 surface_normal)
    {
        closest_point = Vector3.zero;
        surface_normal = Vector3.zero;
        float surface_penetration_depth = 0;

        Vector3 sphere_pos = sphere_collider.transform.position;
        if (Physics.ComputePenetration(target_collider, target_collider.transform.position, target_collider.transform.rotation, sphere_collider, sphere_pos, Quaternion.identity, out surface_normal, out surface_penetration_depth))
        {
            closest_point = sphere_pos + (surface_normal * (sphere_collider.radius - surface_penetration_depth));

            surface_normal = -surface_normal;

            return true;
        }

        return false;
    }
}