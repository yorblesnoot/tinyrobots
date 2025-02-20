using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastOutcomeIndicator : MonoBehaviour
{
    static CastOutcomeIndicator instance;
    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public static void Show(Vector3 position, bool valid)
    {
        instance.gameObject.SetActive(!valid);
        instance.transform.position = position;
    }

    public static void Hide() { instance.gameObject.SetActive(false); }
}
