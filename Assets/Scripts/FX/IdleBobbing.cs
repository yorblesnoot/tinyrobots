using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBobbing : MonoBehaviour
{
    Vector3 initialPosition;
    [SerializeField] float amplitude = 1;
    [SerializeField] float frequency = 1;
    private void Awake()
    {
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float height = Mathf.Sin(Time.time * frequency) * amplitude;
        Vector3 newPosition = initialPosition;
        newPosition.y += height;
        transform.position = newPosition;
    }
}
