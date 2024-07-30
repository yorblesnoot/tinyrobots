using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtScreen : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 towardsCamera = -Camera.main.transform.forward;
        transform.rotation = Quaternion.LookRotation(towardsCamera);
    }
}
