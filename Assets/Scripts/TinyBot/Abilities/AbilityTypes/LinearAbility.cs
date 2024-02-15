using UnityEngine;

public abstract class LinearAbility : Ability
{
    protected override void AimAt(GameObject target)
    {
        Vector3 direction = (target.transform.position - emissionPoint.transform.position).normalized;
        Vector3 endPoint = emissionPoint.transform.position + direction * range;
        LineMaker.DrawLine(emissionPoint.transform.position, endPoint);
    }
}
