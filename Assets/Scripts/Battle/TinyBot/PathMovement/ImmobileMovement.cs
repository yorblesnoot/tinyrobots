using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmobileMovement : PrimaryMovement
{
    [SerializeField] Transform ikTarget;
    [SerializeField] Transform rotatorBase;
    int terrainMask;
    Vector3 gluedPosition;
    Quaternion gluedRotation;
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
        Debug.Log(direction + " dir");
        Debug.Log(bot.TargetPoint.position);
        Vector3 origin = bot.TargetPoint.position + direction;
        Debug.Log(origin);
        Physics.Raycast(origin, -direction, out RaycastHit hit, 5, mask);
        Debug.DrawLine(origin, origin - direction, Color.magenta, 10f);
        transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
    }

    public override IEnumerator NeutralStance()
    {
        Vector3 raySource = Owner.transform.position;
        Physics.Raycast(raySource, Vector3.down, out RaycastHit hit, 3, terrainMask);
        gluedPosition = hit.point;
        HandleImpulse();
        yield break;
    }

    protected override void HandleImpulse()
    {
        ikTarget.position = gluedPosition;
    }
}
