using System.Collections;
using System.Linq;
using UnityEngine;

public static class PhysicsHelper
{
    static System.Random rand = new();
    public static IEnumerator LerpTo(this GameObject thing, Vector3 endPosition, float duration, bool local = false)
    {
        UnityEngine.Transform transform = thing.transform;
        float timeElapsed = 0;
        Vector3 startPosition = local ? transform.localPosition : transform.position;
        while (timeElapsed < duration)
        {
            Vector3 step = Vector3.Lerp(startPosition, endPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            if(local) transform.localPosition = step;
            else transform.position = step;
            yield return null;
        }
        if (local) transform.localPosition = endPosition;
        else transform.position = endPosition;
    }

    public static IEnumerator DampTo(this GameObject thing, Vector3 endPosition, float duration, bool local = false)
    {
        UnityEngine.Transform transform = thing.transform;
        float timeElapsed = 0;
        Vector3 velocity = Vector3.zero;
        while (timeElapsed < duration)
        {
            Vector3 step = Vector3.SmoothDamp(local ? thing.transform.localPosition : transform.position, endPosition, ref velocity, duration);
            timeElapsed += Time.deltaTime;
            if (local) transform.localPosition = step;
            else transform.position = step;
            yield return null;
        }
    }

    public static Quaternion RandomCardinalRotate()
    {
        int random = rand.Next(0, 5);
        Quaternion rot = Quaternion.Euler(0, random * 90, 0);
        return rot;
    }

    public static void ParabolicProjectile(this GameObject particleHolder, Vector3 targetPosition, float timeToHit)
    {
        Transform transform = particleHolder.transform;
        ParticleSystem[] projectiles = particleHolder.GetComponentsInChildren<ParticleSystem>();
        ParticleSystem.MainModule[] mainModules = projectiles.Select(x => x.main).ToArray();
        float gravity = projectiles[0].main.gravityModifierMultiplier * 9.8f;

        Vector3 localCoords = targetPosition - transform.position;
        Vector2 flatLocal = new(localCoords.x, localCoords.z);
        float horizontalDistance = flatLocal.magnitude;
        float verticalDistance = localCoords.y;

        Vector3 lookPosition = new(targetPosition.x, transform.position.y, targetPosition.z);
        transform.LookAt(lookPosition);

        float verticalVelocity = verticalDistance / timeToHit + gravity * timeToHit / 2;
        float horizontalVelocity = horizontalDistance / timeToHit;
        float combinedVelocity = Mathf.Sqrt(Mathf.Pow(horizontalVelocity, 2) + Mathf.Pow(verticalVelocity, 2));
        float compAngle = Mathf.Atan(verticalVelocity / horizontalVelocity);
        compAngle = Mathf.Rad2Deg * compAngle;
        Quaternion firingAngle = Quaternion.Euler(-Mathf.Abs(compAngle), 0, 0);
        transform.rotation *= firingAngle;
        for (int i = 0; i < mainModules.Length; i++)
        {
            mainModules[i].startSpeed = combinedVelocity;
        }
    }
}
