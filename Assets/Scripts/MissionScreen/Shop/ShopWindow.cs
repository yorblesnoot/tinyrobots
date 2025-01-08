using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShopWindow : MonoBehaviour
{
    UnityAction closeCallback;
    [SerializeField] TradePanel buyPanel;
    [SerializeField] TradePanel sellPanel;
    private void Awake()
    {
        gameObject.SetActive(false);
    }
    public void Open(int location, UnityAction eventCallback)
    {
        gameObject.SetActive(true);
        closeCallback = eventCallback;
        buyPanel.Display(SceneGlobals.PlayerData.ShopData[location]);
        sellPanel.Display(SceneGlobals.PlayerData);
    }

    private void OnDisable()
    {
        closeCallback?.Invoke();
    }
}
