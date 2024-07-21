using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMask : MonoBehaviour
{
    [SerializeField] string[] maskLayers;
    private void Awake()
    {
        Camera camera = GetComponent<Camera>();
        int layermask = LayerMask.GetMask(maskLayers);
        camera.eventMask = layermask;
    }
}
