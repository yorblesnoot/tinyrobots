using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPoint : MonoBehaviour
{
    public SlotType SlotType;
    public bool ContainsSubTree = false;
    private void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
