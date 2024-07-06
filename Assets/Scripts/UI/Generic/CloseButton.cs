using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CloseButton : MonoBehaviour
{
    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Close);
    }

    void Close()
    {
        MasterUI.ClosedWindow.Invoke();
        gameObject.transform.parent.gameObject.SetActive(false);
    }

}
