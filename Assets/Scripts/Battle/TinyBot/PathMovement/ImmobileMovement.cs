using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmobileMovement : PrimaryMovement
{
    [SerializeField] float ikOffset = .7f;
    [SerializeField] Transform ikTarget;
    [SerializeField] Transform rotatorBase;
    int terrainMask;
    Vector3 gluedPosition;
    Quaternion gluedRotation;

    public override float LocomotionHeight => locomotionHeight;
    [SerializeField] float locomotionHeight;

    private void Awake()
    {
        terrainMask = LayerMask.GetMask("Terrain");
        Style = MoveStyle.WALK;
    }

    public void AttachToChassis(TinyBot bot)
    {
        Owner = bot;
        int mask = LayerMask.GetMask("ParticleChassis");
        Vector3 direction = (Vector3.down - bot.transform.forward) * 2;
        Vector3 origin = bot.TargetPoint.position + direction;
        bool ray = Physics.Raycast(origin, -direction, out RaycastHit hit, 5, mask);
        Debug.DrawLine(origin, hit.point, Color.magenta, 10f);
        transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
        transform.SetParent(bot.TargetPoint, true);
    }

    public override IEnumerator NeutralStance()
    {
        Vector3 raySource = Owner.transform.position;
        Physics.Raycast(raySource, Vector3.down, out RaycastHit hit, 3, terrainMask);
        Debug.DrawRay(raySource, Vector3.down, Color.cyan, 100);
        gluedPosition = hit.point;
        gluedPosition.y += ikOffset;
        Vector3 lookDirection = Owner.transform.forward;
        lookDirection.y = 0;
        ikTarget.rotation = Quaternion.LookRotation(lookDirection, -hit.normal);
        HandleImpulse();
        yield return null;
        gluedRotation = rotatorBase.rotation;
    }

    protected override void HandleImpulse()
    {
        ikTarget.position = gluedPosition;
    }

    public override void AnimateToOrientation(Vector3 offset)
    {
        HandleImpulse();
        rotatorBase.rotation = gluedRotation;
    }

    protected override void InstantNeutral()
    {
        StartCoroutine(NeutralStance());
    }
}
