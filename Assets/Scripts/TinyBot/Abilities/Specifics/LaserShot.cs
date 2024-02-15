using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShot : LinearAbility
{
    [SerializeField] GameObject laser;
    [SerializeField] float shotSpeed;
    [SerializeField] TurretTracker turretTracker;
    public override IEnumerator ExecuteAbility(Vector3 target)
    {
        Vector3 direction = (target - owner.ChassisPoint.position).normalized;
        GameObject shot = Instantiate(laser, emissionPoint.transform);
        shot.transform.SetParent(null);
        Rigidbody rigidbody = shot.GetComponent<Rigidbody>();
        rigidbody.velocity = direction * shotSpeed;
        yield break;
    }

    protected override void AimAt(GameObject target)
    {
        base.AimAt(target);
        turretTracker.TrackTarget(target);
    }
}
