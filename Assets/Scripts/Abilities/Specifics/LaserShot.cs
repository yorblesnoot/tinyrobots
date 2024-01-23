using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShot : LinearAbility
{
    [SerializeField] GameObject laser;
    [SerializeField] float shotSpeed;
    public override void ExecuteAbility(TinyBot user, Vector3 target)
    {
        Vector3 direction = (target - user.transform.position).normalized;
        GameObject shot = Instantiate(laser, emissionPoint.transform);
        shot.transform.SetParent(null);
        Rigidbody rigidbody = shot.GetComponent<Rigidbody>();
        rigidbody.velocity = direction * shotSpeed;
    }
}
