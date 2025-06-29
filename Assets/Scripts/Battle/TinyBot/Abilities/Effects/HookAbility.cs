using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HookAbility : AbilityEffect
{
    [SerializeField] protected LineRenderer line;
    [SerializeField] protected GameObject projectile;
    [SerializeField] protected float TravelSpeed = 10;
    protected Vector3 BaseHookPosition;
    protected Quaternion BaseHookRotation;
    protected Transform BaseParent;
    
    private void Start()
    {
        BaseHookPosition = projectile.transform.localPosition;
        BaseHookRotation = projectile.transform.localRotation;
        BaseParent = projectile.transform.parent;
        line.useWorldSpace = true;
    }
    protected IEnumerator LaunchWithLine(GameObject launched, List<Vector3> trajectory, float velocity, bool faceMove = true)
    {
        float travelTime = Vector3.Distance(trajectory[0], trajectory[^1]) / velocity;
        float intervalTime = travelTime / trajectory.Count;
        float timeElapsed;
        Vector3 baseDirection = trajectory[1] - trajectory[0];
        launched.transform.rotation = Quaternion.LookRotation(faceMove ? baseDirection : -baseDirection);
        for (int i = 0; i < trajectory.Count - 1; i++)
        {
            timeElapsed = 0;
            while (timeElapsed < intervalTime)
            {
                timeElapsed += Time.deltaTime;
                float interpolator = timeElapsed / intervalTime;
                launched.transform.position = Vector3.Lerp(trajectory[i], trajectory[i + 1], interpolator);
                Vector3[] linePoints = new Vector3[2] { Ability.emissionPoint.position, projectile.transform.position };
                line.SetPositions(linePoints);
                yield return null;
            }
        }
    }

    public void ResetHook()
    {
        line.positionCount = 0;
        projectile.transform.SetParent(BaseParent);
        projectile.transform.SetLocalPositionAndRotation(BaseHookPosition, BaseHookRotation);
    }
}
