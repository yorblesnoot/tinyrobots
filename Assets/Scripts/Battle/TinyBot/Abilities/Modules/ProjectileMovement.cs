using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProjectileMovement 
{
    public static IEnumerator LaunchAlongLine(GameObject launched, float travelTime, List<Vector3> currentTrajectory)
    {
        float intervalTime = travelTime / currentTrajectory.Count;
        float timeElapsed;
        launched.transform.rotation = Quaternion.LookRotation(currentTrajectory[1] - currentTrajectory[0]);
        for (int i = 0; i < currentTrajectory.Count - 1; i++)
        {
            Quaternion startRotation = launched.transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(currentTrajectory[i + 1] - currentTrajectory[i]);
            timeElapsed = 0;
            while (timeElapsed < intervalTime)
            {
                timeElapsed += Time.deltaTime;
                float interpolator = timeElapsed / intervalTime;
                launched.transform.SetPositionAndRotation(Vector3.Lerp(currentTrajectory[i], currentTrajectory[i + 1], interpolator),
                    Quaternion.Slerp(startRotation, targetRotation, interpolator));

                yield return null;
            }
        }
    }
}
