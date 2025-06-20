using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFlowButton : MonoBehaviour
{
    [SerializeField] List<Window> targets;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnPress);
    }

    void OnPress()
    {
        foreach (var target in targets) target.target.SetActive(target.open);
    }

    [System.Serializable]
    struct Window
    {
        public GameObject target;
        public bool open;
    }
}
