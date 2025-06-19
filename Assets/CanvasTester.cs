using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasTester : MonoBehaviour
{
    void Update()
    {
        Debug.Log(Input.mousePosition + " raw mouse");
        Debug.Log(Input.mousePosition + " mouse position");
    }
}
