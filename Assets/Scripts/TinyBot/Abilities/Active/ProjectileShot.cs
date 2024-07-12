using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectileShot : ActiveAbility
{
    [SerializeField] GameObject projectile;
    [SerializeField] float travelTime;
    protected override IEnumerator PerformEffects()
    {
        yield return StartCoroutine(LaunchAlongLine(projectile, travelTime));
        NeutralAim();
    }

    protected IEnumerator LaunchAlongLine(GameObject launched, float travelTime)
    {
        GameObject spawned = launched.scene.name == null ? Instantiate(launched) : launched;
        float intervalTime = travelTime / currentTrajectory.Count;
        float timeElapsed;
        spawned.transform.rotation = Quaternion.LookRotation(currentTrajectory[1] - currentTrajectory[0]);
        for (int i = 0; i < currentTrajectory.Count - 1; i++)
        {
            Quaternion startRotation = spawned.transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(currentTrajectory[i + 1] - currentTrajectory[i]);
            timeElapsed = 0;
            while (timeElapsed < intervalTime)
            {
                timeElapsed += Time.deltaTime;
                float interpolator = timeElapsed / intervalTime;
                spawned.transform.SetPositionAndRotation(Vector3.Lerp(currentTrajectory[i], currentTrajectory[i + 1], interpolator),
                    Quaternion.Slerp(startRotation, targetRotation, interpolator));

                yield return null;
            }
        }
        CompleteTrajectory(currentTrajectory.Last(), spawned);
    }

    protected virtual void CompleteTrajectory(Vector3 position, GameObject launched)
    {
        Destroy(launched);
        foreach (var targetable in currentTargets)
        {
            targetable.ReceiveHit(damage, Owner.transform.position, currentTrajectory[^1]);
        }
    }
}
