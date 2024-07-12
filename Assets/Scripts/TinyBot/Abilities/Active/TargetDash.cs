using System.Collections;
using System.Linq;
using UnityEngine;

public class TargetDash : ProjectileShot
{
    [SerializeField] float intervalTime = .2f;
    protected override IEnumerator PerformEffects()
    {
        foreach (Vector3 point in currentTrajectory)
        {
            yield return StartCoroutine(Owner.gameObject.LerpTo(point, intervalTime));
        }
        EndAbility();
        Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
    }

    public override bool IsUsable(Vector3 targetPosition)
    {
        if (Pathfinder3D.GetLandingPointBy(currentTrajectory[^1], MoveStyle.WALK, out Vector3Int nodeTarget))
        {
            currentTrajectory[^1] = nodeTarget;
            return true;
        }
        return false;
    }
}
