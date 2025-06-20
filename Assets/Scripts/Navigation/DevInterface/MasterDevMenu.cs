using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterDevMenu : MonoBehaviour
{
    GameObject menu;
    private void Awake()
    {
#if !UNITY_EDITOR
    
        gameObject.SetActive(false);
#endif
        menu = transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) menu.SetActive(!menu.activeSelf);
        else if (Input.GetKeyDown(KeyCode.F2)) SceneGlobals.PlayerData.DevMode = !SceneGlobals.PlayerData.DevMode;
    }
}
