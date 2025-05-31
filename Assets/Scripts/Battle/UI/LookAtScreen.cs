using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtScreen : MonoBehaviour
{

    public static void Look(Transform transform)
    {
        Vector3 towardsCamera = -Camera.main.transform.forward;
        transform.rotation = Quaternion.LookRotation(towardsCamera);
    }

    void LateUpdate()
    {
        Look(transform);
    }
}
