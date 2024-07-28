using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookAbility : ProjectileAbility
{
    [SerializeField] protected LineRenderer line;
    protected Vector3 baseHookPosition;
    protected Transform baseParent;
    private void Start()
    {
        baseHookPosition = projectile.transform.localPosition;
        line.useWorldSpace = true;
        baseParent = projectile.transform.parent;
    }
    protected IEnumerator LaunchWithLine(GameObject launched, List<Vector3> trajectory, float intervalTime)
    {
        float timeElapsed;
        launched.transform.rotation = Quaternion.LookRotation(trajectory[1] - trajectory[0]);
        for (int i = 0; i < trajectory.Count - 1; i++)
        {
            timeElapsed = 0;
            while (timeElapsed < intervalTime)
            {
                timeElapsed += Time.deltaTime;
                float interpolator = timeElapsed / intervalTime;
                launched.transform.position = Vector3.Lerp(trajectory[i], trajectory[i + 1], interpolator);
                Vector3[] linePoints = new Vector3[2] { emissionPoint.transform.position, projectile.transform.position };
                line.SetPositions(linePoints);
                yield return null;
            }
        }
    }

    public override void EndAbility()
    {
        line.positionCount = 0;
        projectile.transform.SetParent(baseParent);
        projectile.transform.localPosition = baseHookPosition;
        base.EndAbility();
    }
}
