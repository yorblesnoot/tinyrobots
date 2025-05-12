using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnAwake : MonoBehaviour
{
    private void Awake()
    {
        // Hide the GameObject when it is instantiated
        gameObject.SetActive(false);
    }
}
