using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MasterUI : MonoBehaviour
{
    [SerializeField] List<OpenButton> openButtons;
    [SerializeField] GameObject deactivatedMenus;
    public static UnityEvent ClosedWindow = new();
    private void Awake()
    {
        ClosedWindow.RemoveAllListeners();
        ClosedWindow.AddListener(OnWindowClose);
        foreach(var map in openButtons)
        {
            map.button.onClick.AddListener(() => TogglePanel(map.window));
        }
    }

    void TogglePanel(GameObject panel)
    {
        foreach (var map in openButtons) map.window.SetActive(false);
        panel.SetActive(true);
        deactivatedMenus.SetActive(false);
    }

    void OnWindowClose()
    {
        deactivatedMenus.SetActive(true);
    }

    [Serializable]
    class OpenButton
    {
        public Button button;
        public GameObject window;
    }
}
