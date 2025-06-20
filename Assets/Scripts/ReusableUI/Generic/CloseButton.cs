using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CloseButton : MonoBehaviour
{
    [SerializeField] GameObject closeTarget;
    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Close);
    }

    void Close()
    {
        MasterUI.ClosedWindow.Invoke();
        GameObject target = closeTarget == null ? gameObject.transform.parent.gameObject : closeTarget;
        target.SetActive(false);
    }

}
