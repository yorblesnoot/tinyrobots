using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBounds : MonoBehaviour
{
    [SerializeField] Transform outerCorner;
    private void Awake()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
        outerCorner.GetComponent<Renderer>().enabled = false;
    }

    public Vector3Int GetMapSize()
    {
        return Vector3Int.FloorToInt(outerCorner.position);
    }

    readonly Vector3[] directions = { Vector3.up, Vector3.down, Vector3.back, Vector3.forward, Vector3.left, Vector3.right };

    private void OnDrawGizmos()
    {
        foreach (var direction in directions)
        {
            Gizmos.DrawRay(outerCorner.position, direction * 100);
        }
        
    }
}
